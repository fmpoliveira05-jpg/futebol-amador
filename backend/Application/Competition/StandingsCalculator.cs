using Domain.Constants;

namespace Application.Competition
{
    /// <summary>Resultado de um jogo terminado, visto pela classificação.</summary>
    public sealed record FinishedMatch(Guid HomeTeamId, Guid AwayTeamId, int HomeGoals, int AwayGoals, DateTime Date);

    /// <summary>Linha da classificação calculada.</summary>
    public sealed class StandingRow
    {
        public Guid TeamId { get; init; }
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference => GoalsFor - GoalsAgainst;
        public int Points => Won * ModelConstants.LeagueConst.PointsWin
                             + Drawn * ModelConstants.LeagueConst.PointsDraw
                             + Lost * ModelConstants.LeagueConst.PointsLoss;

        /// <summary>Últimos resultados ("V", "E", "D"), do mais antigo para o mais recente.</summary>
        public List<string> Form { get; } = new();
    }

    /// <summary>
    /// Calcula a classificação de uma época a partir dos jogos terminados (ver ADR D2).
    /// Vitória 3, empate 1, derrota 0. Desempate: pontos, diferença de golos, golos marcados e, por fim,
    /// a ordem recebida em <paramref name="teamOrder"/> (normalmente o nome).
    /// </summary>
    public static class StandingsCalculator
    {
        public static List<StandingRow> Calculate(IReadOnlyList<Guid> teamOrder, IEnumerable<FinishedMatch> matches)
        {
            var rows = teamOrder.Distinct().ToDictionary(id => id, id => new StandingRow { TeamId = id });
            var form = teamOrder.Distinct().ToDictionary(id => id, _ => new List<(DateTime Date, string R)>());

            foreach (var m in matches.OrderBy(m => m.Date))
            {
                // Jogos com equipas que já não estão inscritas (por exemplo, apagadas) não contam.
                if (!rows.TryGetValue(m.HomeTeamId, out var home) || !rows.TryGetValue(m.AwayTeamId, out var away))
                {
                    continue;
                }

                home.Played++;
                away.Played++;
                home.GoalsFor += m.HomeGoals;
                home.GoalsAgainst += m.AwayGoals;
                away.GoalsFor += m.AwayGoals;
                away.GoalsAgainst += m.HomeGoals;

                if (m.HomeGoals > m.AwayGoals)
                {
                    home.Won++;
                    away.Lost++;
                    form[home.TeamId].Add((m.Date, "V"));
                    form[away.TeamId].Add((m.Date, "D"));
                }
                else if (m.HomeGoals < m.AwayGoals)
                {
                    away.Won++;
                    home.Lost++;
                    form[home.TeamId].Add((m.Date, "D"));
                    form[away.TeamId].Add((m.Date, "V"));
                }
                else
                {
                    home.Drawn++;
                    away.Drawn++;
                    form[home.TeamId].Add((m.Date, "E"));
                    form[away.TeamId].Add((m.Date, "E"));
                }
            }

            foreach (var (teamId, results) in form)
            {
                rows[teamId].Form.AddRange(results
                    .TakeLast(ModelConstants.LeagueConst.FormLength)
                    .Select(r => r.R));
            }

            var order = teamOrder.Distinct().Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);

            return rows.Values
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.GoalDifference)
                .ThenByDescending(r => r.GoalsFor)
                .ThenBy(r => order[r.TeamId])
                .ToList();
        }

        /// <summary>
        /// Zona de uma posição (1 = primeiro): "PROMOTION", "RELEGATION" ou nulo.
        /// Numa liga pequena as zonas nunca se sobrepõem: a subida tem prioridade.
        /// </summary>
        public static string? Zone(int position, int teamCount, int promotionSpots, int relegationSpots, bool hasLeagueAbove, bool hasLeagueBelow)
        {
            if (hasLeagueAbove && position <= promotionSpots)
            {
                return "PROMOTION";
            }

            if (hasLeagueBelow && position > teamCount - relegationSpots && position > promotionSpots)
            {
                return "RELEGATION";
            }

            return null;
        }
    }
}
