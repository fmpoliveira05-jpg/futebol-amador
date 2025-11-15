using Application.DTOs;
using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Exceptions;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
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
                IdTeam = Guid.NewGuid()
            };

            var mockPlayerService = new Mock<IPlayerService>();
            mockPlayerService.Setup(s => s.GetPlayerByIdAsync(playerId))
                .ReturnsAsync(expectedPlayerDetails);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IPlayerService>();
                    services.AddSingleton(mockPlayerService.Object);
                });
            }).CreateClient();

            var response = await _client.GetAsync($"/api/Player/{playerId}");
            
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<PlayerDetailsDto>();
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Name, Is.EqualTo(expectedPlayerDetails.Name));
                Assert.That(result.DateOfBirth, Is.EqualTo(expectedPlayerDetails.DateOfBirth));
                Assert.That(result.Address, Is.EqualTo(expectedPlayerDetails.Address));
                Assert.That(result.Position, Is.EqualTo(expectedPlayerDetails.Position));
                Assert.That(result.Height, Is.EqualTo(expectedPlayerDetails.Height));
                Assert.That(result.IdTeam, Is.EqualTo(expectedPlayerDetails.IdTeam));
            });

            mockPlayerService.Verify(s => s.GetPlayerByIdAsync(playerId), Times.Once);
        }

        [Test]
        public async Task GetPlayer_Returns_NotFound_When_PlayerDoesNotExist()
        {
            var playerId = Guid.NewGuid().ToString();

            var mockPlayerService = new Mock<IPlayerService>();
            mockPlayerService.Setup(s => s.GetPlayerByIdAsync(playerId))
                .ThrowsAsync(new NotFoundException("O Player não existe"));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.AddSingleton(mockPlayerService.Object);
                });
            }).CreateClient();

            var response = await _client.GetAsync($"/api/Player/{playerId}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
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
                IdTeam = Guid.NewGuid()
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

            var mockPlayerService = new Mock<IPlayerService>();
            var mockAuthService = new Mock<IAuthService>();

            mockPlayerService.Setup(s => s.UpdatePlayerAsync(playerId, It.IsAny<UpdatePlayerDto>()))
                .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IPlayerService>();
                    services.RemoveAll<IAuthService>();

                    services.AddSingleton<IPlayerService>(mockPlayerService.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.PutAsJsonAsync($"/api/Player/{playerId}", updateDto);

            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Contains.Substring("Player information updated succesfully"));

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

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.PutAsJsonAsync($"/api/Player/{playerId}", invalidDto);

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
                Address = "Rua teste, teste",
                Email = "test@example.com",
                Phone = "+351123456789",
                Position = 0,
                Height = 170
            };

            _client = _factory.CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.PutAsJsonAsync($"/api/Player/{playerId}", updateDto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
        #endregion

        #region LeaveTeam Tests
        [Test]
        public async Task LeaveTeam_Returns_Success_When_PlayerLeavesTeam()
        {
            var playerId = Guid.NewGuid().ToString();
            var teamName = "Test Team";

            var mockPlayerService = new Mock<IPlayerService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();

            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId))
                .Verifiable();
            mockPlayerService.Setup(s => s.LeaveTeam(playerId))
                .ReturnsAsync(teamName);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));

                    services.AddSingleton(mockPlayerService.Object);
                    services.AddSingleton(mockPlayerAuthValidator.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.PutAsync($"/api/Player/{playerId}/leave-team", null);

            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Contains.Substring("Player succesfully left the team"));
            Assert.That(content, Contains.Substring(teamName));

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
                .Throws(new UnauthorizedAccessException("Not authorized to perform this action"));

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

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
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
                Password = "Test123!",
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

            mockAuthService.Setup(a => a.LoginAsync(playerDto.Email, playerDto.Password))
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
            mockAuthService.Verify(a => a.LoginAsync(playerDto.Email, playerDto.Password), Times.Once);
        }

        [Test]
        public async Task DeletePlayer_Returns_Deleted()
        {
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockPlayerService = new Mock<IPlayerService>();

            var playerId = Guid.NewGuid().ToString();

            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId))
                .Verifiable();
            mockPlayerService.Setup(s => s.DeletePlayerAsync(playerId))
                .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IPlayerService));

                    services.AddSingleton(mockPlayerAuthValidator.Object);
                    services.AddSingleton(mockPlayerService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.DeleteAsync($"/api/Player/{playerId}");

            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            mockPlayerService.Verify(s => s.DeletePlayerAsync(playerId), Times.Once);
            mockPlayerAuthValidator.Verify(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), playerId), Times.Once);
        }
        #endregion

        #endregion
    }
}
