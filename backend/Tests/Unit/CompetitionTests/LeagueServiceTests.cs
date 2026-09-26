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
    public class LeagueServiceTests
    {
        private const string SuperAdminId = "super-admin";
        private CompetitionTestDb t = null!;
        private LeagueService sut = null!;

        [SetUp]
        public void SetUp()
        {
            t = new CompetitionTestDb();
            t.Db.SuperAdmin.Add(new SuperAdmin(SuperAdminId, "Super", new DateOnly(1990, 1, 1), "Rua 1, Porto", "s@a.pt", "+351912345670")
            {
                CreationDate = new DateTime(2026, 1, 1),
            });
            t.Db.SaveChanges();
            sut = new LeagueService(t.Competition, t.Authorization.Object, t.UnitOfWork, t.Notifications.Object, t.Clock,
                NullLogger<LeagueService>.Instance, new Random(42));
        }

        [TearDown]
        public void TearDown() => t.Dispose();

        private (League Top, League Middle, League Bottom, List<Team> MiddleTeams) ThreeLeagues(int teamsInMiddle = 4)
        {
            var top = t.AddLeague("Divisão 1", 1);
            var middle = t.AddLeague("Divisão 2", 2);
            var bottom = t.AddLeague("Divisão 3", 3);
            var teams = Enumerable.Range(1, teamsInMiddle).Select(i => t.AddTeam($"Equipa {i}", middle)).ToList();
            t.Db.SaveChanges();
            return (top, middle, bottom, teams);
        }

        private async Task<Season> StartedSeason(League league)
        {
            var season = await sut.CreateSeasonAsync(SuperAdminId, league.Id, new CreateSeasonDto
            {
                StartDate = t.Clock.GetUtcNow().UtcDateTime.AddDays(10),
            });
            await sut.StartSeasonAsync(SuperAdminId, season.Id, new StartSeasonDto { KickoffTime = "16:30" });
            return await t.Db.Season.Include(s => s.Teams).SingleAsync(s => s.Id == season.Id);
        }

        /// <summary>Marca como terminados todos os jogos da época, com o resultado dado por <paramref name="score"/>.</summary>
        private void PlayAll(Season season, Func<Team, Team, (int Home, int Away)> score)
        {
            var matches = t.Db.Match.Include(m => m.Teams).ThenInclude(ts => ts.Team).Where(m => m.IdSeason == season.Id).ToList();
            foreach (var m in matches)
            {
                var home = m.Teams.Single(x => x.IdTeam == m.IdHomeTeam);
                var away = m.Teams.Single(x => x.IdTeam != m.IdHomeTeam);
                (home.NumGoals, away.NumGoals) = score(home.Team, away.Team);
                m.MatchStatus = MatchStatus.DONE;
            }

            t.Db.SaveChanges();
        }

        [Test(Description = "A época nova inscreve as equipas da liga; o sorteio cria 2(n−1) jornadas de jogos competitivos no campo da casa.")]
        public async Task StartSeason_DrawsDoubleRoundRobin()
        {
            var (_, middle, _, teams) = ThreeLeagues(4);

            var season = await StartedSeason(middle);
            var matches = await t.Db.Match.Include(m => m.Teams).Where(m => m.IdSeason == season.Id).ToListAsync();
            var pitchOf = teams.ToDictionary(x => x.Id, x => x.IdPitch);

            Assert.Multiple(() =>
            {
                Assert.That(season.Status, Is.EqualTo(SeasonStatus.IN_PROGRESS));
                Assert.That(season.Teams, Has.Count.EqualTo(4));
                Assert.That(matches, Has.Count.EqualTo(12));
                Assert.That(matches.Select(m => m.Round).Distinct().Count(), Is.EqualTo(6));
                Assert.That(matches.All(m => m.IsCompetive), Is.True);
                Assert.That(matches.All(m => m.idPitch == pitchOf[m.IdHomeTeam!.Value]), Is.True, "joga-se no campo da equipa da casa");
                Assert.That(matches.All(m => m.MatchDate.TimeOfDay == new TimeSpan(16, 30, 0)), Is.True);
                Assert.That(matches.Min(m => m.MatchDate), Is.EqualTo(season.StartDate.Date.AddHours(16.5)));
                Assert.That(matches.Max(m => m.MatchDate), Is.LessThan(season.EndDate));
            });
        }

        [Test]
        public void ManagementRequiresSuperAdmin()
        {
            var (_, middle, _, teams) = ThreeLeagues();
            var admin = t.AddPlayer("admin", team: teams[0], admin: true);
            t.Db.SaveChanges();

            Assert.ThrowsAsync<ForbiddenException>(() => sut.CreateLeagueAsync(admin.Id, new CreateLeagueDto
            {
                Name = "Liga Nova", Level = 9, SeasonDurationDays = 60, TrophyName = "Taça Nova",
            }));
            Assert.ThrowsAsync<ForbiddenException>(() => sut.CreateSeasonAsync(null, middle.Id, new CreateSeasonDto { StartDate = DateTime.UtcNow }));
        }

        [Test]
        public async Task CreateLeague_RejectsRepeatedLevel()
        {
            ThreeLeagues();
            Assert.ThrowsAsync<ValidationException>(() => sut.CreateLeagueAsync(SuperAdminId, new CreateLeagueDto
            {
                Name = "Outra", Level = 2, SeasonDurationDays = 60, TrophyName = "Taça",
            }));

            var created = await sut.CreateLeagueAsync(SuperAdminId, new CreateLeagueDto
            {
                Name = "Divisão 4", Level = 4, PromotionSpots = 1, SeasonDurationDays = 90, TrophyName = "Taça D4",
            });
            Assert.That(created.Level, Is.EqualTo(4));
        }

        [Test(Description = "Classificação com PD, V, E, D, GM, GS, DG, P, forma e zonas de subida e descida.")]
        public async Task GetStandings_CalculatesTableFromFinishedMatches()
        {
            var (_, middle, _, teams) = ThreeLeagues(4);
            var season = await StartedSeason(middle);
            var strength = teams.Select((team, i) => (team.Id, i)).ToDictionary(x => x.Id, x => x.i);
            // A equipa com índice maior ganha sempre 2-0; no mesmo nível... não há: todas diferentes.
            PlayAll(season, (h, a) => strength[h.Id] > strength[a.Id] ? (2, 0) : (0, 2));

            var table = await sut.GetStandingsAsync(middle.Id, null);

            Assert.That(table, Is.Not.Null);
            var first = table!.Rows[0];
            var last = table.Rows[^1];
            Assert.Multiple(() =>
            {
                Assert.That(first.TeamName, Is.EqualTo("Equipa 4"));
                Assert.That((first.Played, first.Won, first.Drawn, first.Lost), Is.EqualTo((6, 6, 0, 0)));
                Assert.That((first.GoalsFor, first.GoalsAgainst, first.GoalDifference, first.Points), Is.EqualTo((12, 0, 12, 18)));
                Assert.That(first.Form, Is.EqualTo(new[] { "V", "V", "V", "V", "V" }));
                Assert.That(first.Zone, Is.EqualTo("PROMOTION"));
                Assert.That(last.TeamName, Is.EqualTo("Equipa 1"));
                Assert.That(last.Points, Is.Zero);
                Assert.That(last.Zone, Is.EqualTo("RELEGATION"));
                Assert.That(table.Rows.Select(r => r.Position), Is.EqualTo(new[] { 1, 2, 3, 4 }));
            });
        }

        [Test(Description = "Os amigáveis entre equipas da liga não contam para a classificação.")]
        public async Task GetStandings_IgnoresFriendlies()
        {
            var (_, middle, _, teams) = ThreeLeagues(2);
            await StartedSeason(middle);
            var friendly = new Matches(t.Clock.GetUtcNow().UtcDateTime, false, teams[0].IdPitch,
                new List<TeamStatistics> { new(teams[0]) { NumGoals = 5 }, new(teams[1]) })
            {
                MatchStatus = MatchStatus.DONE,
            };
            t.Db.Match.Add(friendly);
            await t.Db.SaveChangesAsync();

            var table = await sut.GetStandingsAsync(middle.Id, null);
            Assert.That(table!.Rows.All(r => r.Played == 0), Is.True);
        }

        [Test(Description = "Fechar a época: troféu ao campeão, sobe o 1.º, desce o último, jogos por jogar cancelados e nova época em inscrições.")]
        public async Task CloseSeason_AwardsTitleAndMovesTeams()
        {
            var (top, middle, bottom, teams) = ThreeLeagues(4);
            var season = await StartedSeason(middle);
            var strength = teams.ToDictionary(x => x.Id, x => int.Parse(x.Name[^1..]));
            PlayAll(season, (h, a) => strength[h.Id] > strength[a.Id] ? (1, 0) : (0, 1));

            // Um jogo fica por jogar.
            var pending = await t.Db.Match.FirstAsync(m => m.IdSeason == season.Id);
            pending.MatchStatus = MatchStatus.SCHEDULED;
            await t.Db.SaveChangesAsync();

            await sut.CloseSeasonAsync(SuperAdminId, season.Id);

            var champion = teams.Single(x => x.Name == "Equipa 4");
            var titles = await sut.GetTitlesAsync(champion.Id);
            var reloaded = await t.Db.Team.ToDictionaryAsync(x => x.Name);
            var next = await t.Db.Season.Include(s => s.Teams).SingleAsync(s => s.IdLeague == middle.Id && s.Status == SeasonStatus.REGISTRATION);

            Assert.Multiple(() =>
            {
                Assert.That(titles, Has.Count.EqualTo(1));
                Assert.That(titles[0].TrophyName, Is.EqualTo("Taça Divisão 2"));
                Assert.That(titles[0].Count, Is.EqualTo(1));
                Assert.That(reloaded["Equipa 4"].IdLeague, Is.EqualTo(top.Id), "o campeão sobe");
                Assert.That(reloaded["Equipa 1"].IdLeague, Is.EqualTo(bottom.Id), "o último desce");
                Assert.That(reloaded["Equipa 2"].IdLeague, Is.EqualTo(middle.Id));
                Assert.That(t.Db.Season.Single(s => s.Id == season.Id).Status, Is.EqualTo(SeasonStatus.FINISHED));
                Assert.That(t.Db.Match.Single(m => m.Id == pending.Id).MatchStatus, Is.EqualTo(MatchStatus.CANCELED));
                Assert.That(next.Teams.Select(x => x.IdTeam), Is.EquivalentTo(new[] { reloaded["Equipa 2"].Id, reloaded["Equipa 3"].Id }));
            });
        }

        [Test(Description = "Os títulos agrupam-se por troféu: três campeonatos da mesma liga dão \"x3\".")]
        public void GroupTitles_CountsByTrophy()
        {
            var teamId = Guid.NewGuid();
            TeamTitle T(string trophy, string season) => new()
            {
                IdTeam = teamId, TrophyName = trophy, LeagueName = "Liga", SeasonName = season,
                WonAt = new DateTime(2000 + int.Parse(season[..4]) % 100, 6, 1),
            };

            var grouped = LeagueService.GroupTitles(new[]
            {
                T("Taça A", "2024/25"), T("Taça B", "2023/24"), T("Taça A", "2025/26"), T("Taça A", "2026/27"),
            });

            Assert.Multiple(() =>
            {
                Assert.That(grouped.Select(g => (g.TrophyName, g.Count)), Is.EqualTo(new[] { ("Taça A", 3), ("Taça B", 1) }));
                Assert.That(grouped[0].Seasons, Is.EqualTo(new[] { "2024/25", "2025/26", "2026/27" }));
            });
        }

        [Test(Description = "Tarefa periódica: sorteia a época uma semana antes do início e fecha-a na data de fim.")]
        public async Task RunScheduledTasks_StartsAndClosesSeasons()
        {
            var (_, middle, _, _) = ThreeLeagues(3);
            var season = await sut.CreateSeasonAsync(SuperAdminId, middle.Id, new CreateSeasonDto
            {
                StartDate = t.Clock.GetUtcNow().UtcDateTime.Date.AddDays(10),
            });

            Assert.That(await sut.RunScheduledTasksAsync(), Is.Zero, "a 10 dias do início ainda não sorteia");

            t.Clock.Advance(TimeSpan.FromDays(4));
            Assert.That(await sut.RunScheduledTasksAsync(), Is.EqualTo(1));
            Assert.That(t.Db.Season.Single(s => s.Id == season.Id).Status, Is.EqualTo(SeasonStatus.IN_PROGRESS));

            t.Clock.Set(t.Db.Season.Single(s => s.Id == season.Id).EndDate.AddMinutes(1));
            await sut.RunScheduledTasksAsync();
            Assert.That(t.Db.Season.Single(s => s.Id == season.Id).Status, Is.EqualTo(SeasonStatus.FINISHED));
        }

        [Test(Description = "Uma equipa inscreve-se nas inscrições abertas e sai das inscrições de outra liga.")]
        public async Task RegisterTeam_MovesRegistration()
        {
            var (top, middle, _, teams) = ThreeLeagues(2);
            var admin = t.AddPlayer("admin", team: teams[0], admin: true);
            await t.Db.SaveChangesAsync();
            await sut.CreateSeasonAsync(SuperAdminId, middle.Id, new CreateSeasonDto { StartDate = DateTime.UtcNow.AddDays(20) });
            var topSeason = await sut.CreateSeasonAsync(SuperAdminId, top.Id, new CreateSeasonDto { StartDate = DateTime.UtcNow.AddDays(20) });

            var result = await sut.RegisterTeamAsync(admin.Id, top.Id, teams[0].Id);

            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(topSeason.Id));
                Assert.That(result.TeamCount, Is.EqualTo(1));
                Assert.That(t.Db.SeasonTeam.Count(st => st.IdTeam == teams[0].Id), Is.EqualTo(1));
                Assert.That(t.Db.Team.Single(x => x.Id == teams[0].Id).IdLeague, Is.EqualTo(top.Id));
            });
        }

        [Test]
        public async Task RegisterTeam_BlockedDuringSeasonInProgress()
        {
            var (top, middle, _, teams) = ThreeLeagues(2);
            var admin = t.AddPlayer("admin", team: teams[0], admin: true);
            await t.Db.SaveChangesAsync();
            await StartedSeason(middle);
            await sut.CreateSeasonAsync(SuperAdminId, top.Id, new CreateSeasonDto { StartDate = DateTime.UtcNow.AddDays(20) });

            Assert.ThrowsAsync<BusinessRuleException>(() => sut.RegisterTeamAsync(admin.Id, top.Id, teams[0].Id));
        }

        [Test]
        public void StartSeason_NeedsTwoTeams()
        {
            var league = t.AddLeague("Sozinha", 1);
            t.AddTeam("Única", league);
            t.Db.SaveChanges();

            Assert.ThrowsAsync<BusinessRuleException>(async () =>
            {
                var s = await sut.CreateSeasonAsync(SuperAdminId, league.Id, new CreateSeasonDto { StartDate = DateTime.UtcNow.AddDays(3) });
                await sut.StartSeasonAsync(SuperAdminId, s.Id, new StartSeasonDto());
            });
        }
    }
}
