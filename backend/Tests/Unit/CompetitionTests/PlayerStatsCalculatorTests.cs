using Application.Competition;
using Domain.Enums;
using NUnit.Framework;

namespace Tests.Unit.CompetitionTests
{
    [TestFixture]
    public class PlayerStatsCalculatorTests
    {
        private const string P = "jogador";

        [Test]
        public void MinutesPlayed_StarterWithoutEvents_Plays90()
        {
            Assert.That(PlayerStatsCalculator.MinutesPlayed(P, true, Array.Empty<EventInfo>()), Is.EqualTo(90));
        }

        [Test]
        public void MinutesPlayed_StarterSubbedOff()
        {
            var events = new[] { new EventInfo(MatchEventType.SUBSTITUTION, 60, P, "outro") };
            Assert.That(PlayerStatsCalculator.MinutesPlayed(P, true, events), Is.EqualTo(60));
        }

        [Test]
        public void MinutesPlayed_SubComesOnAndIsSentOff()
        {
            var events = new[]
            {
                new EventInfo(MatchEventType.SUBSTITUTION, 55, "outro", P),
                new EventInfo(MatchEventType.RED_CARD, 80, P, null),
            };
            Assert.That(PlayerStatsCalculator.MinutesPlayed(P, false, events), Is.EqualTo(25));
        }

        [Test(Description = "Um suplente que não entrou não jogou.")]
        public void MinutesPlayed_UnusedSub_IsNull()
        {
            Assert.That(PlayerStatsCalculator.MinutesPlayed(P, false, Array.Empty<EventInfo>()), Is.Null);
        }

        [Test(Description = "Minutos acima de 90 (compensação) contam como 90.")]
        public void MinutesPlayed_ClampsStoppageTime()
        {
            var events = new[] { new EventInfo(MatchEventType.SUBSTITUTION, 93, "outro", P) };
            Assert.That(PlayerStatsCalculator.MinutesPlayed(P, false, events), Is.EqualTo(0));
        }

        [Test(Description = "Histórico por época e equipa, com golos, assistências, cartões e minutos, e totais.")]
        public void Career_GroupsBySeasonAndTeam()
        {
            var t1 = Guid.NewGuid();
            var t2 = Guid.NewGuid();
            var matches = new[]
            {
                new PlayerMatch(Guid.NewGuid(), new DateTime(2025, 9, 1), "2025/26", t1, "Leões", true, true, new[]
                {
                    new EventInfo(MatchEventType.GOAL, 10, P, "x"),
                    new EventInfo(MatchEventType.GOAL, 20, "x", P),
                    new EventInfo(MatchEventType.YELLOW_CARD, 30, P, null),
                }),
                new PlayerMatch(Guid.NewGuid(), new DateTime(2025, 9, 8), "2025/26", t1, "Leões", false, true, new[]
                {
                    new EventInfo(MatchEventType.SUBSTITUTION, 70, "x", P),
                }),
                new PlayerMatch(Guid.NewGuid(), new DateTime(2026, 9, 1), "2026/27", t2, "Águias", true, true, Array.Empty<EventInfo>()),
                // Suplente não utilizado: não conta.
                new PlayerMatch(Guid.NewGuid(), new DateTime(2026, 9, 8), "2026/27", t2, "Águias", false, true, Array.Empty<EventInfo>()),
            };

            var career = PlayerStatsCalculator.Career(P, matches);
            var totals = PlayerStatsCalculator.Totals(career);

            Assert.Multiple(() =>
            {
                Assert.That(career.Select(c => c.Season), Is.EqualTo(new[] { "2026/27", "2025/26" }));
                var old = career.Single(c => c.Season == "2025/26");
                Assert.That((old.Games, old.Goals, old.Assists, old.YellowCards, old.Minutes), Is.EqualTo((2, 1, 1, 1, 110)));
                Assert.That(career.Single(c => c.Season == "2026/27").Games, Is.EqualTo(1));
                Assert.That((totals.Games, totals.Minutes), Is.EqualTo((3, 200)));
            });
        }

        [TestCase(2026, 8, 1, "2026/27")]
        [TestCase(2026, 7, 31, "2025/26")]
        [TestCase(2000, 1, 15, "1999/00")]
        public void SportsSeason_AugustToJuly(int y, int m, int d, string expected)
        {
            Assert.That(PlayerStatsCalculator.SportsSeason(new DateTime(y, m, d)), Is.EqualTo(expected));
        }
    }
}
