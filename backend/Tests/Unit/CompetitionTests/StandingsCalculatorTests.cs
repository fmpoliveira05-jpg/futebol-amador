using Application.Competition;
using NUnit.Framework;

namespace Tests.Unit.CompetitionTests
{
    [TestFixture]
    public class StandingsCalculatorTests
    {
        private static readonly Guid A = Guid.Parse("00000000-0000-0000-0000-00000000000a");
        private static readonly Guid B = Guid.Parse("00000000-0000-0000-0000-00000000000b");
        private static readonly Guid C = Guid.Parse("00000000-0000-0000-0000-00000000000c");

        private static FinishedMatch M(Guid home, Guid away, int hg, int ag, int day) =>
            new(home, away, hg, ag, new DateTime(2026, 10, day, 15, 0, 0, DateTimeKind.Utc));

        [Test(Description = "Vitória vale 3, empate 1 e derrota 0; PD, V, E, D, GM, GS e DG batem certo.")]
        public void Calculate_CountsPointsAndGoals()
        {
            var rows = StandingsCalculator.Calculate(new[] { A, B, C }, new[]
            {
                M(A, B, 2, 0, 1),   // A vence
                M(B, C, 1, 1, 2),   // empate
                M(C, A, 3, 1, 3),   // C vence
            });

            var a = rows.Single(r => r.TeamId == A);
            Assert.Multiple(() =>
            {
                Assert.That(a.Played, Is.EqualTo(2));
                Assert.That((a.Won, a.Drawn, a.Lost), Is.EqualTo((1, 0, 1)));
                Assert.That((a.GoalsFor, a.GoalsAgainst, a.GoalDifference), Is.EqualTo((3, 3, 0)));
                Assert.That(a.Points, Is.EqualTo(3));
                Assert.That(rows.Single(r => r.TeamId == B).Points, Is.EqualTo(1));
                Assert.That(rows.Single(r => r.TeamId == C).Points, Is.EqualTo(4));
            });
        }

        [Test(Description = "Desempate: pontos, depois diferença de golos, depois golos marcados, depois a ordem dada (nome).")]
        public void Calculate_OrdersByPointsThenGoalDifferenceThenGoalsFor()
        {
            var rows = StandingsCalculator.Calculate(new[] { A, B, C }, new[]
            {
                M(A, C, 1, 0, 1),
                M(B, C, 3, 1, 2),   // B e A com 3 pontos; B tem melhor DG
            });

            Assert.That(rows.Select(r => r.TeamId), Is.EqualTo(new[] { B, A, C }));
        }

        [Test(Description = "Sem jogos, a ordem é a recebida (alfabética) e todos têm zero.")]
        public void Calculate_WithoutMatches_KeepsGivenOrder()
        {
            var rows = StandingsCalculator.Calculate(new[] { C, A, B }, Array.Empty<FinishedMatch>());
            Assert.That(rows.Select(r => r.TeamId), Is.EqualTo(new[] { C, A, B }));
            Assert.That(rows.All(r => r.Points == 0 && r.Played == 0), Is.True);
        }

        [Test(Description = "A forma tem no máximo 5 resultados, do mais antigo para o mais recente.")]
        public void Calculate_FormKeepsLastFiveInChronologicalOrder()
        {
            var matches = new List<FinishedMatch>
            {
                M(A, B, 0, 1, 1), // D
                M(A, B, 1, 1, 2), // E
                M(A, B, 2, 0, 3), // V
                M(A, B, 2, 0, 4), // V
                M(A, B, 0, 3, 5), // D
                M(A, B, 1, 1, 6), // E
            };

            // Fora de ordem de propósito: a forma segue a data.
            matches.Reverse();
            var a = StandingsCalculator.Calculate(new[] { A, B }, matches).Single(r => r.TeamId == A);

            Assert.That(a.Form, Is.EqualTo(new[] { "E", "V", "V", "D", "E" }));
        }

        [Test(Description = "Jogos com equipas que não estão inscritas são ignorados.")]
        public void Calculate_IgnoresMatchesWithUnknownTeams()
        {
            var rows = StandingsCalculator.Calculate(new[] { A, B }, new[] { M(A, C, 5, 0, 1) });
            Assert.That(rows.Single(r => r.TeamId == A).Played, Is.Zero);
        }

        [TestCase(1, "PROMOTION")]
        [TestCase(2, "PROMOTION")]
        [TestCase(3, null)]
        [TestCase(7, "RELEGATION")]
        [TestCase(8, "RELEGATION")]
        public void Zone_TopAndBottomSpots(int position, string? expected)
        {
            Assert.That(StandingsCalculator.Zone(position, 8, 2, 2, hasLeagueAbove: true, hasLeagueBelow: true), Is.EqualTo(expected));
        }

        [Test(Description = "Na liga mais alta ninguém sobe; na mais baixa ninguém desce.")]
        public void Zone_NoPromotionAtTopNorRelegationAtBottom()
        {
            Assert.Multiple(() =>
            {
                Assert.That(StandingsCalculator.Zone(1, 8, 2, 2, hasLeagueAbove: false, hasLeagueBelow: true), Is.Null);
                Assert.That(StandingsCalculator.Zone(8, 8, 2, 2, hasLeagueAbove: true, hasLeagueBelow: false), Is.Null);
            });
        }

        [Test(Description = "Numa liga pequena, uma equipa não pode estar ao mesmo tempo na zona de subida e de descida.")]
        public void Zone_SmallLeague_PromotionWins()
        {
            Assert.Multiple(() =>
            {
                Assert.That(StandingsCalculator.Zone(1, 3, 2, 2, true, true), Is.EqualTo("PROMOTION"));
                Assert.That(StandingsCalculator.Zone(2, 3, 2, 2, true, true), Is.EqualTo("PROMOTION"));
                Assert.That(StandingsCalculator.Zone(3, 3, 2, 2, true, true), Is.EqualTo("RELEGATION"));
            });
        }
    }
}
