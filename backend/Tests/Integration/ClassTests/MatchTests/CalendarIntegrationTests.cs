using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.Pitch;
using Application.DTOs.PostPoneGame;
using Application.DTOs.Team;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub.ClienteService;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration.Calendar
{
    public class CalendarIntegrationTests
    {
        private ApiTestAppFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public void Setup()
        {
            _factory = new ApiTestAppFactory();
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task CalendarTeam_NoFilters_ReturnsMatches()
        {
            // Arrange
            var teamId = Guid.NewGuid();

            // Criar um objeto de retorno completo e válido segundo o DTO InfoMatchCalendar
            var expected = new List<InfoMatchCalendar>
            {
                new InfoMatchCalendar
                {
                    IdMatch = Guid.NewGuid(),
                    MatchStatus = MatchStatus.SCHEDULED,
                    GameDate = DateTime.UtcNow.AddDays(2),
                    MatchResult = MatchResult.UNPLAYED,
                    IsCompetitive = true,
                    IsHome = true,
                    Team = new TeamStatisticsDto
                    {
                        IdTeam = teamId,
                        Name = "My Team",
                        NumGoals = 0
                    },
                    Opponent = new TeamStatisticsDto
                    {
                        IdTeam = Guid.NewGuid(),
                        Name = "Team B",
                        NumGoals = 0
                    },
                    PitchGame = new PitchDto
                    {
                        Name = "Pitch A",
                        Address = "Address A"
                    }
                }
            };

            var mockMatch = new Mock<IMatchService>();
            var mockAuth = new Mock<IPlayerAuthorizationService>();

            mockMatch.Setup(m => m.GetCalendar(teamId))
                     .ReturnsAsync(expected);

            mockAuth.Setup(a => a.UserAuthorizationIsMemberTeamById(It.IsAny<string>(), teamId))
                    .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IMatchService));
                    services.RemoveAll(typeof(IPlayerAuthorizationService));

                    services.AddSingleton(mockMatch.Object);
                    services.AddSingleton(mockAuth.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var result = await _client.GetFromJsonAsync<List<InfoMatchCalendar>>($"/api/Calendar/{teamId}");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Opponent.Name, Is.EqualTo("Team B"));
            Assert.That(result[0].IdMatch, Is.EqualTo(expected[0].IdMatch));

            mockMatch.Verify(m => m.GetCalendar(teamId), Times.Once);
            mockMatch.Verify(m => m.GetCalendarWithFilters(It.IsAny<Guid>(), It.IsAny<FilterCalendarDto>()), Times.Never);
        }

        [Test]
        public async Task CalendarTeam_WithFilters_Calls_GetCalendarWithFilters()
        {
            var teamId = Guid.NewGuid();

            var expected = new List<InfoMatchCalendar>
            {
                new InfoMatchCalendar
                {
                    IdMatch = Guid.NewGuid(),
                    MatchStatus = MatchStatus.DONE,
                    GameDate = DateTime.UtcNow.AddDays(-2),
                    MatchResult = MatchResult.WIN,
                    IsCompetitive = true,
                    IsHome = false,
                    Team = new TeamStatisticsDto { IdTeam = teamId, Name = "My Team", NumGoals = 2 },
                    Opponent = new TeamStatisticsDto { IdTeam = Guid.NewGuid(), Name = "Team C", NumGoals = 1 },
                    PitchGame = new PitchDto { Name = "Pitch B", Address = "Address B" }
                }
            };

            var mockMatch = new Mock<IMatchService>();
            var mockAuth = new Mock<IPlayerAuthorizationService>();

            // Setup para aceitar qualquer filtro que chegue
            mockMatch.Setup(m => m.GetCalendarWithFilters(teamId, It.IsAny<FilterCalendarDto>()))
                     .ReturnsAsync(expected);

            mockAuth.Setup(a => a.UserAuthorizationIsMemberTeamById(It.IsAny<string>(), teamId))
                    .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IMatchService));
                    services.RemoveAll(typeof(IPlayerAuthorizationService));

                    services.AddSingleton(mockMatch.Object);
                    services.AddSingleton(mockAuth.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            // Enviamos filtros na query string
            var result = await _client.GetFromJsonAsync<List<InfoMatchCalendar>>($"/api/Calendar/{teamId}?IsRealized=true");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Opponent.Name, Is.EqualTo("Team C"));

            mockMatch.Verify(m => m.GetCalendarWithFilters(teamId, It.IsAny<FilterCalendarDto>()), Times.Once);
            mockMatch.Verify(m => m.GetCalendar(teamId), Times.Never);
        }

        [Test]
        public async Task PostponeMatch_ReturnsInfoPostPoneMatch()
        {
            var teamId = Guid.NewGuid();
            var dto = new PostPoneMatchDto
            {
                IdMatch = Guid.NewGuid(),
                IdOpponent = Guid.NewGuid(),
                PostPoneDate = DateTime.UtcNow.AddDays(1)
            };

            // Criar um objeto de retorno válido (Preenchendo Team e Opponent que são Required)
            var expected = new InfoPostPoneMatch
            {
                IdMatch = dto.IdMatch,
                GameDate = DateTime.UtcNow, // Data original
                PostPoneDate = dto.PostPoneDate, // Nova data
                Team = new TeamDto { IdTeam = teamId, Name = "My Team" },
                Opponent = new TeamDto { IdTeam = dto.IdOpponent, Name = "Opponent Team" }
            };

            var mockMatch = new Mock<IMatchService>();
            var mockAuth = new Mock<IPlayerAuthorizationService>();

            mockMatch.Setup(m => m.PostPoneMatch(teamId, It.IsAny<PostPoneMatchDto>()))
                     .ReturnsAsync(expected);

            mockAuth.Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), teamId))
                    .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IMatchService));
                    services.RemoveAll(typeof(IPlayerAuthorizationService));

                    services.AddSingleton(mockMatch.Object);
                    services.AddSingleton(mockAuth.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.PutAsJsonAsync($"/api/Calendar/{teamId}/PostponeMatch", dto);

            // Assert
            if (!response.IsSuccessStatusCode) Assert.Fail(await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadFromJsonAsync<InfoPostPoneMatch>();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.IdMatch, Is.EqualTo(expected.IdMatch));
            Assert.That(result.PostPoneDate, Is.EqualTo(expected.PostPoneDate));

            mockMatch.Verify(m => m.PostPoneMatch(teamId, It.IsAny<PostPoneMatchDto>()), Times.Once);
        }

        [Test]
        public async Task CancelMatch_ReturnsOk()
        {
            var teamId = Guid.NewGuid();
            var matchId = Guid.NewGuid();
            var desc = "Bad weather";

            var mockMatch = new Mock<IMatchService>();
            var mockAuth = new Mock<IPlayerAuthorizationService>();

            mockMatch.Setup(m => m.CancelMatch(teamId, matchId, desc)).Returns(Task.CompletedTask);
            mockAuth.Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), teamId)).Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IMatchService));
                    services.RemoveAll(typeof(IPlayerAuthorizationService));

                    services.AddSingleton(mockMatch.Object);
                    services.AddSingleton(mockAuth.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // ✅ Send description in the request body as JSON
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/Calendar/{teamId}/CancelMatch/{matchId}")
            {
                Content = new StringContent($"\"{desc}\"", System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            mockMatch.Verify(m => m.CancelMatch(teamId, matchId, desc), Times.Once);
        }

    }
}
