using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.DTOs.Team;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration.ClassTests.MatchTests
{
    [TestFixture]
    internal class PostPoneMatchControllerIntegrationTests
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

        #region GetListPostPoneMatchTeam Tests

        [Test]
        public async Task GetListPostPoneMatchTeam_Returns_List_When_NoFilters()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var expectedList = new List<InfoPostPoneMatch>
            {
                new InfoPostPoneMatch
                {
                    IdMatch = Guid.NewGuid(),
                    GameDate = DateTime.UtcNow.AddDays(7),
                    PostPoneDate = DateTime.UtcNow.AddDays(14),
                    Team = new TeamDto { IdTeam = teamId, Name = "My Team" },
                    Opponent = new TeamDto { IdTeam = Guid.NewGuid(), Name = "Team A" }
                },
                new InfoPostPoneMatch
                {
                    IdMatch = Guid.NewGuid(),
                    GameDate = DateTime.UtcNow.AddDays(10),
                    PostPoneDate = DateTime.UtcNow.AddDays(17),
                    Team = new TeamDto { IdTeam = teamId, Name = "My Team" },
                    Opponent = new TeamDto { IdTeam = Guid.NewGuid(), Name = "Team B" }
                }
            };

            var mockMatchService = new Mock<IMatchService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockAuthorizationService.Setup(s => s.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), teamId))
                .Returns(Task.CompletedTask);

            mockMatchService.Setup(s => s.GetListPostPoneMatchTeam(teamId))
                .ReturnsAsync(expectedList);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IMatchService));
                    services.RemoveAll(typeof(IPlayerAuthorizationService));

                    services.AddSingleton(mockMatchService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.GetAsync($"/api/Team/{teamId}/PostPoneMatch");

            // Assert
            if (!response.IsSuccessStatusCode) Assert.Fail(await response.Content.ReadAsStringAsync());
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<List<InfoPostPoneMatch>>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Opponent.Name, Is.EqualTo(expectedList[0].Opponent.Name)); // Verificar propriedade aninhada

            mockAuthorizationService.Verify(s => s.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), teamId), Times.Once);
            mockMatchService.Verify(s => s.GetListPostPoneMatchTeam(teamId), Times.Once);
        }

        [Test]
        public async Task GetListPostPoneMatchTeam_Returns_List_When_WithFilters()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var expectedList = new List<InfoPostPoneMatch>
            {
                new InfoPostPoneMatch
                {
                    IdMatch = Guid.NewGuid(),
                    GameDate = DateTime.UtcNow.AddDays(7),
                    PostPoneDate = DateTime.UtcNow.AddDays(14),
                    Team = new TeamDto { IdTeam = teamId, Name = "My Team" },
                    Opponent = new TeamDto { IdTeam = Guid.NewGuid(), Name = "Team A" }
                }
            };

            var mockMatchService = new Mock<IMatchService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockAuthorizationService.Setup(s => s.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), teamId))
                .Returns(Task.CompletedTask);

            // Ajuste no Mock para aceitar qualquer filtro
            mockMatchService.Setup(s => s.GetListPostPoneMatchTeamWithFilters(teamId, It.IsAny<FilterPostPoneMatchDto>()))
                .ReturnsAsync(expectedList);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IMatchService));
                    services.RemoveAll(typeof(IPlayerAuthorizationService));

                    services.AddSingleton(mockMatchService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.GetAsync($"/api/Team/{teamId}/PostPoneMatch?NameOpponent=Team%20A&IsHome=true");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<List<InfoPostPoneMatch>>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].IdMatch, Is.EqualTo(expectedList[0].IdMatch));

            mockAuthorizationService.Verify(s => s.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), teamId), Times.Once);
            mockMatchService.Verify(s => s.GetListPostPoneMatchTeamWithFilters(teamId, It.IsAny<FilterPostPoneMatchDto>()), Times.Once);
        }

        [Test]
        public async Task GetListPostPoneMatchTeam_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var teamId = Guid.NewGuid();

            var mockMatchService = new Mock<IMatchService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IMatchService));
                    services.RemoveAll(typeof(IPlayerAuthorizationService));

                    services.AddSingleton(mockMatchService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.GetAsync($"/api/Team/{teamId}/PostPoneMatch");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GetListPostPoneMatchTeam_Returns_Error_When_UserNotAdmin()
        {
            // Arrange
            var teamId = Guid.NewGuid();

            var mockMatchService = new Mock<IMatchService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockAuthorizationService.Setup(s => s.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), teamId))
                .ThrowsAsync(new InvalidOperationException("Apenas administradores de equipa têm acesso a este recurso."));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IMatchService));
                    services.RemoveAll(typeof(IPlayerAuthorizationService));

                    services.AddSingleton(mockMatchService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.GetAsync($"/api/Team/{teamId}/PostPoneMatch");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Contains.Substring("Apenas administradores de equipa têm acesso a este recurso."));
        }

        #endregion

        #region AcceptPostponeMatch Tests

        [Test]
        public async Task AcceptPostponeMatch_Returns_MatchDto_When_Successful()
        {
            var teamId = Guid.NewGuid();
            var acceptDto = new AcceptRefusePostPoneDto
            {
                IdMatch = Guid.NewGuid(),
                IdOpponent = Guid.NewGuid()
            };

            var expectedMatchDto = new MatchDto
            {
                IdMatch = acceptDto.IdMatch,
                GameDate = DateTime.UtcNow.AddDays(14),
                NameTeam = "Test Team",
                NameOpponent = "Opponent Team",
                NamePitch = "Test Pitch"
            };

            var mockMatchService = new Mock<IMatchService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockAuthorizationService.Setup(s => s.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), teamId))
                .Returns(Task.CompletedTask);

            mockMatchService.Setup(s => s.AcceptPostPoneMatch(teamId, It.IsAny<AcceptRefusePostPoneDto>()))
                .ReturnsAsync(expectedMatchDto);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IMatchService));
                    services.RemoveAll(typeof(IPlayerAuthorizationService));

                    services.AddSingleton(mockMatchService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.PostAsJsonAsync($"/api/Team/{teamId}/PostPoneMatch/AcceptPostponeMatch", acceptDto);

            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<MatchDto>();
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IdMatch, Is.EqualTo(expectedMatchDto.IdMatch));
                Assert.That(result.NameTeam, Is.EqualTo(expectedMatchDto.NameTeam));
                Assert.That(result.NameOpponent, Is.EqualTo(expectedMatchDto.NameOpponent));
                Assert.That(result.NamePitch, Is.EqualTo(expectedMatchDto.NamePitch));
            });

            mockAuthorizationService.Verify(s => s.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), teamId), Times.Once);
            mockMatchService.Verify(s => s.AcceptPostPoneMatch(teamId, It.IsAny<AcceptRefusePostPoneDto>()), Times.Once);
        }

        [Test]
        public async Task AcceptPostponeMatch_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var acceptDto = new AcceptRefusePostPoneDto
            {
                IdMatch = Guid.NewGuid(),
                IdOpponent = Guid.NewGuid()
            };

            var mockMatchService = new Mock<IMatchService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IMatchService));
                    services.RemoveAll(typeof(IPlayerAuthorizationService));

                    services.AddSingleton(mockMatchService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.PostAsJsonAsync($"/api/Team/{teamId}/PostPoneMatch/AcceptPostponeMatch", acceptDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task AcceptPostponeMatch_Returns_Error_When_UserNotAdmin()
        {
            var teamId = Guid.NewGuid();
            var acceptDto = new AcceptRefusePostPoneDto
            {
                IdMatch = Guid.NewGuid(),
                IdOpponent = Guid.NewGuid()
            };

            var mockMatchService = new Mock<IMatchService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockAuthorizationService.Setup(s => s.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), teamId))
                .ThrowsAsync(new InvalidOperationException("Apenas administradores de equipa têm acesso a este recurso."));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IMatchService));
                    services.RemoveAll(typeof(IPlayerAuthorizationService));

                    services.AddSingleton(mockMatchService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.PostAsJsonAsync($"/api/Team/{teamId}/PostPoneMatch/AcceptPostponeMatch", acceptDto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Contains.Substring("Apenas administradores de equipa têm acesso a este recurso."));
        }

        #endregion

        #region RejectPostponeMatch Tests

        [Test]
        public async Task RejectPostponeMatch_Returns_Ok_When_Successful()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var rejectDto = new AcceptRefusePostPoneDto
            {
                IdMatch = Guid.NewGuid(),
                IdOpponent = Guid.NewGuid()
            };

            var mockMatchService = new Mock<IMatchService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockAuthorizationService.Setup(s => s.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), teamId))
                .Returns(Task.CompletedTask);

            mockMatchService.Setup(s => s.RejectPostPoneMatch(teamId, It.IsAny<AcceptRefusePostPoneDto>()))
                .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IMatchService));
                    services.RemoveAll(typeof(IPlayerAuthorizationService));

                    services.AddSingleton(mockMatchService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.DeleteAsync($"/api/Team/{teamId}/PostPoneMatch/RejectPostponeMatch");

            // Para DELETE com corpo, precisamos usar um método personalizado
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/Team/{teamId}/PostPoneMatch/RejectPostponeMatch")
            {
                Content = JsonContent.Create(rejectDto)
            };
            response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            mockAuthorizationService.Verify(s => s.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), teamId), Times.Once);
            mockMatchService.Verify(s => s.RejectPostPoneMatch(teamId, It.IsAny<AcceptRefusePostPoneDto>()), Times.Once);
        }

        [Test]
        public async Task RejectPostponeMatch_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var rejectDto = new AcceptRefusePostPoneDto
            {
                IdMatch = Guid.NewGuid(),
                IdOpponent = Guid.NewGuid()
            };

            var mockMatchService = new Mock<IMatchService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IMatchService));
                    services.RemoveAll(typeof(IPlayerAuthorizationService));

                    services.AddSingleton(mockMatchService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/Team/{teamId}/PostPoneMatch/RejectPostponeMatch")
            {
                Content = JsonContent.Create(rejectDto)
            };
            var response = await _client.SendAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task RejectPostponeMatch_Returns_Error_When_UserNotAdmin()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var rejectDto = new AcceptRefusePostPoneDto
            {
                IdMatch = Guid.NewGuid(),
                IdOpponent = Guid.NewGuid()
            };

            var mockMatchService = new Mock<IMatchService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockAuthorizationService.Setup(s => s.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), teamId))
                .ThrowsAsync(new InvalidOperationException("Apenas administradores de equipa têm acesso a este recurso."));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IMatchService));
                    services.RemoveAll(typeof(IPlayerAuthorizationService));

                    services.AddSingleton(mockMatchService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act & Assert
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/Team/{teamId}/PostPoneMatch/RejectPostponeMatch")
            {
                Content = JsonContent.Create(rejectDto)
            };

            var response = await _client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Contains.Substring("Apenas administradores de equipa têm acesso a este recurso."));
        }

        #endregion
    }
}