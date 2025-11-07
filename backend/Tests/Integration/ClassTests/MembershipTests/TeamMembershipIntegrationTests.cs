using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Net.Http.Json;
using System.Security.Claims;
using Tests.Integration;

namespace Tests.Integration.Membership
{
    [TestFixture]
    public class TeamMembershipIntegrationTests
    {
        private ApiTestAppFactory _factory = null!;
        private HttpClient _client;

        [SetUp]
        public void Setup()
        {
            _factory = new ApiTestAppFactory();
            _client = _factory.CreateClient(new()
            {
                BaseAddress = new Uri("http://localhost")
            });
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task GetPlayersWithoutTeam_Returns_Ok_With_ListOfPlayers()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var expectedPlayers = new List<PlayerWithoutTeamInfoDto>
            {
                new PlayerWithoutTeamInfoDto { PlayerId = "1", Name = "Player1", Address = "City1", Age = 25, Height = 180 },
                new PlayerWithoutTeamInfoDto { PlayerId = "2", Name = "Player2", Address = "City2", Age = 30, Height = 175 }
            };

            var mockTeamService = new Mock<ITeamService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockTeamService.Setup(s => s.GetPlayersWithoutTeam())
                .ReturnsAsync(expectedPlayers);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var serviceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITeamService));
                    if (serviceDescriptor != null) services.Remove(serviceDescriptor);

                    var authDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPlayerAuthorizationService));
                    if (authDescriptor != null) services.Remove(authDescriptor);

                    services.AddSingleton(mockTeamService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await client.GetAsync($"/api/Team/{teamId}/playersWithoutTeam");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<PlayerWithoutTeamInfoDto>>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Player1"));
            Assert.That(result[1].Name, Is.EqualTo("Player2"));

            mockTeamService.Verify(s => s.GetPlayersWithoutTeam(), Times.Once);
        }

        [Test]
        public async Task GetPlayersWithoutTeam_WithFilters_Returns_Ok_With_ListOfPlayers()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var filter = new FilterPlayersWithoutTeamDto
            {
                PlayerName = "Player1",
                City = "City1",
                MinAge = 20,
                MaxAge = 30,
                MinHeight = 170,
                MaxHeight = 185
            };

            var expectedPlayers = new List<PlayerWithoutTeamInfoDto>
            {
                new PlayerWithoutTeamInfoDto { PlayerId = "1", Name = "Player1", Address = "City1", Age = 25, Height = 180 }
            };

            var mockTeamService = new Mock<ITeamService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockTeamService.Setup(s => s.GetPlayersWithoutTeamWithFilters(It.IsAny<FilterPlayersWithoutTeamDto>()))
                .ReturnsAsync(expectedPlayers);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var serviceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITeamService));
                    if (serviceDescriptor != null) services.Remove(serviceDescriptor);

                    var authDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPlayerAuthorizationService));
                    if (authDescriptor != null) services.Remove(authDescriptor);

                    services.AddSingleton(mockTeamService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var queryString = $"?PlayerName={filter.PlayerName}&City={filter.City}&MinAge={filter.MinAge}&MaxAge={filter.MaxAge}&MinHeight={filter.MinHeight}&MaxHeight={filter.MaxHeight}";
            var response = await client.GetAsync($"/api/Team/{teamId}/playersWithoutTeam{queryString}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<PlayerWithoutTeamInfoDto>>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Player1"));

            mockTeamService.Verify(s => s.GetPlayersWithoutTeamWithFilters(It.IsAny<FilterPlayersWithoutTeamDto>()), Times.Once);
        }
    }
}