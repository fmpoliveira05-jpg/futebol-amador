using Domain.Constants;
using Domain.Enums;

namespace Application.Competition
{
    /// <summary>Evento de um jogo, na forma que o cálculo precisa.</summary>
    public sealed record EventInfo(MatchEventType Type, int? Minute, string? PlayerId, string? RelatedPlayerId);

    /// <summary>
    /// Um jogo terminado da equipa em que o jogador jogou ou podia ter jogado.
    /// </summary>
    /// <param name="Season">Época ("2026/27"): a da liga, ou a época desportiva da data nos amigáveis.</param>
    /// <param name="IsStarter">O jogador foi titular.</param>
    /// <param name="HasLineup">A equipa tinha onze registado neste jogo.</param>
    /// <param name="TeamEvents">Eventos da equipa do jogador neste jogo.</param>
    public sealed record PlayerMatch(Guid MatchId, DateTime Date, string Season, Guid TeamId, string TeamName,
        bool IsStarter, bool HasLineup, IReadOnlyList<EventInfo> TeamEvents);

    /// <summary>Totais de um jogador num conjunto de jogos.</summary>
    public class PlayerTotals
    {
        public int Games { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int Minutes { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
    }

    /// <summary>Linha do histórico por época e equipa (como no ZeroZero).</summary>
    public sealed class CareerLine : PlayerTotals
    {
        public string Season { get; init; } = null!;
        public Guid TeamId { get; init; }
        public string TeamName { get; init; } = null!;
    }

    /// <summary>
    /// Estatísticas de um jogador a partir dos onzes e dos eventos dos jogos (ver ADR D8).
    /// </summary>
    public static class PlayerStatsCalculator
    {
        /// <summary>
        /// Minutos jogados num jogo: um titular joga desde o minuto 0 e um suplente desde que entra; sai no
        /// minuto da substituição ou da expulsão, ou no fim (90). Devolve nulo se o jogador não jogou.
        /// </summary>
        public static int? MinutesPlayed(string playerId, bool isStarter, IReadOnlyList<EventInfo> events)
        {
            const int end = ModelConstants.LineupConst.MatchMinutes;
            int? entered = isStarter ? 0 : null;

            if (entered == null)
            {
                var subIn = events
                    .Where(e => e.Type == MatchEventType.SUBSTITUTION && e.RelatedPlayerId == playerId)
                    .OrderBy(e => e.Minute ?? end)
                    .FirstOrDefault();
                if (subIn == null)
                {
                    return null;
                }

                entered = Clamp(subIn.Minute ?? end);
            }

            var left = end;
            foreach (var e in events)
            {
                var leaves = (e.Type == MatchEventType.SUBSTITUTION || e.Type == MatchEventType.RED_CARD)
                             && e.PlayerId == playerId;
                if (leaves)
                {
                    var minute = Clamp(e.Minute ?? end);
                    if (minute >= entered.Value)
                    {
                        left = Math.Min(left, minute);
                    }
                }
            }

            return Math.Max(0, left - entered.Value);
        }

        /// <summary>Histórico por época e equipa, da época mais recente para a mais antiga.</summary>
        public static List<CareerLine> Career(string playerId, IEnumerable<PlayerMatch> matches)
        {
            var lines = new Dictionary<(string Season, Guid TeamId), CareerLine>();

            foreach (var m in matches.OrderBy(m => m.Date))
            {
                var mine = m.TeamEvents.Where(e => e.PlayerId == playerId || e.RelatedPlayerId == playerId).ToList();
                var minutes = MinutesPlayed(playerId, m.IsStarter, m.TeamEvents);

                // Sem onze registado (jogos antigos), conta o jogo se o jogador aparece nos eventos.
                var played = minutes.HasValue || (!m.HasLineup && mine.Count > 0);
                var goals = m.TeamEvents.Count(e => e.Type == MatchEventType.GOAL && e.PlayerId == playerId);
                var assists = m.TeamEvents.Count(e => e.Type == MatchEventType.GOAL && e.RelatedPlayerId == playerId);
                var yellow = m.TeamEvents.Count(e => e.Type == MatchEventType.YELLOW_CARD && e.PlayerId == playerId);
                var red = m.TeamEvents.Count(e => e.Type == MatchEventType.RED_CARD && e.PlayerId == playerId);

                if (!played && goals + assists + yellow + red == 0)
                {
                    continue;
                }

                var key = (m.Season, m.TeamId);
                if (!lines.TryGetValue(key, out var line))
                {
                    line = new CareerLine { Season = m.Season, TeamId = m.TeamId, TeamName = m.TeamName };
                    lines[key] = line;
                }

                if (played)
                {
                    line.Games++;
                }

                line.Minutes += minutes ?? 0;
                line.Goals += goals;
                line.Assists += assists;
                line.YellowCards += yellow;
                line.RedCards += red;
            }

            return lines.Values
                .OrderByDescending(l => l.Season, StringComparer.Ordinal)
                .ThenBy(l => l.TeamName, StringComparer.CurrentCulture)
                .ToList();
        }

        /// <summary>Soma das linhas do histórico.</summary>
        public static PlayerTotals Totals(IEnumerable<CareerLine> career)
        {
            var t = new PlayerTotals();
            foreach (var l in career)
            {
                t.Games += l.Games;
                t.Goals += l.Goals;
                t.Assists += l.Assists;
                t.Minutes += l.Minutes;
                t.YellowCards += l.YellowCards;
                t.RedCards += l.RedCards;
            }

            return t;
        }

        /// <summary>Época desportiva de uma data: de agosto a julho ("2026/27").</summary>
        public static string SportsSeason(DateTime date)
        {
            var start = date.Month >= 8 ? date.Year : date.Year - 1;
            return $"{start}/{(start + 1) % 100:00}";
        }

        private static int Clamp(int minute) => Math.Clamp(minute, 0, ModelConstants.LineupConst.MatchMinutes);
    }
}
