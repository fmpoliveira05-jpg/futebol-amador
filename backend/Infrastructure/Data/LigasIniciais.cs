using Application.Competition;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    /// <summary>
    /// Passagem das divisões para ligas (ver docs/novas-funcionalidades.md, D1 e D9). Corre no arranque e só
    /// faz alguma coisa numa base de dados que ainda não tem ligas:
    /// <list type="number">
    /// <item>cria uma liga por divisão, com o mesmo nome (a "Divisão 1" é o escalão 1);</item>
    /// <item>põe cada equipa na liga da sua divisão;</item>
    /// <item>marca como administrador principal o administrador mais antigo das equipas que ainda não o têm;</item>
    /// <item>abre as inscrições da primeira época de cada liga, a começar no sábado a seguir a uma semana.</item>
    /// </list>
    /// </summary>
    public static class LigasIniciais
    {
        /// <summary>Configuração por omissão de cada liga criada a partir de uma divisão.</summary>
        public const int Promocoes = 2;
        public const int Descidas = 2;
        public const int DuracaoDias = 120;

        public static async Task GarantirAsync(AmateurFootballContext db, DateTime agoraUtc, CancellationToken ct = default)
        {
            await AdministradoresPrincipaisAsync(db, ct);

            if (await db.League.AnyAsync(ct))
            {
                return;
            }

            var divisoes = await db.Rank.ToListAsync(ct);
            if (divisoes.Count == 0)
            {
                return;
            }

            // "Divisão 1" é a mais alta; divisões sem número ficam no fim, pela ordem do nome.
            var ordenadas = divisoes
                .OrderBy(r => NumeroDaDivisao(r.Name) ?? int.MaxValue)
                .ThenBy(r => r.Name)
                .ToList();

            var inicio = ProximoSabado(agoraUtc.Date.AddDays(7));
            var ligaPorDivisao = new Dictionary<Guid, League>();
            for (var i = 0; i < ordenadas.Count; i++)
            {
                var liga = new League
                {
                    Name = ordenadas[i].Name,
                    Level = i + 1,
                    PromotionSpots = Promocoes,
                    RelegationSpots = Descidas,
                    SeasonDurationDays = DuracaoDias,
                    TrophyName = $"Troféu {ordenadas[i].Name}",
                };
                ligaPorDivisao[ordenadas[i].Id] = liga;
                db.League.Add(liga);
            }

            var equipas = await db.Team.ToListAsync(ct);
            foreach (var equipa in equipas)
            {
                if (ligaPorDivisao.TryGetValue(equipa.IdRank, out var liga))
                {
                    equipa.IdLeague = liga.Id;
                }
                else
                {
                    equipa.IdLeague = ligaPorDivisao.Values.OrderByDescending(l => l.Level).First().Id;
                }
            }

            foreach (var liga in ligaPorDivisao.Values)
            {
                var epoca = new Season
                {
                    IdLeague = liga.Id,
                    Name = PlayerStatsCalculator.SportsSeason(inicio),
                    Status = SeasonStatus.REGISTRATION,
                    StartDate = inicio,
                    EndDate = inicio.AddDays(liga.SeasonDurationDays),
                };
                db.Season.Add(epoca);

                foreach (var equipa in equipas.Where(e => e.IdLeague == liga.Id))
                {
                    db.SeasonTeam.Add(new SeasonTeam { IdSeason = epoca.Id, IdTeam = equipa.Id });
                }
            }

            await db.SaveChangesAsync(ct);
        }

        private static async Task AdministradoresPrincipaisAsync(AmateurFootballContext db, CancellationToken ct)
        {
            var semPrincipal = await db.Team.Include(t => t.Members).Where(t => t.CreatorId == null).ToListAsync(ct);
            foreach (var equipa in semPrincipal)
            {
                equipa.CreatorId = equipa.Members
                    .Where(m => m.IsAdmin)
                    .OrderBy(m => m.IsAdminLastChangedAt ?? DateTime.MaxValue)
                    .ThenBy(m => m.Id, StringComparer.Ordinal)
                    .FirstOrDefault()?.Id;
            }

            if (semPrincipal.Count > 0)
            {
                await db.SaveChangesAsync(ct);
            }
        }

        private static int? NumeroDaDivisao(string nome)
        {
            var digitos = new string(nome.Where(char.IsDigit).ToArray());
            return int.TryParse(digitos, out var n) ? n : null;
        }

        private static DateTime ProximoSabado(DateTime data)
        {
            var dias = ((int)DayOfWeek.Saturday - (int)data.DayOfWeek + 7) % 7;
            return DateTime.SpecifyKind(data.AddDays(dias), DateTimeKind.Utc);
        }
    }
}
