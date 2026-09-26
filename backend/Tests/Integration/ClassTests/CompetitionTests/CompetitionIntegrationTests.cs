using Application.DTOs.Competition;
using Domain.Entities;
using Domain.Enums;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration.ClassTests.CompetitionTests
{
    /// <summary>
    /// Endpoints novos de ponta a ponta (API real, base de dados em memória): ligas, classificação,
    /// táticas, perfil do jogador e relatório do jogo.
    /// </summary>
    [TestFixture]
    public class CompetitionIntegrationTests
    {
        private ApiTestAppFactory factory = null!;
        private HttpClient client = null!;
        private League league = null!;
        private Team teamA = null!, teamB = null!;
        private Matches finished = null!;
        private string scorerId = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            factory = new ApiTestAppFactory();
            client = factory.CreateClient();
            var suffix = Guid.NewGuid().ToString("N")[..6];
            scorerId = $"marcador-{suffix}";

            factory.SeedDatabase(db =>
            {
                var rank = new Rank { Name = "Divisão teste", WinPoints = 3, DrawPoints = 1, LosePoints = 0, PointsToPromotion = 10 };
                league = new League
                {
                    Name = $"Liga {suffix}", Level = 40 + Random.Shared.Next(9), PromotionSpots = 1, RelegationSpots = 1,
                    SeasonDurationDays = 60, TrophyName = $"Taça {suffix}",
                };
                teamA = new Team($"Alfa {suffix}", null, "", new Pitch("Campo Alfa", "Rua Alfa, 1"), rank) { IdLeague = league.Id };
                teamB = new Team($"Beta {suffix}", null, "", new Pitch("Campo Beta", "Rua Beta, 1"), rank) { IdLeague = league.Id };
                var season = new Season
                {
                    League = league, Name = "2026/27", Status = SeasonStatus.IN_PROGRESS,
                    StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(50),
                };
                season.Teams.Add(new SeasonTeam { Team = teamA });
                season.Teams.Add(new SeasonTeam { Team = teamB });

                var scorer = new Player(scorerId, "Marcador Teste", new DateOnly(1999, 4, 2), "Rua 2, Braga", $"{scorerId}@t.pt",
                    "+351911111111", Position.FORWARD, 175, null) { CreationDate = DateTime.UtcNow, Team = teamA, Nationality = "Portugal" };

                finished = new Matches(DateTime.UtcNow.AddDays(-3), true, teamA.Pitch.Id,
                    new List<TeamStatistics> { new(teamA) { NumGoals = 2, MatchResult = MatchResult.WIN }, new(teamB) { NumGoals = 1, MatchResult = MatchResult.LOSE } })
                {
                    MatchStatus = MatchStatus.DONE, Season = season, Round = 1, IdHomeTeam = teamA.Id,
                };

                db.AddRange(rank, league, teamA, teamB, season, scorer, finished);
                db.MatchEvent.Add(new MatchEvent { Match = finished, IdTeam = teamA.Id, Type = MatchEventType.GOAL, Minute = 33, PlayerId = scorerId });
            });
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            client.Dispose();
            factory.Dispose();
        }

        [Test]
        public async Task Leagues_ListsLeagueWithCurrentSeason()
        {
            var leagues = await client.GetFromJsonAsync<List<LeagueDto>>("/api/leagues");
            var mine = leagues!.Single(l => l.Id == league.Id);

            Assert.That(mine.CurrentSeason?.Name, Is.EqualTo("2026/27"));
            Assert.That(mine.TeamCount, Is.EqualTo(2));
        }

        [Test(Description = "A classificação vem calculada dos jogos da época (vitória 3 pontos).")]
        public async Task Standings_ComputedFromMatches()
        {
            var table = await client.GetFromJsonAsync<StandingsDto>($"/api/leagues/{league.Id}/standings");
            var viaLeaderboard = await client.GetFromJsonAsync<StandingsDto>($"/api/Leaderboard?leagueId={league.Id}");

            Assert.Multiple(() =>
            {
                Assert.That(table!.Rows[0].TeamId, Is.EqualTo(teamA.Id));
                Assert.That(table.Rows[0].Points, Is.EqualTo(3));
                Assert.That(table.Rows[0].Form, Is.EqualTo(new[] { "V" }));
                Assert.That(table.Rows[1].Form, Is.EqualTo(new[] { "D" }));
                Assert.That(viaLeaderboard!.Rows.Select(r => r.Points), Is.EqualTo(new[] { 3, 0 }));
            });
        }

        [Test]
        public async Task Formations_AreServed()
        {
            var formations = await client.GetFromJsonAsync<List<FormationDto>>("/api/lineups/formations");
            Assert.That(formations!.Select(f => f.Code), Is.EquivalentTo(new[] { "4-4-2", "4-3-3", "4-2-3-1", "3-5-2", "3-4-3", "5-3-2", "4-5-1" }));
            Assert.That(formations.All(f => f.Slots.Count == 11), Is.True);
        }

        [Test]
        public async Task PlayerProfile_CountsGoals()
        {
            var profile = await client.GetFromJsonAsync<PlayerProfileDto>($"/api/Player/{scorerId}/profile");

            Assert.Multiple(() =>
            {
                Assert.That(profile!.Totals.Goals, Is.EqualTo(1));
                Assert.That(profile.Career.Single().Season, Is.EqualTo("2026/27"));
                Assert.That(profile.Nationality, Is.EqualTo("Portugal"));
                Assert.That(profile.CurrentTeam!.IdTeam, Is.EqualTo(teamA.Id));
            });
        }

        [Test]
        public async Task MatchReport_IsPublic()
        {
            var report = await client.GetFromJsonAsync<MatchReportDto>($"/api/matches/{finished.Id}/report");

            Assert.Multiple(() =>
            {
                Assert.That(report!.Home.TeamId, Is.EqualTo(teamA.Id));
                Assert.That(report.Home.Goals, Is.EqualTo(2));
                Assert.That(report.LeagueName, Is.EqualTo(league.Name));
                Assert.That(report.Home.Events.Single().PlayerName, Is.EqualTo("Marcador Teste"));
            });
        }

        [Test]
        public async Task CreatingLeague_WithoutLogin_IsUnauthorized()
        {
            var response = await client.PostAsJsonAsync("/api/leagues", new CreateLeagueDto { Name = "X", Level = 1, SeasonDurationDays = 30, TrophyName = "Y" });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Titles_EmptyForNewTeam()
        {
            var titles = await client.GetFromJsonAsync<List<TeamTitleDto>>($"/api/Team/{teamB.Id}/titles");
            Assert.That(titles, Is.Empty);
        }
    }
}
