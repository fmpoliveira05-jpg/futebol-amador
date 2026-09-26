using Application.Competition;
using NUnit.Framework;

namespace Tests.Unit.CompetitionTests
{
    [TestFixture]
    public class FixtureGeneratorTests
    {
        private static List<Guid> Teams(int n) => Enumerable.Range(1, n).Select(_ => Guid.NewGuid()).ToList();

        [Test(Description = "Cada equipa recebe e visita todas as outras exatamente uma vez, em 2(n−1) jornadas.")]
        public void DoubleRoundRobin_EveryPairHomeAndAwayOnce([Range(2, 20)] int n)
        {
            var teams = Teams(n);
            var fixtures = FixtureGenerator.DoubleRoundRobin(teams, new Random(n));
            var evenN = n % 2 == 0 ? n : n + 1;

            Assert.Multiple(() =>
            {
                Assert.That(fixtures, Has.Count.EqualTo(n * (n - 1)));
                Assert.That(fixtures.Select(f => (f.HomeTeamId, f.AwayTeamId)).Distinct().Count(), Is.EqualTo(n * (n - 1)));
                Assert.That(fixtures.Max(f => f.Round), Is.EqualTo(2 * (evenN - 1)));
                Assert.That(fixtures.All(f => f.HomeTeamId != f.AwayTeamId), Is.True);
            });
        }

        [Test(Description = "Nenhuma equipa joga duas vezes na mesma jornada.")]
        public void DoubleRoundRobin_OneMatchPerTeamPerRound([Range(2, 20)] int n)
        {
            var fixtures = FixtureGenerator.DoubleRoundRobin(Teams(n), new Random(7));
            foreach (var round in fixtures.GroupBy(f => f.Round))
            {
                var playing = round.SelectMany(f => new[] { f.HomeTeamId, f.AwayTeamId }).ToList();
                Assert.That(playing.Distinct().Count(), Is.EqualTo(playing.Count), $"jornada {round.Key}");
            }
        }

        [Test(Description = "Casa e fora alternam: nunca mais de duas jornadas seguidas no mesmo sítio.")]
        public void DoubleRoundRobin_AtMostTwoConsecutiveHomeOrAway([Range(2, 20)] int n)
        {
            var teams = Teams(n);
            var fixtures = FixtureGenerator.DoubleRoundRobin(teams, new Random(n * 31));

            foreach (var team in teams)
            {
                var venues = fixtures
                    .Where(f => f.HomeTeamId == team || f.AwayTeamId == team)
                    .OrderBy(f => f.Round)
                    .Select(f => f.HomeTeamId == team ? 'C' : 'F')
                    .ToList();

                var run = 1;
                for (var i = 1; i < venues.Count; i++)
                {
                    run = venues[i] == venues[i - 1] ? run + 1 : 1;
                    Assert.That(run, Is.LessThanOrEqualTo(2), $"{n} equipas: {new string(venues.ToArray())}");
                }
            }
        }

        [Test]
        public void DoubleRoundRobin_RejectsFewerThanTwoOrRepeatedTeams()
        {
            var t = Guid.NewGuid();
            Assert.Throws<ArgumentException>(() => FixtureGenerator.DoubleRoundRobin(new[] { t }, new Random()));
            Assert.Throws<ArgumentException>(() => FixtureGenerator.DoubleRoundRobin(new[] { t, t }, new Random()));
        }

        [Test(Description = "As jornadas são semanais quando cabem na duração e ficam mais juntas quando não cabem.")]
        public void RoundDate_FitsInsideDuration()
        {
            var start = new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc);
            var kickoff = new TimeSpan(15, 0, 0);

            Assert.Multiple(() =>
            {
                Assert.That(FixtureGenerator.RoundDate(start, 1, 14, 120, kickoff), Is.EqualTo(start.AddHours(15)));
                Assert.That(FixtureGenerator.RoundDate(start, 2, 14, 120, kickoff), Is.EqualTo(start.AddDays(7).AddHours(15)));
                // 14 jornadas em 30 dias: de 2 em 2 dias.
                Assert.That(FixtureGenerator.RoundDate(start, 14, 14, 30, kickoff), Is.EqualTo(start.AddDays(26).AddHours(15)));
                Assert.That(FixtureGenerator.RoundDate(start, 14, 14, 30, kickoff), Is.LessThan(start.AddDays(30)));
            });
        }
    }
}
