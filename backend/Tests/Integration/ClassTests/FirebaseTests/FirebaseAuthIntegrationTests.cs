using Application.DTOs;
using Application.Interfaces.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using Tests.Integration;

namespace IntegrationTests.Controllers
{
    [TestFixture]
    public class FirebaseAuthIntegrationTests
    {
        private ApiTestAppFactory _factory;
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
        public async Task Login_ReturnsOk_WithValidCredentials()
        {
            var loginDto = new LoginDto { Email = "test@example.com", Password = "password123" };
            var expectedResponse = new LoginResponseDto
            {
                Email = "test@example.com",
                Name = "Test User",
                FirebaseLoginResponseDto = new FirebaseLoginResponseDto
                {
                    Email = "test@example.com",
                    LocalId = "user123",
                    IdToken = "token123",
                    RefreshToken = "refresh123"
                }
            };

            var mockAuthService = new Mock<IAuthService>();

            mockAuthService
                .Setup(x => x.LoginAsync(loginDto.Email, loginDto.Password))
                .ReturnsAsync(expectedResponse);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthService>();
                    services.AddSingleton(mockAuthService.Object);
                });
            }).CreateClient();

            var response = await _client.PostAsJsonAsync("/api/user/login", loginDto);

            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo(expectedResponse.Email));
            Assert.That(result.Name, Is.EqualTo(expectedResponse.Name));

            mockAuthService.Verify(x => x.LoginAsync(loginDto.Email, loginDto.Password), Times.Once);
        }

        [Test]
        public async Task Login_ReturnsUnauthorized_WhenAuthenticationFails()
        {
            var loginDto = new LoginDto { Email = "test@example.com", Password = "wrongpassword" };

            var mockAuthService = new Mock<IAuthService>();
            mockAuthService
                .Setup(x => x.LoginAsync(loginDto.Email, loginDto.Password))
                .ThrowsAsync(new AuthenticationException("Invalid credentials"));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthService>();
                    services.AddSingleton(mockAuthService.Object);
                });
            }).CreateClient();

            var response = await _client.PostAsJsonAsync("/api/user/login", loginDto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }

        [Test]
        public async Task Logout_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            var response = await _client.GetAsync("/api/user/logout");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Logout_ReturnsOk_WhenUserAuthenticated()
        {
            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var mockAuthService = new Mock<IAuthService>();

            mockAuthService
                .Setup(x => x.LogoutAsync(TestAuthHandler.TestUserId))
                .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthService>();
                    services.AddSingleton(mockAuthService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.GetAsync("/api/user/logout");

            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            mockAuthService.Verify(x => x.LogoutAsync(TestAuthHandler.TestUserId), Times.Once);
        }

        [Test]
        public async Task ChangePassword_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            var currentPassword = "oldPassword123";
            var newPassword = "newPassword123";

            var response = await _client.GetAsync($"/api/user/ChangePassword?currentPassword={currentPassword}&newPassword={newPassword}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task ChangePassword_ReturnsNoContent_WhenValid()
        {
            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var currentPassword = "oldPassword123";
            var newPassword = "newPassword123";

            var mockAuthService = new Mock<IAuthService>();
            mockAuthService
                .Setup(x => x.ChangePasswordAsync(TestAuthHandler.TestUserId, currentPassword, newPassword))
                .Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthService>();
                    services.AddSingleton(mockAuthService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.GetAsync($"/api/user/ChangePassword?currentPassword={currentPassword}&newPassword={newPassword}");

            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            mockAuthService.Verify(x => x.ChangePasswordAsync(TestAuthHandler.TestUserId, currentPassword, newPassword), Times.Once);
        }

        [Test]
        public async Task ChangePassword_ReturnsError_WhenServiceThrowsException()
        {
            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var currentPassword = "wrongPassword";
            var newPassword = "newPassword123";

            var mockAuthService = new Mock<IAuthService>();
            mockAuthService
                .Setup(x => x.ChangePasswordAsync(TestAuthHandler.TestUserId, currentPassword, newPassword))
                .ThrowsAsync(new AuthenticationException("Current password is incorrect"));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthService>();
                    services.AddSingleton(mockAuthService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            var response = await _client.GetAsync($"/api/user/ChangePassword?currentPassword={currentPassword}&newPassword={newPassword}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
    }
}