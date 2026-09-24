using Application.DTOs.Filters;
using Application.DTOs.Membership;
using Application.DTOs.MemberShip;
using Application.DTOs.Team;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration.ClassTests.MembershipIntegrationTests
{
    [TestFixture]
    internal class PlayerMembershipIntegrationTests
    {
        private ApiTestAppFactory _factory = null;
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

        #region ListTeamsToMemberShipRequest Tests

        /*
        [Test]
        public async Task ListTeamsToMemberShipRequest_Returns_Teams_When_NoFilters()
        {
            var expectedTeams = new List<InfoTeamsDto>
            {
                new InfoTeamsDto { Id = Guid.NewGuid(), Name = "Team 1", Address = "City1", PlayerCount = 10, CurrentPoints = 100 },
                new InfoTeamsDto { Id = Guid.NewGuid(), Name = "Team 2", Address = "City2", PlayerCount = 15, CurrentPoints = 150 }
            };

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockPlayerService.Setup(s => s.GetListTeams())
                .ReturnsAsync(expectedTeams);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.GetAsync("/api/Player/listTeamsToMemberShipRequest");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<List<InfoTeamsDto>>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));

            mockPlayerService.Verify(s => s.GetListTeams(), Times.Once);
        }
        */

        /*
        [Test]
        public async Task ListTeamsToMemberShipRequest_Returns_Teams_When_WithFilters()
        {
            var expectedTeams = new List<InfoTeamsDto>
            {
                new InfoTeamsDto { Id = Guid.NewGuid(), Name = "Filtered Team", Address = "FilterCity", PlayerCount = 5, CurrentPoints = 50 }
            };

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockPlayerService.Setup(s => s.GetTeamListWithFilters(It.IsAny<FilterListTeamDto>()))
                .ReturnsAsync(expectedTeams);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.GetAsync("/api/Player/listTeamsToMemberShipRequest?NameTeam=Filtered&City=FilterCity&MinNumberPlayers=1&MaxNumberPlayers=10");


            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<List<InfoTeamsDto>>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));

            mockPlayerService.Verify(s => s.GetTeamListWithFilters(It.IsAny<FilterListTeamDto>()), Times.Once);
        }
        */

        [Test]
        public async Task ListTeamsToMemberShipRequest_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.GetAsync("/api/Player/listTeamsToMemberShipRequest");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        #endregion

        #region GetMembershipRequests Tests

        [Test]
        public async Task GetMembershipRequests_Returns_Requests_When_NoFilters()
        {
            // Arrange
            var playerId = TestAuthHandler.TestUserId;
            var expectedRequests = new List<MemberShipRequestDto>
            {
                new MemberShipRequestDto
                {
                    RequestId = Guid.NewGuid(),
                    Player = new PlayerDto
                    {
                        Id = playerId,
                        Name =  "Test Player"
                    },
                    Team = new TeamDto
                    {
                        IdTeam = Guid.NewGuid(),
                        Name = "Team A",
                    },
                    RequestDate = DateTime.UtcNow.AddDays(-1),
                    IsPlayerSender = true
                }
            };

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId))
                .Verifiable();

            mockMembershipRequestService.Setup(s => s.GetMembershipRequestsAsyncPlayer(playerId))
                .ReturnsAsync(expectedRequests);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.GetAsync($"/api/Player/{playerId}/membership-requests");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<List<MemberShipRequestDto>>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));

            mockPlayerAuthValidator.Verify(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId), Times.Once);
            mockMembershipRequestService.Verify(s => s.GetMembershipRequestsAsyncPlayer(playerId), Times.Once);
        }

        [Test]
        public async Task GetMembershipRequests_Returns_Requests_When_WithFilters()
        {
            // Arrange
            var playerId = TestAuthHandler.TestUserId;
            var expectedRequests = new List<MemberShipRequestDto>
            {
                new MemberShipRequestDto
                {
                    RequestId = Guid.NewGuid(),
                    Player = new PlayerDto
                    {
                        Id = playerId,
                        Name =  "Test Player"
                    },
                    Team = new TeamDto
                    {
                        IdTeam = Guid.NewGuid(),
                        Name = "Team B",
                    },
                    RequestDate = DateTime.UtcNow.AddDays(-2),
                    IsPlayerSender = false
                }
            };

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId))
                .Verifiable();

            mockMembershipRequestService.Setup(s => s.GetMembershipRequestsAsyncPlayerWithFilters(playerId, It.IsAny<FilterMembershipRequestsPlayer>()))
                .ReturnsAsync(expectedRequests);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.GetAsync($"/api/Player/{playerId}/membership-requests?SenderName=Team&MinDate=2023-01-01");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<List<MemberShipRequestDto>>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));

            mockPlayerAuthValidator.Verify(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId), Times.Once);
            mockMembershipRequestService.Verify(s => s.GetMembershipRequestsAsyncPlayerWithFilters(playerId, It.IsAny<FilterMembershipRequestsPlayer>()), Times.Once);
        }

        [Test]
        public async Task GetMembershipRequests_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var playerId = TestAuthHandler.TestUserId;

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.GetAsync($"/api/Player/{playerId}/membership-requests");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        #endregion

        #region AcceptMembershipRequest Tests

        [Test]
        public async Task AcceptMembershipRequest_Returns_MemberShipRequestDto_When_Successful()
        {
            // Arrange
            var playerId = TestAuthHandler.TestUserId;
            var requestId = Guid.NewGuid();
            var expectedResponse = new MemberShipRequestDto
            {
                RequestId = requestId,
                Player = new PlayerDto
                {
                    Id = playerId,
                    Name = "Test Player"
                },
                Team = new TeamDto
                {
                    IdTeam = Guid.NewGuid(),
                    Name = "Accepted Team",
                },
                RequestDate = DateTime.UtcNow,
                IsPlayerSender = true
            };

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId))
                .Verifiable();

            mockMembershipRequestService.Setup(s => s.AcceptMembershipRequestAsyncPlayer(playerId, requestId))
                .ReturnsAsync(expectedResponse);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.PostAsJsonAsync($"/api/Player/{playerId}/membership-requests/accept", requestId);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<MemberShipRequestDto>();
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.RequestId, Is.EqualTo(expectedResponse.RequestId));
                Assert.That(result.Player.Id, Is.EqualTo(expectedResponse.Player.Id));
                Assert.That(result.Team.Name, Is.EqualTo(expectedResponse.Team.Name));
                Assert.That(result.IsPlayerSender, Is.EqualTo(expectedResponse.IsPlayerSender));
            });

            mockPlayerAuthValidator.Verify(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId), Times.Once);
            mockMembershipRequestService.Verify(s => s.AcceptMembershipRequestAsyncPlayer(playerId, requestId), Times.Once);
        }

        [Test]
        public async Task AcceptMembershipRequest_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var playerId = TestAuthHandler.TestUserId;
            var requestId = Guid.NewGuid();

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            var response = await _client.PostAsJsonAsync($"/api/Player/{playerId}/membership-requests/accept", requestId);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        #endregion

        #region RejectMembershipRequest Tests

        [Test]
        public async Task RejectMembershipRequest_Returns_MemberShipRequestDto_When_Successful()
        {
            var playerId = TestAuthHandler.TestUserId;
            var requestId = Guid.NewGuid();
            var expectedResponse = new MemberShipRequestDto
            {
                RequestId = requestId,
                Player = new PlayerDto
                {
                    Id = playerId,
                    Name = "Test Player"
                },
                Team = new TeamDto
                {
                    IdTeam = Guid.NewGuid(),
                    Name = "Rejected Team",
                },
                RequestDate = DateTime.UtcNow.AddDays(-1),
                IsPlayerSender = false
            };

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId))
                .Verifiable();

            mockMembershipRequestService.Setup(s => s.RejectMembershipRequestAsyncPlayer(playerId, requestId))
                .ReturnsAsync(expectedResponse);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.DeleteAsync($"/api/Player/{playerId}/membership-requests/reject/{requestId}");

            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<MemberShipRequestDto>();
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.RequestId, Is.EqualTo(expectedResponse.RequestId));
                Assert.That(result.Player.Id, Is.EqualTo(expectedResponse.Player.Id));
                Assert.That(result.Team.Name, Is.EqualTo(expectedResponse.Team.Name));
                Assert.That(result.IsPlayerSender, Is.EqualTo(expectedResponse.IsPlayerSender));
            });

            mockPlayerAuthValidator.Verify(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId), Times.Once);
            mockMembershipRequestService.Verify(s => s.RejectMembershipRequestAsyncPlayer(playerId, requestId), Times.Once);
        }

        [Test]
        [Ignore("Test temporarily ignored")]
        public async Task RejectMembershipRequest_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var playerId = TestAuthHandler.TestUserId;
            var requestId = Guid.NewGuid();

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.DeleteAsync($"/api/Player/{playerId}/membership-requests/reject/{requestId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        #endregion

        #region SendMembershipRequest Tests

        [Test]
        public async Task SendMembershipRequest_Returns_MemberShipRequestDto_When_Successful()
        {
            // Arrange
            var playerId = TestAuthHandler.TestUserId;
            var teamId = Guid.NewGuid();
            var expectedResponse = new MemberShipRequestDto
            {
                RequestId = Guid.NewGuid(),
                Player = new PlayerDto
                {
                    Id = playerId,
                    Name = "Test Player"
                },
                Team = new TeamDto
                {
                    IdTeam = teamId,
                    Name = "Target Team",
                },
                RequestDate = DateTime.UtcNow,
                IsPlayerSender = true
            };

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId))
                .Verifiable();

            mockMembershipRequestService.Setup(s => s.SendMembershipRequestAsyncPlayer(playerId, teamId))
                .ReturnsAsync(expectedResponse);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.PostAsJsonAsync($"/api/Player/{playerId}/membership-requests/send", teamId);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<MemberShipRequestDto>();
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Player.Id, Is.EqualTo(expectedResponse.Player.Id));
                Assert.That(result.Team.IdTeam, Is.EqualTo(expectedResponse.Team.IdTeam));
                Assert.That(result.Team.Name, Is.EqualTo(expectedResponse.Team.Name));
                Assert.That(result.IsPlayerSender, Is.EqualTo(expectedResponse.IsPlayerSender));
            });

            mockPlayerAuthValidator.Verify(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId), Times.Once);
            mockMembershipRequestService.Verify(s => s.SendMembershipRequestAsyncPlayer(playerId, teamId), Times.Once);
        }

        [Test]
        public async Task SendMembershipRequest_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var playerId = TestAuthHandler.TestUserId;
            var teamId = Guid.NewGuid();

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.PostAsJsonAsync($"/api/Player/{playerId}/membership-requests/send", teamId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        #endregion

        #region Authorization Failure Tests

        [Test]
        public async Task GetMembershipRequests_Returns_Forbidden_When_UserNotAuthorized()
        {
            // Arrange
            var playerId = "different-player-id";

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId))
                .Throws(new UnauthorizedAccessException("O utilizador que está a tentar entrar não é o mesmo da url."));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.GetAsync($"/api/Player/{playerId}/membership-requests");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Contains.Substring("O utilizador que está a tentar entrar não é o mesmo da url."));
        }

        [Test]
        public async Task AcceptMembershipRequest_Returns_Forbidden_When_UserNotAuthorized()
        {
            var playerId = "different-player-id";
            var requestId = Guid.NewGuid();

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId))
                .Throws(new UnauthorizedAccessException("O utilizador que está a tentar entrar não é o mesmo da url."));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.PostAsJsonAsync($"/api/Player/{playerId}/membership-requests/accept", requestId);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Contains.Substring("O utilizador que está a tentar entrar não é o mesmo da url."));
        }

        #endregion
    }
}