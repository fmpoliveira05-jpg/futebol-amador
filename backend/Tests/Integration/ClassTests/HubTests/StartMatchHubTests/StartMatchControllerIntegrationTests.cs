using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Application.Interfaces.Services.Hub.ClienteService;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Tests.Integration.ClassTests.HubTests.StartMatchHubTests
{
    public class StartMatchControllerIntegrationTests
    {
        private ApiTestAppFactory _factory = null!;
        private WebApplicationFactory<Program> _appFactory = null!;

        [SetUp]
        public void SetUp()
        {
            _factory = new ApiTestAppFactory();
            _appFactory = _factory;
        }

        [TearDown]
        public void TearDown()
        {
            _appFactory?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task StartMatch_ValidGuid_ShouldCallStartMatchClientService_AndReturnOk()
        {
            // Arrange
            var mockStartClient = new Mock<IStartMatchHubClientService>();
            mockStartClient.Setup(m => m.InitializeAsync()).Returns(Task.CompletedTask);
            mockStartClient.Setup(m => m.JoinStartMatchAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockStartClient.Object);
                });
            });

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var idTeam = Guid.NewGuid();
            var idMatch = Guid.NewGuid();
            var url = $"api/Calendar/{idTeam}/StartMatch";

            var content = new StringContent($"\"{idMatch}\"", Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync(url, content);

            // Assert (usando Assert.That)
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            mockStartClient.Verify(m => m.InitializeAsync(), Times.Once);
            mockStartClient.Verify(m => m.JoinStartMatchAsync(idMatch, idTeam), Times.Once);
        }

        [Test]
        public async Task StartMatch_EmptyGuid_ShouldReturnBadRequest()
        {
            var mockStartClient = new Mock<IStartMatchHubClientService>();
            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockStartClient.Object);
                });
            });

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var idTeam = Guid.NewGuid();
            var url = $"api/Calendar/{idTeam}/StartMatch";

            var emptyGuid = Guid.Empty;
            var content = new StringContent($"\"{emptyGuid}\"", Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            mockStartClient.Verify(m => m.JoinStartMatchAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task LeaveStartMatch_ShouldCallLeaveAndReturnOk()
        {
            var mockStartClient = new Mock<IStartMatchHubClientService>();
            mockStartClient.Setup(m => m.InitializeAsync()).Returns(Task.CompletedTask);
            mockStartClient.Setup(m => m.LeaveStartMatchAsync()).Returns(Task.CompletedTask).Verifiable();

            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockStartClient.Object);
                });
            });

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var idTeam = Guid.NewGuid();
            var url = $"api/Calendar/{idTeam}/LeaveStartMatch";

            var response = await client.PostAsync(url, null);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            mockStartClient.Verify(m => m.InitializeAsync(), Times.Once);
            mockStartClient.Verify(m => m.LeaveStartMatchAsync(), Times.Once);
        }
    }
}
