namespace Application.Competition
{
    /// <summary>Um jogo sorteado: jornada, casa e fora.</summary>
    public sealed record Fixture(int Round, Guid HomeTeamId, Guid AwayTeamId);

    /// <summary>
    /// Sorteio de uma liga a duas voltas pelo método do círculo (ver ADR D3).
    /// </summary>
    public static class FixtureGenerator
    {
        /// <summary>
        /// Gera o calendário completo: 2(n−1) jornadas (n arredondado para par), cada equipa joga com todas as
        /// outras uma vez em casa e outra fora. A segunda volta repete a primeira com casa e fora trocados.
        /// Nenhuma equipa joga mais de duas jornadas seguidas em casa ou fora (verificado nos testes para 2 a 20 equipas).
        /// </summary>
        /// <param name="teams">Equipas inscritas (pelo menos 2).</param>
        /// <param name="random">Gerador usado para baralhar a ordem (injetável nos testes).</param>
        public static List<Fixture> DoubleRoundRobin(IReadOnlyList<Guid> teams, Random random)
        {
            if (teams.Count < 2)
            {
                throw new ArgumentException("São precisas pelo menos duas equipas para sortear um calendário.", nameof(teams));
            }

            if (teams.Distinct().Count() != teams.Count)
            {
                throw new ArgumentException("Há equipas repetidas.", nameof(teams));
            }

            // Baralha (Fisher–Yates) para o sorteio não depender da ordem de inscrição.
            var list = teams.ToList();
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            // Número ímpar: uma "folga" (Guid.Empty) em cada jornada.
            if (list.Count % 2 == 1)
            {
                list.Add(Guid.Empty);
            }

            var n = list.Count;
            var rounds = n - 1;
            var firstLeg = new List<Fixture>();

            // Método do círculo (polígono): a última equipa fica ao centro e joga com a equipa r; as restantes
            // emparelham-se simetricamente à volta de r. A equipa do centro alterna casa e fora; nos outros
            // pares a casa depende da distância k, o que faz cada equipa alternar quase sempre.
            for (var r = 0; r < rounds; r++)
            {
                var pairs = new List<(Guid A, Guid B, int K)> { (list[n - 1], list[r], 0) };
                for (var k = 1; k < n / 2; k++)
                {
                    pairs.Add((list[(r + k) % rounds], list[(r - k + rounds) % rounds], k));
                }

                foreach (var (a, b, k) in pairs)
                {
                    if (a == Guid.Empty || b == Guid.Empty)
                    {
                        continue;
                    }

                    var aAtHome = k == 0 ? r % 2 == 0 : k % 2 == 1;
                    firstLeg.Add(aAtHome ? new Fixture(r + 1, a, b) : new Fixture(r + 1, b, a));
                }
            }

            // Segunda volta: casa e fora trocados, começando pela 2.ª jornada da primeira volta e acabando na 1.ª.
            // Esta ordem evita três jogos seguidos no mesmo sítio na passagem entre as voltas.
            var secondLeg = firstLeg.Select(f =>
                new Fixture(rounds + ((f.Round - 2 + rounds) % rounds) + 1, f.AwayTeamId, f.HomeTeamId));

            return firstLeg.Concat(secondLeg).ToList();
        }

        /// <summary>
        /// Data de cada jornada: espaça as jornadas de forma a caberem na duração da época (mínimo 1 dia entre jornadas).
        /// </summary>
        public static DateTime RoundDate(DateTime firstRoundDate, int round, int totalRounds, int durationDays, TimeSpan kickoff)
        {
            var interval = totalRounds <= 1 ? 7 : Math.Max(1, (durationDays - 1) / totalRounds);
            interval = Math.Min(interval, 7);
            return firstRoundDate.Date.AddDays((round - 1) * interval) + kickoff;
        }
    }
}
