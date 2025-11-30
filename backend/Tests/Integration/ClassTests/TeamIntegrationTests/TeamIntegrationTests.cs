using Application.DTOs.Filters;
using Application.DTOs.Pitch;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration.ClassTests.TeamIntegrationTests
{
    [TestFixture]
    internal class TeamIntegrationTests
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

        #region Tests

        #region CreateTeam Tests
        [Test]
        public async Task CreateTeam_Returns_Created_With_TeamId()
        {
            // Arrange
            var teamDto = new CreateTeamDto
            {
                Name = "Test Team",
                Description = "Test Description",
                HomePitch = new PitchDto { Name = "Test Pitch", Address = "Test Address" }
            };
            var expectedTeamId = Guid.NewGuid();

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockTeamService.Setup(s => s.CreateTeamAsync(It.IsAny<CreateTeamDto>(), It.IsAny<string>()))
                .ReturnsAsync(expectedTeamId);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.PostAsJsonAsync("/api/Team", teamDto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var responseContent = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.That(responseContent, Is.Not.Null);

            mockTeamService.Verify(s => s.CreateTeamAsync(It.IsAny<CreateTeamDto>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task CreateTeam_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var teamDto = new CreateTeamDto
            {
                Name = "Test Team",
                Description = "Test Description",
                HomePitch = new PitchDto { Name = "Test Pitch", Address = "Test Address" }
            };

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.PostAsJsonAsync("/api/Team", teamDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        #endregion

        #region GetTeamById Tests
        [Test]
        public async Task GetTeamById_Returns_TeamDetails_When_TeamExists()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var expectedTeam = new TeamDetailsDto
            {
                Id = teamId,
                Name = "Test Team",
                Description = "Test Description",
                FoundationDate = DateOnly.FromDateTime(DateTime.UtcNow),
                TotalPoints = 0,
                RankName = "Gold",
                // CORREÇÃO: PitchDto agora é um objeto completo, não uma string
                PitchDto = new PitchDto
                {
                    Name = "Main Pitch",
                    Address = "Stadium Road 123"
                },
                Players = new List<PlayerDetailsDto>()
            };

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockTeamService.Setup(s => s.GetTeamByIdAsync(teamId))
                .ReturnsAsync(expectedTeam);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton(mockTeamService.Object);
                    services.AddSingleton(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Act
            var response = await _client.GetAsync($"/api/Team/{teamId}");

            // Assert
            if (!response.IsSuccessStatusCode) Assert.Fail(await response.Content.ReadAsStringAsync());
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<TeamDetailsDto>();
            Assert.That(result, Is.Not.Null);

            // Verificações Detalhadas
            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(expectedTeam.Id));
                Assert.That(result.Name, Is.EqualTo(expectedTeam.Name));
                Assert.That(result.Description, Is.EqualTo(expectedTeam.Description));
                Assert.That(result.FoundationDate, Is.EqualTo(expectedTeam.FoundationDate));
                Assert.That(result.TotalPoints, Is.EqualTo(expectedTeam.TotalPoints));
                Assert.That(result.RankName, Is.EqualTo(expectedTeam.RankName));

                // Verificar propriedade aninhada do PitchDto
                Assert.That(result.PitchDto.Name, Is.EqualTo(expectedTeam.PitchDto.Name));
                Assert.That(result.PitchDto.Address, Is.EqualTo(expectedTeam.PitchDto.Address));

                Assert.That(result.Players, Is.Empty);
            });

            mockTeamService.Verify(s => s.GetTeamByIdAsync(teamId), Times.Once);
        }

        [Test]
        public async Task GetTeamById_Returns_NotFound_When_TeamDoesNotExist()
        {
            // Arrange
            var teamId = Guid.NewGuid();

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockTeamService.Setup(s => s.GetTeamByIdAsync(teamId))
                .ThrowsAsync(new NotFoundException("Team not found"));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Act
            var response = await _client.GetAsync($"/api/Team/{teamId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
        #endregion

        #region UpdateTeamInfo Tests
        [Test]
        public async Task UpdateTeamInfo_Returns_Success_When_ValidRequest()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var updateDto = new CreateTeamDto
            {
                Name = "Updated Team Name",
                Description = "Updated Description",
                HomePitch = new PitchDto
                {
                    Name = "Updated Pitch",
                    Address = "Updated Address"
                }
            };

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockTeamService.Setup(s => s.UpdateTeamInfoAsync(teamId, It.IsAny<CreateTeamDto>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.PutAsJsonAsync($"/api/Team/{teamId}", updateDto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Contains.Substring("Equipa atualizada com sucesso"));

            mockTeamService.Verify(s => s.UpdateTeamInfoAsync(teamId, It.IsAny<CreateTeamDto>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task UpdateTeamInfo_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var updateDto = new CreateTeamDto
            {
                Name = "Updated Team Name",
                Description = "Updated Description"
            };

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.PutAsJsonAsync($"/api/Team/{teamId}", updateDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        #endregion

        #region DeleteTeam Tests
        [Test]
        public async Task DeleteTeam_Returns_NoContent_When_Successful()
        {
            // Arrange
            var teamId = Guid.NewGuid();

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockTeamService.Setup(s => s.DeleteTeamAsync(teamId, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.DeleteAsync($"/api/Team/{teamId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            mockTeamService.Verify(s => s.DeleteTeamAsync(teamId, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task DeleteTeam_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var teamId = Guid.NewGuid();

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.DeleteAsync($"/api/Team/{teamId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        #endregion

        #region SearchTeams Tests
        [Test]
        public async Task SearchTeams_Returns_Teams_When_NoFilters()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var expectedTeams = new List<InfoTeamsDto>
            {
                new InfoTeamsDto { Id = Guid.NewGuid(), Name = "Team 1", Description = "test desc1", Address = "City1", CurrentPoints = 100, AverageAge = 20, PlayerCount = 10 },
                new InfoTeamsDto { Id = Guid.NewGuid(), Name = "Team 2", Description = "test desc2", Address = "City2", CurrentPoints = 80, AverageAge = 23, PlayerCount = 12 },
            };

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockTeamService.Setup(s => s.SearchTeamsAsync(teamId))
                .ReturnsAsync(expectedTeams);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Act
            var response = await _client.GetAsync($"/api/Team/{teamId}/search");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<List<InfoTeamsDto>>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));

            mockTeamService.Verify(s => s.SearchTeamsAsync(teamId), Times.Once);
        }

        [Test]
        public async Task SearchTeams_Returns_Teams_When_WithFilters()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var expectedTeams = new List<InfoTeamsDto>
            {
                new InfoTeamsDto { Id = Guid.NewGuid(), Name = "Team 1", Description = "test desc1", Address = "City1", CurrentPoints = 100, AverageAge = 20, PlayerCount = 10 },
            };

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockTeamService.Setup(s => s.SearchTeamsWithFiltersAsync(teamId, It.IsAny<FilterListTeamDto>()))
                .ReturnsAsync(expectedTeams);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Act
            var response = await _client.GetAsync($"/api/Team/{teamId}/search?NameTeam=Test&City=TestCity&MinNumberPoints=0&MaxNumberPoints=100");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<List<InfoTeamsDto>>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));

            mockTeamService.Verify(s => s.SearchTeamsWithFiltersAsync(teamId, It.IsAny<FilterListTeamDto>()), Times.Once);
        }
        #endregion

        #region GetTeamPlayersWithFilters Tests
        [Test]
        public async Task GetTeamPlayersWithFilters_Returns_Players_When_NoFilters()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var expectedPlayers = new List<PlayerDetailsDto>
            {
                new PlayerDetailsDto { Name = "Player1", Height = 180, Position = 0, IsAdmin = true },
                new PlayerDetailsDto { Name = "Player2", Height = 170, Position = 0, IsAdmin = false }
            };

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockTeamService.Setup(s => s.GetTeamPlayersAsync(teamId))
                .ReturnsAsync(expectedPlayers);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Act
            var response = await _client.GetAsync($"/api/Team/{teamId}/members");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<List<PlayerDetailsDto>>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));

            mockTeamService.Verify(s => s.GetTeamPlayersAsync(teamId), Times.Once);
        }

        [Test]
        public async Task GetTeamPlayersWithFilters_Returns_Players_When_WithFilters()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var expectedPlayers = new List<PlayerDetailsDto>
            {
                new PlayerDetailsDto { Name = "Player1", Height = 180, Position = 0, IsAdmin = true }
            };

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockTeamService.Setup(s => s.GetTeamPlayersAsyncWithFilters(teamId, It.IsAny<FilterTeamPlayers>()))
                .ReturnsAsync(expectedPlayers);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Act
            var response = await _client.GetAsync($"/api/Team/{teamId}/members?IsAdmin=true&Name=Player1");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<List<PlayerDetailsDto>>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));

            mockTeamService.Verify(s => s.GetTeamPlayersAsyncWithFilters(teamId, It.IsAny<FilterTeamPlayers>()), Times.Once);
        }
        #endregion

        #region RemovePlayerFromTeam Tests
        [Test]
        public async Task RemovePlayerFromTeam_Returns_NoContent_When_Successful()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var playerIdToRemove = "player-to-remove-id";

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockTeamService.Setup(s => s.RemovePlayerFromTeamAsync(teamId, playerIdToRemove, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.DeleteAsync($"/api/Team/{teamId}/members/{playerIdToRemove}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            mockTeamService.Verify(s => s.RemovePlayerFromTeamAsync(teamId, playerIdToRemove, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task RemovePlayerFromTeam_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var playerIdToRemove = "player-to-remove-id";

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.DeleteAsync($"/api/Team/{teamId}/members/{playerIdToRemove}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        #endregion

        #region PromotePlayerToAdmin Tests
        [Test]
        public async Task PromotePlayerToAdmin_Returns_Success_When_ValidRequest()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var playerIdToPromote = "player-to-promote-id";

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockTeamService.Setup(s => s.PromotePlayerToAdminAsync(teamId, playerIdToPromote, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.PutAsync($"/api/Team/{teamId}/members/promote/{playerIdToPromote}", null);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Contains.Substring("Jogador promovido a admin"));

            mockTeamService.Verify(s => s.PromotePlayerToAdminAsync(teamId, playerIdToPromote, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task PromotePlayerToAdmin_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var playerIdToPromote = "player-to-promote-id";

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.PutAsync($"/api/Team/{teamId}/members/promote/{playerIdToPromote}", null);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        #endregion

        #region DemoteAdminToPlayer Tests
        [Test]
        public async Task DemoteAdminToPlayer_Returns_Success_When_ValidRequest()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var adminIdToDemote = "admin-to-demote-id";

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            mockTeamService.Setup(s => s.DemoteAdminToPlayerAsync(teamId, adminIdToDemote, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.PutAsync($"/api/Team/{teamId}/members/demote/{adminIdToDemote}", null);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Contains.Substring("Admin rebaixado a jogador"));

            mockTeamService.Verify(s => s.DemoteAdminToPlayerAsync(teamId, adminIdToDemote, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task DemoteAdminToPlayer_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var adminIdToDemote = "admin-to-demote-id";

            var mockTeamService = new Mock<ITeamService>();
            var mockMembershipRequestService = new Mock<IMembershipRequestService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ITeamService));
                    services.RemoveAll(typeof(IMembershipRequestService));

                    services.AddSingleton<ITeamService>(mockTeamService.Object);
                    services.AddSingleton<IMembershipRequestService>(mockMembershipRequestService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.PutAsync($"/api/Team/{teamId}/members/demote/{adminIdToDemote}", null);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        #endregion

        #endregion
    }
}