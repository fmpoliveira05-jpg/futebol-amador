using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using System.Net;
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

            _client = _factory.WithWebHostBuilder(builder =>
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

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.GetAsync($"/api/Team/{teamId}/playersWithoutTeam");

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

            _client = _factory.WithWebHostBuilder(builder =>
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

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var queryString = $"?PlayerName={filter.PlayerName}&City={filter.City}&MinAge={filter.MinAge}&MaxAge={filter.MaxAge}&MinHeight={filter.MinHeight}&MaxHeight={filter.MaxHeight}";
            var response = await _client.GetAsync($"/api/Team/{teamId}/playersWithoutTeam{queryString}");

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<PlayerWithoutTeamInfoDto>>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Player1"));

            mockTeamService.Verify(s => s.GetPlayersWithoutTeamWithFilters(It.IsAny<FilterPlayersWithoutTeamDto>()), Times.Once);
        }

        [Test]
        public async Task AcceptMembershipRequest_Returns_Ok()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var mockPlayerAuthorizationService = new Mock<IPlayerAuthorizationService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();
            var mockTeamService = new Mock<ITeamService>(); // Adicione este mock também

            mockMembershipRequestService.Setup(s => s.AcceptMembershipRequestTeam(teamId, requestId, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerAuthorizationService));
                    services.RemoveAll(typeof(IMembershipRequestService));
                    services.RemoveAll(typeof(ITeamService));

                    services.AddSingleton<IPlayerAuthorizationService>(mockPlayerAuthorizationService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.PostAsJsonAsync($"/api/Team/{teamId}/membership-request/accept", requestId);

            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            mockMembershipRequestService.Verify(s => s.AcceptMembershipRequestTeam(teamId, requestId, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task AcceptMembershipRequest_Returns_Unauthorized_When_NotAuthenticated()
        {
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var mockPlayerAuthService = new Mock<IPlayerAuthorizationService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerAuthorizationService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerAuthorizationService>(mockPlayerAuthService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            var response = await _client.PostAsJsonAsync($"/api/Team/{teamId}/membership-request/accept", requestId);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task RejectMembershipRequest_Returns_Ok()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var mockPlayerAuthService = new Mock<IPlayerAuthorizationService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockMembershipRequestService.Setup(s => s.RejectMembershipRequestTeam(teamId, requestId, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerAuthorizationService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerAuthorizationService>(mockPlayerAuthService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.DeleteAsync($"/api/Team/{teamId}/membership-request/{requestId}/reject");

            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            mockMembershipRequestService.Verify(s => s.RejectMembershipRequestTeam(teamId, requestId, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task RejectMembershipRequest_Returns_Unauthorized_When_NotAuthenticated()
        {
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var mockPlayerAuthService = new Mock<IPlayerAuthorizationService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerAuthorizationService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerAuthorizationService>(mockPlayerAuthService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            var response = await _client.DeleteAsync($"/api/Team/{teamId}/membership-request/{requestId}/reject");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task AcceptMembershipRequest_Returns_Forbidden_When_UserNotAdmin()
        {
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var mockPlayerAuthService = new Mock<IPlayerAuthorizationService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockMembershipRequestService.Setup(s => s.AcceptMembershipRequestTeam(teamId, requestId, It.IsAny<string>()))
                .ThrowsAsync(new ValidationException("O jogador que tenta aceitar o pedido não é administrador da equipa."));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerAuthorizationService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerAuthorizationService>(mockPlayerAuthService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.PostAsJsonAsync($"/api/Team/{teamId}/membership-request/accept", requestId);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }

        [Test]
        public async Task RejectMembershipRequest_Returns_Forbidden_When_UserNotAdmin()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var mockPlayerAuthService = new Mock<IPlayerAuthorizationService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockMembershipRequestService.Setup(s => s.RejectMembershipRequestTeam(teamId, requestId, It.IsAny<string>()))
                .ThrowsAsync(new ValidationException("O jogador que tenta aceitar o pedido não é administrador da equipa."));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerAuthorizationService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerAuthorizationService>(mockPlayerAuthService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.DeleteAsync($"/api/Team/{teamId}/membership-request/{requestId}/reject");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
    }
}