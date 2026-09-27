using Application.DTOs;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Exceptions;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Integration.ClassTests.UserIntegrationTests
{
    [TestFixture]
    internal class PlayerIntegrationTests
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

        #region GetPlayer Tests
        [Test]
        public async Task GetPlayer_Returns_PlayerDetails_When_PlayerExists()
        {
            var playerId = Guid.NewGuid().ToString();
            var expectedPlayerDetails = new PlayerDetailsDto
            {
                Name = "Test Player",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)),
                Address = "Test Address",
                Position = 0,
                Height = 180,
                Team = new TeamDto
                {
                    IdTeam = Guid.NewGuid(),
                    Name = "Test team"
                }
            };

            var mockPlayerService = new Mock<IPlayerService>();
            var mockAuthService = new Mock<IAuthService>();
            var mockValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembership = new Mock<IMembershipRequestService>(); 

            mockPlayerService.Setup(s => s.GetPlayerByIdAsync(playerId))
                .ReturnsAsync(expectedPlayerDetails);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IPlayerService>();
                    services.RemoveAll<IAuthService>();
                    services.RemoveAll<IPlayerAuthorizationValidator>();
                    services.RemoveAll<IMembershipRequestService>();

                    services.AddSingleton(mockPlayerService.Object);
                    services.AddSingleton(mockAuthService.Object);
                    services.AddSingleton(mockValidator.Object);
                    services.AddSingleton(mockMembership.Object);
                });
            }).CreateClient(); 

            // Os detalhes de um jogador exigem sessão (antes eram públicos e incluíam os contactos).
            _client.DefaultRequestHeaders.Add("Authorization", "Test");
            var response = await _client.GetAsync($"/api/Player/details/{playerId}");

            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<PlayerDetailsDto>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo(expectedPlayerDetails.Name));

            mockPlayerService.Verify(s => s.GetPlayerByIdAsync(playerId), Times.Once);
        }

        [Test]
        public async Task GetPlayer_Returns_NotFound_When_PlayerDoesNotExist()
        {
            var playerId = Guid.NewGuid().ToString();

            // Mocks
            var mockPlayerService = new Mock<IPlayerService>();
            var mockAuthService = new Mock<IAuthService>();
            var mockValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembership = new Mock<IMembershipRequestService>();

            mockPlayerService.Setup(s => s.GetPlayerByIdAsync(playerId))
                .ThrowsAsync(new NotFoundException("O Player não existe")); 

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IPlayerService>();
                    services.RemoveAll<IAuthService>();
                    services.RemoveAll<IPlayerAuthorizationValidator>();
                    services.RemoveAll<IMembershipRequestService>();

                    services.AddSingleton(mockPlayerService.Object);
                    services.AddSingleton(mockAuthService.Object);
                    services.AddSingleton(mockValidator.Object);
                    services.AddSingleton(mockMembership.Object);
                });
            }).CreateClient();

            // Os detalhes de um jogador exigem sessão (antes eram públicos e incluíam os contactos).
            _client.DefaultRequestHeaders.Add("Authorization", "Test");
            var response = await _client.GetAsync($"/api/Player/details/{playerId}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
        #endregion

        #region GetFullProfile Tests
        [Test]
        public async Task GetFullProfile_Returns_PlayerDetails_When_Authenticated()
        {
            var expectedPlayerDetails = new PlayerDetailsDto
            {
                Name = "Authenticated Player",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
                Address = "User Address",
                Position = 0,
                Height = 175,
                Team = new TeamDto
                {
                    IdTeam = Guid.NewGuid(),
                    Name = "Test Team"
                }
            };

            var mockPlayerService = new Mock<IPlayerService>();

            mockPlayerService.Setup(s => s.GetPlayerByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedPlayerDetails);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.AddSingleton(mockPlayerService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.GetAsync("/api/Player/get-my-profile");

            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<PlayerDetailsDto>();
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Name, Is.EqualTo(expectedPlayerDetails.Name));
                Assert.That(result.DateOfBirth, Is.EqualTo(expectedPlayerDetails.DateOfBirth));
                Assert.That(result.Position, Is.EqualTo(expectedPlayerDetails.Position));
            });

            mockPlayerService.Verify(s => s.GetPlayerByIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task GetFullProfile_Returns_Unauthorized_When_NotAuthenticated()
        {
            var mockPlayerService = new Mock<IPlayerService>();

            mockPlayerService.Setup(s => s.GetPlayerByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new PlayerDetailsDto());

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.AddSingleton(mockPlayerService.Object);
                });
            }).CreateClient();

            var response = await _client.GetAsync("/api/Player/get-my-profile");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

            mockPlayerService.Verify(s => s.GetPlayerByIdAsync(It.IsAny<string>()), Times.Never);
        }
        #endregion

        #region UpdateUser Tests
        [Test]
        public async Task UpdateUser_Returns_Success_When_ValidRequest()
        {
            // Arrange
            var playerId = TestAuthHandler.TestUserId;
            var updateDto = new UpdatePlayerDto
            {
                Name = "Test Name",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-20),
                Address = "Rua teste, teste",
                Email = "test@example.com",
                Phone = "+351123456789",
                Position = 0,
                Height = 170
            };

            var returnedDto = new UpdatePlayerDto
            {
                Name = updateDto.Name,
            };

            var mockPlayerService = new Mock<IPlayerService>();
            var mockAuthService = new Mock<IAuthService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembership = new Mock<IMembershipRequestService>(); 

            mockPlayerService.Setup(s => s.UpdatePlayerAsync(playerId, It.IsAny<UpdatePlayerDto>()))
                .ReturnsAsync(returnedDto);

            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId))
                .Verifiable();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IPlayerService>();
                    services.RemoveAll<IAuthService>();
                    services.RemoveAll<IPlayerAuthorizationValidator>();
                    services.RemoveAll<IMembershipRequestService>(); 

                    services.AddSingleton(mockPlayerService.Object);
                    services.AddSingleton(mockAuthService.Object);
                    services.AddSingleton(mockPlayerAuthValidator.Object);
                    services.AddSingleton(mockMembership.Object); 
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.PutAsJsonAsync($"/api/Player/update/{playerId}", updateDto);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Falhou com {response.StatusCode}. Detalhes: {errorContent}");
            }
            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<UpdatePlayerDto>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo(updateDto.Name));

            mockPlayerService.Verify(s => s.UpdatePlayerAsync(playerId, It.IsAny<UpdatePlayerDto>()), Times.Once);
        }
      
        [Test]
        public async Task UpdateUser_Returns_BadRequest_When_InvalidModel()
        {
            var playerId = Guid.NewGuid().ToString();
            var invalidDto = new UpdatePlayerDto
            {
                Name = "", 
                Email = "invalid-email",
                Phone = "invalid-phone",
                Height = -5
            };

            var mockPlayerService = new Mock<IPlayerService>();
            var mockAuthService = new Mock<IAuthService>();
            var mockValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembership = new Mock<IMembershipRequestService>(); 

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IPlayerService>();
                    services.RemoveAll<IAuthService>();
                    services.RemoveAll<IPlayerAuthorizationValidator>();
                    services.RemoveAll<IMembershipRequestService>();

                    services.AddSingleton(mockPlayerService.Object);
                    services.AddSingleton(mockAuthService.Object);
                    services.AddSingleton(mockValidator.Object);
                    services.AddSingleton(mockMembership.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // CORREÇÃO DA ROTA: "/update/"
            var response = await _client.PutAsJsonAsync($"/api/Player/update/{playerId}", invalidDto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task UpdateUser_Returns_Forbid_When_PlayerIdDoesNotMatchAuthenticatedUser()
        {
            var playerId = "different-player-id";
            var updateDto = new UpdatePlayerDto
            {
                Name = "Test Name",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-20),
                Address = "Rua teste",
                Email = "test@example.com",
                Phone = "+351912345678",
                Position = 0,
                Height = 170
            };

            var mockPlayerService = new Mock<IPlayerService>();
            var mockAuthService = new Mock<IAuthService>();
            var mockValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockMembership = new Mock<IMembershipRequestService>();

            mockValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId))
                        .Throws(new ForbiddenException("Só podes aceder aos teus próprios dados."));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // 2. Injeção de dependências completa
                    services.RemoveAll<IPlayerService>();
                    services.RemoveAll<IAuthService>();
                    services.RemoveAll<IPlayerAuthorizationValidator>();
                    services.RemoveAll<IMembershipRequestService>();

                    services.AddSingleton(mockPlayerService.Object);
                    services.AddSingleton(mockAuthService.Object);
                    services.AddSingleton(mockValidator.Object);
                    services.AddSingleton(mockMembership.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.PutAsJsonAsync($"/api/Player/update/{playerId}", updateDto);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine(error);
            }

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        #endregion

        #region LeaveTeam Tests
        [Test]
        public async Task LeaveTeam_Returns_Success_When_PlayerLeavesTeam()
        {
            // Arrange
            var playerId = Guid.NewGuid().ToString();

            // O serviço retorna um objeto DTO, não uma string
            var returnedPlayerDto = new InfoPlayerDto
            {
                Id = playerId,
                Name = "Player Name",
                HaveTeam = false // Importante: simula que saiu da equipa
            };

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();

            // Mock da validação
            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId))
                .Verifiable();

            // Mock do serviço a retornar o DTO
            mockPlayerService.Setup(s => s.LeaveTeam(playerId))
                .ReturnsAsync(returnedPlayerDto);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));

                    services.AddSingleton(mockPlayerService.Object);
                    services.AddSingleton(mockPlayerAuthValidator.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.PutAsync($"/api/Player/{playerId}/leave-team", null);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // CORREÇÃO: Ler o objeto JSON devolvido
            var result = await response.Content.ReadFromJsonAsync<InfoPlayerDto>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(playerId));
            Assert.That(result.HaveTeam, Is.False, "O DTO deve indicar que o jogador não tem equipa");

            mockPlayerAuthValidator.Verify(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId), Times.Once);
            mockPlayerService.Verify(s => s.LeaveTeam(playerId), Times.Once);
        }

        [Test]
        public async Task LeaveTeam_Returns_Unauthorized_When_NotAuthenticated()
        {
            var playerId = Guid.NewGuid().ToString();
            _client = _factory.CreateClient();

            var response = await _client.PutAsync($"/api/Player/{playerId}/leave-team", null);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task LeaveTeam_Returns_Forbid_When_PlayerNotAuthorized()
        {
            // Arrange
            var playerId = "unauthorized-player-id";

            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId))
                .Throws(new ForbiddenException("Só podes aceder aos teus próprios dados."));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.AddSingleton(mockPlayerAuthValidator.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.PutAsync($"/api/Player/{playerId}/leave-team", null);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }
        #endregion

        #region CreatePlayer and DeletePlayer Tests (existing - mantidos para referência)
        [Test]
        public async Task CreatePlayer_Returns_Created_With_Login_Response()
        {
            var mockPlayerService = new Mock<IPlayerService>();
            var mockAuthService = new Mock<IAuthService>();

            var playerDto = new CreatePlayerDto
            {
                Name = "Test",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-20),
                Address = "Street Test, Test",
                Email = "test@example.com",
                Password = "Teste#Seguro123",
                AceitaPoliticaPrivacidade = true,
                VersaoPoliticaPrivacidade = Application.Services.ContaService.VersaoPoliticaPredefinida,
                Phone = "+351123456789",
                Position = 0,
                Height = 160
            };
            string expectedPlayerId = Guid.NewGuid().ToString();

            var expectedLoginResponse = new LoginResponseDto
            {
                Name = playerDto.Name,
                DateOfBirth = playerDto.DateOfBirth,
                Address = playerDto.Address,
                Email = playerDto.Email,
                Phone = playerDto.Phone,
                CreationDate = DateTime.UtcNow,
            };

            mockPlayerService.Setup(s => s.CreatePlayerAsync(It.IsAny<CreatePlayerDto>()))
                .ReturnsAsync(expectedPlayerId);

            // Sem Auth:RequireVerifiedEmail o serviço devolve logo a sessão.
            mockAuthService.Setup(a => a.IniciarSessaoAposRegistoAsync(playerDto.Email, playerDto.Password))
                .ReturnsAsync(expectedLoginResponse);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IAuthService));

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            var response = await _client.PostAsJsonAsync("/api/Player/create-profile", playerDto);

            Assert.That((int)response.StatusCode, Is.InRange(200, 299));

            var responseContent = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.That(responseContent, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(responseContent.Name, Is.EqualTo(expectedLoginResponse.Name));
                Assert.That(responseContent.Email, Is.EqualTo(expectedLoginResponse.Email));
                Assert.That(responseContent.Phone, Is.EqualTo(expectedLoginResponse.Phone));
            });

            mockPlayerService.Verify(s => s.CreatePlayerAsync(It.IsAny<CreatePlayerDto>()), Times.Once);
            mockAuthService.Verify(a => a.IniciarSessaoAposRegistoAsync(playerDto.Email, playerDto.Password), Times.Once);
        }

        [Test]
        public async Task CreatePlayer_Returns_PendingVerification_Without_Session()
        {
            var mockPlayerService = new Mock<IPlayerService>();
            var mockAuthService = new Mock<IAuthService>();
            var playerDto = CreatePlayerDtoValido();

            mockPlayerService.Setup(s => s.CreatePlayerAsync(It.IsAny<CreatePlayerDto>())).ReturnsAsync("novo-jogador");
            // Com a confirmação do e-mail exigida, não há sessão depois do registo.
            mockAuthService.Setup(a => a.IniciarSessaoAposRegistoAsync(playerDto.Email, playerDto.Password))
                .ReturnsAsync((LoginResponseDto?)null);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IAuthService));
                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            var response = await _client.PostAsJsonAsync("/api/Player/create-profile", playerDto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var corpo = await response.Content.ReadAsStringAsync();
            Assert.That(corpo, Does.Contain("\"verificacaoEmailPendente\":true"));
            Assert.That(corpo, Does.Not.Contain("idToken"));
        }

        [TestCase("curta#1A")]
        [TestCase("semsimbolos1234A")]
        [TestCase("SEM#MINUSCULAS123")]
        [TestCase("sem#maiusculas123")]
        [TestCase("Sem#Algarismos!!")]
        public async Task CreatePlayer_Returns_BadRequest_When_Password_Is_Weak(string password)
        {
            var mockPlayerService = new Mock<IPlayerService>();
            var playerDto = CreatePlayerDtoValido();
            playerDto.Password = password;

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                });
            }).CreateClient();

            var response = await _client.PostAsJsonAsync("/api/Player/create-profile", playerDto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            mockPlayerService.Verify(s => s.CreatePlayerAsync(It.IsAny<CreatePlayerDto>()), Times.Never);
        }

        [Test]
        public async Task CreatePlayer_Returns_BadRequest_When_Honeypot_Is_Filled()
        {
            var mockPlayerService = new Mock<IPlayerService>();
            var playerDto = CreatePlayerDtoValido();
            playerDto.Website = "http://spam.example";

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                });
            }).CreateClient();

            var response = await _client.PostAsJsonAsync("/api/Player/create-profile", playerDto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            mockPlayerService.Verify(s => s.CreatePlayerAsync(It.IsAny<CreatePlayerDto>()), Times.Never);
        }

        private static CreatePlayerDto CreatePlayerDtoValido() => new()
        {
            Name = "Teste",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-20),
            Address = "Rua de Teste, Guimarães",
            Email = "novo@example.com",
            Password = "Teste#Seguro123",
            Phone = "+351912345678",
            Position = 0,
            Height = 175,
            AceitaPoliticaPrivacidade = true,
            VersaoPoliticaPrivacidade = Application.Services.ContaService.VersaoPoliticaPredefinida,
        };

        [TestCase(false, Application.Services.ContaService.VersaoPoliticaPredefinida)]
        [TestCase(true, "2020-01-01")]
        [TestCase(true, null)]
        public async Task CreatePlayer_Sem_Aceitar_A_Politica_Atual_Responde_400(bool aceita, string? versao)
        {
            var mockPlayerService = new Mock<IPlayerService>();
            _client = _factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IPlayerService));
                services.AddSingleton(mockPlayerService.Object);
            })).CreateClient();

            var dto = CreatePlayerDtoValido();
            dto.AceitaPoliticaPrivacidade = aceita;
            dto.VersaoPoliticaPrivacidade = versao;

            var response = await _client.PostAsJsonAsync("/api/Player/create-profile", dto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("Política de Privacidade"));
            mockPlayerService.Verify(s => s.CreatePlayerAsync(It.IsAny<CreatePlayerDto>()), Times.Never);
        }

        [Test]
        public async Task DeletePlayer_Antigo_Responde_410_E_Nao_Apaga()
        {
            var mockPlayerService = new Mock<IPlayerService>();
            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.AddSingleton(mockPlayerService.Object);
                });
            }).CreateClient();
            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.DeleteAsync($"/api/Player/{TestAuthHandler.TestUserId}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Gone));
            mockPlayerService.Verify(s => s.DeletePlayerAsync(It.IsAny<string>()), Times.Never);
        }
        #endregion

        #endregion
    }
}
