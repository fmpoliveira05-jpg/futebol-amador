using Application.DTOs.Team;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tests.Integration;

namespace Tests.Integration.Leaderboard
{
    [TestFixture]
    public class LeaderboardIntegrationTests
    {
        private ApiTestAppFactory _factory = null!;
        private WebApplicationFactory<Program> _appFactory = null!;
        private HttpClient _client;

        [SetUp]
        public void Setup()
        {
            _factory = new ApiTestAppFactory();
            _appFactory = _factory;
            _client = _factory.CreateClient(new()
            {
                BaseAddress = new Uri("http://localhost")
            });
        }

        #region TearDown
        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _appFactory?.Dispose();
            _factory?.Dispose();
        }
        #endregion

        [Test]
        public async Task GetLeaderboard_Returns_Teams_In_Correct_Order()
        {
            // Arrange - Mock the ILeaderboardService
            var mockLeaderboardService = new Mock<ILeaderboardService>();

            var expectedTeams = new List<TeamLeaderboardDto>
            {
                new TeamLeaderboardDto { TeamName = "Falcons", CurrentPoints = 90 },
                new TeamLeaderboardDto { TeamName = "Wolves", CurrentPoints = 75 },
                new TeamLeaderboardDto { TeamName = "Eagles", CurrentPoints = 60 }
            };

            mockLeaderboardService.Setup(s => s.GetLeaderboardAsync())
                .ReturnsAsync(expectedTeams);

            // Create client with mocked service
            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockLeaderboardService.Object);
                });
            });

            var client = clientFactory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/leaderboard");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<TeamLeaderboardDto>>();

            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0].TeamName, Is.EqualTo("Falcons"));
            Assert.That(result[1].TeamName, Is.EqualTo("Wolves"));
            Assert.That(result[2].TeamName, Is.EqualTo("Eagles"));

            // Verify the service was called
            mockLeaderboardService.Verify(s => s.GetLeaderboardAsync(), Times.Once);
        }
    }
}