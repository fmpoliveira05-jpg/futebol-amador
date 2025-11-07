using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.DTOs.Team;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub.ClienteService;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

            var opponent = new TeamDto
            {
                Name = "Team B"
            };

            var expected = new List<InfoMatchCalendar>
            {
                new InfoMatchCalendar { IdMatch = Guid.NewGuid(), Opponent = opponent }
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

            mockMatch.Verify(m => m.GetCalendar(teamId), Times.Once);
            mockMatch.Verify(m => m.GetCalendarWithFilters(It.IsAny<Guid>(), It.IsAny<FilterCalendarDto>()), Times.Never);
        }

        [Test]
        public async Task CalendarTeam_WithFilters_Calls_GetCalendarWithFilters()
        {
            var teamId = Guid.NewGuid();

            var opponent = new TeamDto
            {
                Name = "Team C"
            };

            var expected = new List<InfoMatchCalendar>
            {
                new InfoMatchCalendar { IdMatch = Guid.NewGuid(), Opponent = opponent }
            };

            var mockMatch = new Mock<IMatchService>();
            var mockAuth = new Mock<IPlayerAuthorizationService>();

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

            var expected = new InfoPostPoneMatch
            {
                IdMatch = dto.IdMatch,
                IdTeam = teamId,
                IdOpponent = dto.IdOpponent,
                PostPoneDate = dto.PostPoneDate
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
            var result = await response.Content.ReadFromJsonAsync<InfoPostPoneMatch>();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.IdMatch, Is.EqualTo(expected.IdMatch));

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
