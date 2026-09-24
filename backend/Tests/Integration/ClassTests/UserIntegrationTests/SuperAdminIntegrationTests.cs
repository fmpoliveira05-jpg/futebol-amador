using Application.DTOs;
using Application.DTOs.SuperAdmin;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Exceptions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration.ClassTests.SuperAdminIntegrationTests
{
    [TestFixture]
    internal class SuperAdminIntegrationTests
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

        #region GetSuperAdmin Tests
        [Test]
        public async Task GetSuperAdmin_Returns_SuperAdminDetails_When_SuperAdminExists()
        {
            // Arrange
            var superAdminId = TestAuthHandler.TestUserId;
            var expectedSuperAdminDetails = new SuperAdminDetailsDTO
            {
                Name = "Test Super Admin",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Address = "Test Address"
            };

            var mockSuperAdminService = new Mock<ISuperAdminService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockAuthService = new Mock<IAuthService>();

            mockSuperAdminService.Setup(s => s.GetSuperAdminByIdAsync(superAdminId))
                .ReturnsAsync(expectedSuperAdminDetails);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ISuperAdminService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IAuthService));

                    services.AddSingleton<ISuperAdminService>(mockSuperAdminService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.GetAsync($"/api/SuperAdmin/{superAdminId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<SuperAdminDetailsDTO>();
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Name, Is.EqualTo(expectedSuperAdminDetails.Name));
                Assert.That(result.DateOfBirth, Is.EqualTo(expectedSuperAdminDetails.DateOfBirth));
                Assert.That(result.Address, Is.EqualTo(expectedSuperAdminDetails.Address));
            });

            mockSuperAdminService.Verify(s => s.GetSuperAdminByIdAsync(superAdminId), Times.Once);
        }

        [Test]
        public async Task GetSuperAdmin_Returns_NotFound_When_SuperAdminDoesNotExist()
        {
            // Arrange
            var superAdminId = Guid.NewGuid().ToString();

            var mockSuperAdminService = new Mock<ISuperAdminService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockAuthService = new Mock<IAuthService>();

            mockSuperAdminService.Setup(s => s.GetSuperAdminByIdAsync(superAdminId))
                .ThrowsAsync(new NotFoundException("O Super Admin não existe"));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ISuperAdminService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IAuthService));

                    services.AddSingleton<ISuperAdminService>(mockSuperAdminService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.GetAsync($"/api/SuperAdmin/{superAdminId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }

        [Test]
        public async Task GetSuperAdmin_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var superAdminId = Guid.NewGuid().ToString();

            var mockSuperAdminService = new Mock<ISuperAdminService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockAuthService = new Mock<IAuthService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ISuperAdminService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IAuthService));

                    services.AddSingleton<ISuperAdminService>(mockSuperAdminService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.GetAsync($"/api/SuperAdmin/{superAdminId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        #endregion

        #region UpdateSuperAdmin Tests
        [Test]
        public async Task UpdateSuperAdmin_Returns_Success_When_ValidRequest()
        {
            // Arrange
            var superAdminId = TestAuthHandler.TestUserId;
            var updateDto = new UpdateSuperAdminDTO
            {
                Name = "Updated Super Admin",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-31),
                Address = "Updated Address",
                Email = "updated@example.com",
                Phone = "+351987654321"
            };

            var mockSuperAdminService = new Mock<ISuperAdminService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockAuthService = new Mock<IAuthService>();

            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), superAdminId))
                .Verifiable();

            mockSuperAdminService.Setup(s => s.UpdateSuperAdminAsync(superAdminId, It.IsAny<UpdateSuperAdminDTO>()))
                .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ISuperAdminService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IAuthService));

                    services.AddSingleton<ISuperAdminService>(mockSuperAdminService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.PutAsJsonAsync($"/api/SuperAdmin/{superAdminId}", updateDto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Contains.Substring("Super Admin information updated succesfully"));

            mockPlayerAuthValidator.Verify(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), superAdminId), Times.Once);
            mockSuperAdminService.Verify(s => s.UpdateSuperAdminAsync(superAdminId, It.IsAny<UpdateSuperAdminDTO>()), Times.Once);
        }

        [Test]
        public async Task UpdateSuperAdmin_Returns_BadRequest_When_InvalidModel()
        {
            // Arrange
            var superAdminId = TestAuthHandler.TestUserId;
            var invalidDto = new UpdateSuperAdminDTO
            {
                Name = "", // Nome vazio - inválido
                Email = "invalid-email", // Email inválido
                Phone = "invalid-phone" // Telefone inválido
            };

            var mockSuperAdminService = new Mock<ISuperAdminService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockAuthService = new Mock<IAuthService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ISuperAdminService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IAuthService));

                    services.AddSingleton<ISuperAdminService>(mockSuperAdminService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.PutAsJsonAsync($"/api/SuperAdmin/{superAdminId}", invalidDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task UpdateSuperAdmin_Returns_Unauthorized_When_SuperAdminIdDoesNotMatchAuthenticatedUser()
        {
            // Arrange
            var superAdminId = "different-superadmin-id";
            var updateDto = new UpdateSuperAdminDTO
            {
                Name = "Test Super Admin",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
                Address = "Test Address",
                Email = "test@example.com",
                Phone = "+351123456789"
            };

            var mockSuperAdminService = new Mock<ISuperAdminService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockAuthService = new Mock<IAuthService>();

            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), superAdminId));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ISuperAdminService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IAuthService));

                    services.AddSingleton<ISuperAdminService>(mockSuperAdminService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            var response = await _client.PutAsJsonAsync($"/api/SuperAdmin/{superAdminId}", updateDto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task UpdateSuperAdmin_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var superAdminId = Guid.NewGuid().ToString();
            var updateDto = new UpdateSuperAdminDTO
            {
                Name = "Test Super Admin",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
                Address = "Test Address",
                Email = "test@example.com",
                Phone = "+351123456789"
            };

            var mockSuperAdminService = new Mock<ISuperAdminService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockAuthService = new Mock<IAuthService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ISuperAdminService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IAuthService));

                    services.AddSingleton<ISuperAdminService>(mockSuperAdminService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.PutAsJsonAsync($"/api/SuperAdmin/{superAdminId}", updateDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        #endregion

        #region CreateSuperAdmin Tests
        [Test]
        public async Task CreateSuperAdmin_Returns_Created_With_Login_Response()
        {
            var superAdminDto = new CreateSuperAdminDTO
            {
                Name = "Super Admin Test",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
                Address = "Admin Address, Address",
                Email = "superadmin@example.com",
                Password = "SuperAdmin123!",
                Phone = "+351987654321"
            };

            var expectedSuperAdminId = Guid.NewGuid().ToString();

            var expectedLoginResponse = new LoginResponseDto
            {
                Name = superAdminDto.Name,
                DateOfBirth = superAdminDto.DateOfBirth,
                Address = superAdminDto.Address,
                Email = superAdminDto.Email,
                Phone = superAdminDto.Phone,
                CreationDate = DateTime.UtcNow,
            };

            var mockSuperAdminService = new Mock<ISuperAdminService>();
            var mockAuthService = new Mock<IAuthService>();

            mockSuperAdminService.Setup(s => s.CreateSuperAdminAsync(It.IsAny<CreateSuperAdminDTO>()))
                .ReturnsAsync(expectedSuperAdminId);

            mockAuthService.Setup(a => a.LoginAsync(superAdminDto.Email, superAdminDto.Password))
                .ReturnsAsync(expectedLoginResponse);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ISuperAdminService));
                    services.RemoveAll(typeof(IAuthService));

                    services.AddSingleton<ISuperAdminService>(mockSuperAdminService.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            var response = await _client.PostAsJsonAsync("/api/SuperAdmin", superAdminDto);

            Assert.That((int)response.StatusCode, Is.InRange(200, 299));

            var responseContent = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.That(responseContent, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(responseContent.Name, Is.EqualTo(expectedLoginResponse.Name));
                Assert.That(responseContent.Email, Is.EqualTo(expectedLoginResponse.Email));
                Assert.That(responseContent.Phone, Is.EqualTo(expectedLoginResponse.Phone));
            });

            mockSuperAdminService.Verify(s => s.CreateSuperAdminAsync(It.IsAny<CreateSuperAdminDTO>()), Times.Once);
            mockAuthService.Verify(a => a.LoginAsync(superAdminDto.Email, superAdminDto.Password), Times.Once);
        }

        [Test]
        public async Task CreateSuperAdmin_Returns_BadRequest_When_InvalidModel()
        {
            // Arrange
            var invalidDto = new CreateSuperAdminDTO
            {
                Name = "", // Nome vazio - inválido
                Email = "invalid-email", // Email inválido
                Password = "short", // Senha muito curta
                Phone = "invalid-phone" // Telefone inválido
            };

            var mockSuperAdminService = new Mock<ISuperAdminService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockAuthService = new Mock<IAuthService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ISuperAdminService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IAuthService));

                    services.AddSingleton<ISuperAdminService>(mockSuperAdminService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            // Act
            var response = await _client.PostAsJsonAsync("/api/SuperAdmin", invalidDto);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        #endregion

        #region DeleteSuperAdmin Tests
        [Test]
        public async Task DeleteSuperAdmin_Returns_NoContent_When_Successful()
        {
            // Arrange
            var superAdminId = TestAuthHandler.TestUserId;

            var mockSuperAdminService = new Mock<ISuperAdminService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockAuthService = new Mock<IAuthService>();

            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), superAdminId))
                .Verifiable();

            mockSuperAdminService.Setup(s => s.DeleteSuperAdminAsync(superAdminId))
                .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ISuperAdminService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IAuthService));

                    services.AddSingleton<ISuperAdminService>(mockSuperAdminService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.DeleteAsync($"/api/SuperAdmin/{superAdminId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            mockPlayerAuthValidator.Verify(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), superAdminId), Times.Once);
            mockSuperAdminService.Verify(s => s.DeleteSuperAdminAsync(superAdminId), Times.Once);
        }

        [Test]
        public async Task DeleteSuperAdmin_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var superAdminId = Guid.NewGuid().ToString();

            var mockSuperAdminService = new Mock<ISuperAdminService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockAuthService = new Mock<IAuthService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ISuperAdminService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IAuthService));

                    services.AddSingleton<ISuperAdminService>(mockSuperAdminService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.DeleteAsync($"/api/SuperAdmin/{superAdminId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task DeleteSuperAdmin_Returns_Forbid_When_SuperAdminNotAuthorized()
        {
            // Arrange
            var superAdminId = "unauthorized-superadmin-id";

            var mockSuperAdminService = new Mock<ISuperAdminService>();
            var mockPlayerAuthValidator = new Mock<IPlayerAuthorizationValidator>();
            var mockAuthService = new Mock<IAuthService>();

            mockPlayerAuthValidator.Setup(v => v.ValidateUserIdIsSameUrl(It.IsAny<string>(), superAdminId));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ISuperAdminService));
                    services.RemoveAll(typeof(IPlayerAuthorizationValidator));
                    services.RemoveAll(typeof(IAuthService));

                    services.AddSingleton<ISuperAdminService>(mockSuperAdminService.Object);
                    services.AddSingleton<IPlayerAuthorizationValidator>(mockPlayerAuthValidator.Object);
                    services.AddSingleton<IAuthService>(mockAuthService.Object);
                });
            }).CreateClient();

            var response = await _client.DeleteAsync($"/api/SuperAdmin/{superAdminId}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        #endregion

        #endregion
    }
}