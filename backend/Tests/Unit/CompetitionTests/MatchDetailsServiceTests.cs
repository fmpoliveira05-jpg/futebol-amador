using Application.DTOs.Competition;
using Application.Services.Competition;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Tests.Unit.CompetitionTests
{
    [TestFixture]
    public class MatchDetailsServiceTests
    {
        private CompetitionTestDb t = null!;
        private MatchDetailsService sut = null!;
        private Team home = null!, away = null!;
        private Player homeAdmin = null!, awayAdmin = null!;
        private Matches match = null!;

        [SetUp]
        public void SetUp()
        {
            t = new CompetitionTestDb();
            home = t.AddTeam("Casa");
            away = t.AddTeam("Fora");
            homeAdmin = t.AddPlayer("casa-gr", Position.GOALKEEPER, home, admin: true);
            awayAdmin = t.AddPlayer("fora-gr", Position.GOALKEEPER, away, admin: true);
            foreach (var (team, prefix) in new[] { (home, "casa"), (away, "fora") })
            {
                for (var i = 1; i <= 4; i++) t.AddPlayer($"{prefix}-d{i}", Position.DEFENDER, team);
                for (var i = 1; i <= 4; i++) t.AddPlayer($"{prefix}-m{i}", Position.MIDFIELDER, team);
                for (var i = 1; i <= 3; i++) t.AddPlayer($"{prefix}-a{i}", Position.FORWARD, team);
            }

            match = new Matches(t.Clock.GetUtcNow().UtcDateTime.AddDays(1), true, home.IdPitch,
                new List<TeamStatistics> { new(home), new(away) })
            {
                IdHomeTeam = home.Id,
            };
            t.Db.Match.Add(match);
            t.Db.SaveChanges();

            sut = new MatchDetailsService(t.Details, t.Teams, t.Authorization.Object, t.UnitOfWork, t.Notifications.Object,
                t.Clock, NullLogger<MatchDetailsService>.Instance);
        }

        [TearDown]
        public void TearDown() => t.Dispose();

        private static SaveLineupDto Lineup433(string prefix) => new()
        {
            Formation = "4-3-3",
            Starters = new List<SaveLineupSlotDto>
            {
                new() { Slot = 0, PlayerId = $"{prefix}-gr" },
                new() { Slot = 1, PlayerId = $"{prefix}-d1" }, new() { Slot = 2, PlayerId = $"{prefix}-d2" },
                new() { Slot = 3, PlayerId = $"{prefix}-d3" }, new() { Slot = 4, PlayerId = $"{prefix}-d4" },
                new() { Slot = 5, PlayerId = $"{prefix}-m1" }, new() { Slot = 6, PlayerId = $"{prefix}-m2" },
                new() { Slot = 7, PlayerId = $"{prefix}-m3" },
                new() { Slot = 8, PlayerId = $"{prefix}-a1" }, new() { Slot = 9, PlayerId = $"{prefix}-a2" },
                new() { Slot = 10, PlayerId = $"{prefix}-a3" },
            },
            Bench = new List<string> { $"{prefix}-m4" },
        };

        [Test(Description = "O administrador guarda o onze; o adversário só o vê depois do prazo.")]
        public async Task SaveLineup_AndOpponentVisibility()
        {
            var saved = await sut.SaveLineupAsync(homeAdmin.Id, match.Id, home.Id, Lineup433("casa"));

            Assert.Multiple(() =>
            {
                Assert.That(saved.Exists, Is.True);
                Assert.That(saved.Formation, Is.EqualTo("4-3-3"));
                Assert.That(saved.Starters, Has.Count.EqualTo(11));
                Assert.That(saved.Starters[0].PositionCode, Is.EqualTo("GR"));
                Assert.That(saved.Bench.Single().PlayerId, Is.EqualTo("casa-m4"));
                Assert.That(saved.Deadline, Is.EqualTo(match.MatchDate.AddHours(-2)));
            });

            Assert.ThrowsAsync<ForbiddenException>(() => sut.GetLineupAsync(awayAdmin.Id, match.Id, home.Id));

            t.Clock.Set(match.MatchDate.AddHours(-1));
            var seenByOpponent = await sut.GetLineupAsync(awayAdmin.Id, match.Id, home.Id);
            Assert.That(seenByOpponent.IsLocked, Is.True);
        }

        [Test(Description = "Depois do prazo (2 horas antes) já não se altera o onze.")]
        public void SaveLineup_AfterDeadline_Throws()
        {
            t.Clock.Set(match.MatchDate.AddHours(-2));
            Assert.ThrowsAsync<BusinessRuleException>(() => sut.SaveLineupAsync(homeAdmin.Id, match.Id, home.Id, Lineup433("casa")));
        }

        [Test]
        public void SaveLineup_RejectsOtherTeamsPlayersAndDuplicates()
        {
            var foreign = Lineup433("casa");
            foreign.Starters[10].PlayerId = "fora-a3";
            var duplicated = Lineup433("casa");
            duplicated.Bench.Add("casa-a1");

            Assert.ThrowsAsync<ValidationException>(() => sut.SaveLineupAsync(homeAdmin.Id, match.Id, home.Id, foreign));
            Assert.ThrowsAsync<ValidationException>(() => sut.SaveLineupAsync(homeAdmin.Id, match.Id, home.Id, duplicated));
        }

        [Test(Description = "Sem onze até ao prazo: é preenchido automaticamente, com o último onze da equipa.")]
        public async Task AutoFill_UsesLastLineup()
        {
            // Jogo anterior com onze definido.
            var previous = new Matches(t.Clock.GetUtcNow().UtcDateTime.AddHours(5), true, home.IdPitch,
                new List<TeamStatistics> { new(home), new(away) });
            t.Db.Match.Add(previous);
            await t.Db.SaveChangesAsync();
            await sut.SaveLineupAsync(homeAdmin.Id, previous.Id, home.Id, Lineup433("casa"));
            previous.MatchStatus = MatchStatus.DONE;
            await t.Db.SaveChangesAsync();

            t.Clock.Set(match.MatchDate.AddHours(-1));
            var filled = await sut.AutoFillMissingLineupsAsync();
            var homeLineup = await sut.GetLineupAsync(homeAdmin.Id, match.Id, home.Id);
            var awayLineup = await sut.GetLineupAsync(awayAdmin.Id, match.Id, away.Id);

            Assert.Multiple(() =>
            {
                Assert.That(filled, Is.EqualTo(2));
                Assert.That(homeLineup.IsAutoFilled, Is.True);
                Assert.That(homeLineup.Formation, Is.EqualTo("4-3-3"));
                Assert.That(homeLineup.Starters.Select(s => s.PlayerId), Is.EqualTo(Lineup433("casa").Starters.Select(s => s.PlayerId)));
                Assert.That(awayLineup.Starters, Has.Count.EqualTo(11));
                Assert.That(awayLineup.Starters.Single(s => s.Slot == 0).PlayerId, Is.EqualTo("fora-gr"));
            });

            Assert.That(await sut.AutoFillMissingLineupsAsync(), Is.Zero, "não volta a preencher");
        }

        [Test(Description = "Eventos e faltas ficam no relatório com os nomes e os totais por equipa; o perfil conta golos e minutos.")]
        public async Task ApplyEvents_ReportAndProfile()
        {
            await sut.SaveLineupAsync(homeAdmin.Id, match.Id, home.Id, Lineup433("casa"));
            var homeEvents = new MatchEventsDto
            {
                Fouls = 9,
                Goals = { new GoalEventDto { ScorerId = "casa-a1", AssistId = "casa-m1", Minute = 12 } },
                Cards = { new CardEventDto { PlayerId = "casa-d1", Type = CardType.YELLOW, Minute = 30 } },
                Substitutions = { new SubstitutionEventDto { PlayerOutId = "casa-a1", PlayerInId = "casa-m4", Minute = 70 } },
            };

            await sut.ValidateEventsAsync(match.Id, home.Id, 1, homeEvents);
            await sut.ApplyEventsAsync(match.Id, new Dictionary<Guid, MatchEventsDto?> { [home.Id] = homeEvents, [away.Id] = new() { Fouls = 14 } });
            var stats = match.Teams.Single(x => x.IdTeam == home.Id);
            stats.NumGoals = 1;
            match.MatchStatus = MatchStatus.DONE;
            await t.Db.SaveChangesAsync();

            var report = await sut.GetReportAsync(match.Id);
            var profile = await new PlayerProfileService(t.Players, t.Details, t.Transfers, t.Teams, t.Clock).GetProfileAsync("casa-a1");
            var sub = await new PlayerProfileService(t.Players, t.Details, t.Transfers, t.Teams, t.Clock).GetProfileAsync("casa-m4");
            var proprio = await new PlayerProfileService(t.Players, t.Details, t.Transfers, t.Teams, t.Clock).GetProfileAsync("casa-a1", "casa-a1");

            Assert.Multiple(() =>
            {
                Assert.That(report.Home.TeamName, Is.EqualTo("Casa"));
                Assert.That((report.Home.Goals, report.Home.Fouls, report.Home.YellowCards, report.Home.Substitutions), Is.EqualTo(((int?)1, (int?)9, 1, 1)));
                Assert.That(report.Away.Fouls, Is.EqualTo(14));
                Assert.That(report.Home.Events.First().Type, Is.EqualTo("GOAL"));
                Assert.That(report.Home.Events.First().RelatedPlayerName, Is.EqualTo("Jogador casa-m1"));
                Assert.That(report.Home.Lineup, Is.Not.Null);

                Assert.That((profile.Totals.Games, profile.Totals.Goals, profile.Totals.Minutes), Is.EqualTo((1, 1, 70)));
                Assert.That(profile.Career.Single().TeamName, Is.EqualTo("Casa"));
                Assert.That((sub.Totals.Games, sub.Totals.Minutes), Is.EqualTo((1, 20)));

                // O perfil é público: a data de nascimento só vai para o próprio (os outros veem a idade).
                Assert.That(profile.DateOfBirth, Is.Null);
                Assert.That(profile.Age, Is.GreaterThan(0));
                Assert.That(proprio.DateOfBirth, Is.Not.Null);
            });
        }
    }
}
