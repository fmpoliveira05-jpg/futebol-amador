using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

// Ferramenta de operação do Futebol Amador.
//
//   dotnet run -c Release -- semear "<ligação SQL>" [equipas=120] [jogadoresPorEquipa=12] [livres=600]
//   dotnet run -c Release -- carga  <url base> [utilizadores=100] [segundos=30] [relatorio.json]
//
// "semear" cria divisões, ligas, equipas, jogadores, uma época a decorrer e jogos terminados numa base
// de dados de teste (nunca apontar para produção). "carga" simula utilizadores virtuais em paralelo nos
// endpoints públicos mais usados e mostra p50/p95/p99 e erros por endpoint.

if (args.Length < 2)
{
    Console.Error.WriteLine("Uso: semear <ligação> [equipas] [jogadoresPorEquipa] [livres] | carga <url> [utilizadores] [segundos] [relatorio.json]");
    return 2;
}

return args[0] switch
{
    "semear" => await Semear(args[1], Int(args, 2, 120), Int(args, 3, 12), Int(args, 4, 600)),
    "carga" => await Carga(args[1].TrimEnd('/'), Int(args, 2, 100), Int(args, 3, 30), args.Length > 4 ? args[4] : null),
    _ => 2,
};

static int Int(string[] a, int i, int predefinido) => a.Length > i ? int.Parse(a[i], CultureInfo.InvariantCulture) : predefinido;

static async Task<int> Semear(string ligacao, int numEquipas, int porEquipa, int livres)
{
    var opcoes = new DbContextOptionsBuilder<AmateurFootballContext>().UseSqlServer(ligacao).Options;
    await using var db = new AmateurFootballContext(opcoes);
    await db.Database.MigrateAsync();
    await DivisoesIniciais.GarantirAsync(db);
    await LigasIniciais.GarantirAsync(db, DateTime.UtcNow);

    if (await db.Team.AnyAsync())
    {
        Console.WriteLine("A base de dados já tem equipas: nada a semear.");
        return 0;
    }

    var aleatorio = new Random(2026);
    var rank = await db.Rank.OrderBy(r => r.Name).FirstAsync();
    var ligas = await db.League.OrderBy(l => l.Level).ToListAsync();
    var cidades = new[] { "Guimarães", "Braga", "Porto", "Viana do Castelo", "Barcelos", "Famalicão", "Fafe", "Felgueiras" };
    var posicoes = Enum.GetValues<Position>();
    var equipas = new List<Team>();
    var n = 0;

    for (var e = 0; e < numEquipas; e++)
    {
        var cidade = cidades[e % cidades.Length];
        var equipa = new Team($"Equipa {e + 1:D3} de {cidade}", "Equipa de teste de carga", "",
            new Pitch($"Campo {e + 1}", $"Rua do Campo {e + 1}, {cidade}"), rank)
        {
            IdLeague = ligas[e % ligas.Count].Id,
            CurrentPoints = aleatorio.Next(0, 60),
        };
        for (var j = 0; j < porEquipa; j++)
        {
            n++;
            var jogador = NovoJogador(n, cidade, posicoes, aleatorio);
            jogador.IdTeam = equipa.Id;
            jogador.IsAdmin = j == 0;
            equipa.Members.Add(jogador);
            if (j == 0)
            {
                equipa.CreatorId = jogador.Id;
            }
        }
        equipas.Add(equipa);
    }
    db.Team.AddRange(equipas);

    for (var l = 0; l < livres; l++)
    {
        n++;
        db.Player.Add(NovoJogador(n, cidades[l % cidades.Length], posicoes, aleatorio));
    }
    await db.SaveChangesAsync();

    // Uma época a decorrer em cada liga, com jogos terminados (a classificação faz as contas).
    foreach (var liga in ligas)
    {
        var daLiga = equipas.Where(t => t.IdLeague == liga.Id).ToList();
        var epoca = new Season
        {
            IdLeague = liga.Id, Name = "2026/27 (carga)", Status = SeasonStatus.IN_PROGRESS,
            StartDate = DateTime.UtcNow.AddDays(-30), EndDate = DateTime.UtcNow.AddDays(60),
        };
        foreach (var t in daLiga)
        {
            epoca.Teams.Add(new SeasonTeam { IdTeam = t.Id });
        }
        db.Season.Add(epoca);

        var ronda = 0;
        for (var i = 0; i + 1 < daLiga.Count; i += 2)
        {
            for (var r = 0; r < 4; r++)
            {
                var casa = daLiga[i];
                var fora = daLiga[(i + 1 + r * 2) % daLiga.Count];
                if (casa.Id == fora.Id)
                {
                    continue;
                }
                var golosCasa = aleatorio.Next(0, 5);
                var golosFora = aleatorio.Next(0, 5);
                var jogo = new Matches(DateTime.UtcNow.AddDays(-25 + r * 5), true, casa.IdPitch,
                    new List<TeamStatistics>
                    {
                        new() { IdTeam = casa.Id, NumGoals = golosCasa, MatchResult = Resultado(golosCasa, golosFora) },
                        new() { IdTeam = fora.Id, NumGoals = golosFora, MatchResult = Resultado(golosFora, golosCasa) },
                    }, new Chat())
                {
                    MatchStatus = MatchStatus.DONE, IdSeason = epoca.Id, Round = ++ronda, IdHomeTeam = casa.Id,
                };
                db.Match.Add(jogo);
            }
        }
    }
    await db.SaveChangesAsync();

    Console.WriteLine($"Semeado: {equipas.Count} equipas, {n} jogadores ({livres} sem equipa), {await db.Match.CountAsync()} jogos, {ligas.Count} ligas.");
    return 0;
}

static MatchResult Resultado(int a, int b) => a > b ? MatchResult.WIN : a < b ? MatchResult.LOSE : MatchResult.DRAW;

static Player NovoJogador(int n, string cidade, Position[] posicoes, Random aleatorio) =>
    new($"carga{n:D6}", $"Jogador {n:D5}", new DateOnly(1980 + aleatorio.Next(0, 25), 1 + aleatorio.Next(0, 12), 1 + aleatorio.Next(0, 28)),
        $"Rua {n}, {cidade}", $"jogador{n:D6}@carga.invalid", $"+3519{n:D8}", posicoes[n % posicoes.Length], 165 + aleatorio.Next(0, 30), null)
    {
        CreationDate = DateTime.UtcNow.AddDays(-aleatorio.Next(1, 400)),
    };

static async Task<int> Carga(string url, int utilizadores, int segundos, string? relatorio)
{
    using var http = new HttpClient(new SocketsHttpHandler { MaxConnectionsPerServer = utilizadores * 2, PooledConnectionLifetime = TimeSpan.FromMinutes(5) })
    {
        BaseAddress = new Uri(url + "/"),
        Timeout = TimeSpan.FromSeconds(30),
    };

    // Dados reais para os endpoints com ids.
    var ligasJson = JsonDocument.Parse(await http.GetStringAsync("api/leagues")).RootElement;
    var ligas = ligasJson.EnumerateArray().Select(l => l.GetProperty("id").GetString()!).ToArray();
    var equipasJson = JsonDocument.Parse(await http.GetStringAsync("api/Team/listTeams?pageSize=100")).RootElement;
    var equipas = equipasJson.EnumerateArray().Select(t => t.GetProperty("id").GetString()!).ToArray();
    if (ligas.Length == 0 || equipas.Length == 0)
    {
        Console.Error.WriteLine("Sem ligas ou equipas: correr primeiro 'semear'.");
        return 1;
    }

    // Peso de cada endpoint (aproximação do uso real: classificação e listas são o mais visto).
    var cenarios = new (string Nome, int Peso, Func<Random, string> Url)[]
    {
        ("GET /api/leagues", 15, _ => "api/leagues"),
        ("GET /api/Leaderboard", 20, _ => "api/Leaderboard"),
        ("GET /api/leagues/{id}/standings", 20, r => $"api/leagues/{ligas[r.Next(ligas.Length)]}/standings"),
        ("GET /api/Team/listTeams", 15, r => $"api/Team/listTeams?page={r.Next(1, 3)}&pageSize=50"),
        ("GET /api/Team/{id}", 15, r => $"api/Team/{equipas[r.Next(equipas.Length)]}"),
        ("GET /api/Team/{id}/titles", 5, r => $"api/Team/{equipas[r.Next(equipas.Length)]}/titles"),
        ("GET /api/lineups/formations", 5, _ => "api/lineups/formations"),
        ("GET /health/ready", 5, _ => "health/ready"),
    };
    var total = cenarios.Sum(c => c.Peso);
    var tempos = cenarios.ToDictionary(c => c.Nome, _ => new ConcurrentBag<double>());
    var erros = cenarios.ToDictionary(c => c.Nome, _ => new ConcurrentDictionary<string, int>());

    Console.WriteLine($"Carga: {utilizadores} utilizadores virtuais durante {segundos} s contra {url}");
    var fim = DateTime.UtcNow.AddSeconds(segundos);
    var relogio = Stopwatch.StartNew();

    var tarefas = Enumerable.Range(0, utilizadores).Select(async vu =>
    {
        var r = new Random(vu);
        while (DateTime.UtcNow < fim)
        {
            var escolha = r.Next(total);
            var c = cenarios.First(x => (escolha -= x.Peso) < 0);
            var inicio = Stopwatch.GetTimestamp();
            string? erro = null;
            try
            {
                using var resposta = await http.GetAsync(c.Url(r), HttpCompletionOption.ResponseContentRead);
                if (!resposta.IsSuccessStatusCode)
                {
                    erro = ((int)resposta.StatusCode).ToString(CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                erro = ex.GetType().Name;
            }
            tempos[c.Nome].Add(Stopwatch.GetElapsedTime(inicio).TotalMilliseconds);
            if (erro != null)
            {
                erros[c.Nome].AddOrUpdate(erro, 1, (_, v) => v + 1);
            }
        }
    });
    await Task.WhenAll(tarefas);
    var duracao = relogio.Elapsed.TotalSeconds;

    static double P(List<double> ordenados, double p) =>
        ordenados.Count == 0 ? 0 : ordenados[Math.Min(ordenados.Count - 1, (int)Math.Ceiling(p / 100 * ordenados.Count) - 1)];

    var linhas = new List<object>();
    Console.WriteLine($"{"Endpoint",-34} {"pedidos",8} {"p50 ms",8} {"p95 ms",8} {"p99 ms",8} {"máx ms",8} erros");
    foreach (var c in cenarios.Select(c => c.Nome).Append("TOTAL"))
    {
        var lista = (c == "TOTAL" ? tempos.Values.SelectMany(v => v) : tempos[c]).OrderBy(x => x).ToList();
        var e = c == "TOTAL"
            ? erros.Values.SelectMany(d => d).GroupBy(k => k.Key).ToDictionary(g => g.Key, g => g.Sum(x => x.Value))
            : erros[c].ToDictionary(k => k.Key, k => k.Value);
        var textoErros = e.Count == 0 ? "0" : string.Join(",", e.Select(k => $"{k.Key}×{k.Value}"));
        Console.WriteLine($"{c,-34} {lista.Count,8} {P(lista, 50),8:F1} {P(lista, 95),8:F1} {P(lista, 99),8:F1} {(lista.Count > 0 ? lista[^1] : 0),8:F1} {textoErros}");
        linhas.Add(new { endpoint = c, pedidos = lista.Count, p50 = P(lista, 50), p95 = P(lista, 95), p99 = P(lista, 99), max = lista.Count > 0 ? lista[^1] : 0, erros = e });
    }
    var totalPedidos = tempos.Values.Sum(v => v.Count);
    Console.WriteLine($"Débito: {totalPedidos / duracao:F0} pedidos/s em {duracao:F1} s");

    if (relatorio != null)
    {
        await File.WriteAllTextAsync(relatorio, JsonSerializer.Serialize(new { url, utilizadores, segundos, pedidosPorSegundo = totalPedidos / duracao, linhas },
            new JsonSerializerOptions { WriteIndented = true }));
    }

    return erros.Values.Any(d => !d.IsEmpty) ? 1 : 0;
}
