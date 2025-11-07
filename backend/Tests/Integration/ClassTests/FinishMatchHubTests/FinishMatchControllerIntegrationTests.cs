using Application.DTOs.Match;
using Application.Interfaces.Services.Hub.ClienteService;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration.ClassTests.FinishMatchHubTests
{
    public class FinishMatchControllerIntegrationTests
    {
        #region Variables 
        private ApiTestAppFactory _factory = null!;
        private WebApplicationFactory<Program> _appFactory = null!;

        #endregion

        #region SetUp
        [SetUp]
        public void SetUp()
        {
            _factory = new ApiTestAppFactory();
            _appFactory = _factory;
        }
        #endregion

        #region TearDown
        [TearDown]
        public void TearDown()
        {
            _appFactory?.Dispose();
            _factory?.Dispose();
        }

        #endregion

        #region FinishMatch - POST

        [Test]
        public async Task FinishMatch_ValidDto_ShouldCallServiceAndReturnOk()
        {
            // Arrange
            var mockFinishClient = new Mock<IFinishMatchHubClientService>();
            mockFinishClient.Setup(m => m.InitializeAsync()).Returns(Task.CompletedTask);
            mockFinishClient.Setup(m => m.JoinFinishMatchAsync(It.IsAny<ResultMatchDto>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockFinishClient.Object);
                });
            });

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var idTeam = Guid.NewGuid();
            var dto = new ResultMatchDto
            {
                IdTeam = idTeam,
                IdMatch = Guid.NewGuid(),
                IdOpponent = Guid.NewGuid(),
                NumGoalsTeam = 2,
                NumGoalsOpponent = 1
            };
            
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var url = $"api/Calendar/{idTeam}/FinishMatch";

            // Act
            var response = await client.PostAsync(url, content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            mockFinishClient.Verify(m => m.InitializeAsync(), Times.Once);
            mockFinishClient.Verify(m => m.JoinFinishMatchAsync(It.Is<ResultMatchDto>(r => r.IdTeam == idTeam && r.IdMatch == dto.IdMatch)), Times.Once);
        }

        [Test]
        public async Task FinishMatch_IdTeamEmpty_ShouldReturnBadRequest()
        {
            var mockFinishClient = new Mock<IFinishMatchHubClientService>();
            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockFinishClient.Object);
                });
            });

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var idTeam = Guid.Empty;
            var dto = new ResultMatchDto
            {
                IdTeam = Guid.NewGuid(),
                IdMatch = Guid.NewGuid()
            };
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var url = $"api/Calendar/{idTeam}/FinishMatch";

            var response = await client.PostAsync(url, content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            mockFinishClient.Verify(m => m.JoinFinishMatchAsync(It.IsAny<ResultMatchDto>()), Times.Never);
        }

        [Test]
        public async Task FinishMatch_NullBody_ShouldReturnBadRequest()
        {
            var mockFinishClient = new Mock<IFinishMatchHubClientService>();
            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockFinishClient.Object);
                });
            });

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var idTeam = Guid.NewGuid();
            var content = new StringContent("null", Encoding.UTF8, "application/json");
            var url = $"api/Calendar/{idTeam}/FinishMatch";

            var response = await client.PostAsync(url, content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            mockFinishClient.Verify(m => m.JoinFinishMatchAsync(It.IsAny<ResultMatchDto>()), Times.Never);
        }

        [Test]
        public async Task FinishMatch_DtoTeamMismatch_ShouldReturnBadRequest()
        {
            var mockFinishClient = new Mock<IFinishMatchHubClientService>();
            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockFinishClient.Object);
                });
            });

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var idTeam = Guid.NewGuid();
            var dto = new ResultMatchDto
            {
                IdTeam = Guid.NewGuid(),
                IdMatch = Guid.NewGuid()
            };
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var url = $"api/Calendar/{idTeam}/FinishMatch";

            var response = await client.PostAsync(url, content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            mockFinishClient.Verify(m => m.JoinFinishMatchAsync(It.IsAny<ResultMatchDto>()), Times.Never);
        }

        #endregion

        #region UpdateFinishMatch - PUT

        [Test]
        public async Task UpdateFinishMatch_ValidDto_ShouldCallServiceAndReturnOk()
        {
            var mockFinishClient = new Mock<IFinishMatchHubClientService>();
            mockFinishClient.Setup(m => m.InitializeAsync()).Returns(Task.CompletedTask);
            mockFinishClient.Setup(m => m.EditResultMatchAsync(It.IsAny<ResultMatchDto>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockFinishClient.Object);
                });
            });

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var idTeam = Guid.NewGuid();
            var dto = new ResultMatchDto
            {
                IdTeam = idTeam,
                IdMatch = Guid.NewGuid(),
                IdOpponent = Guid.NewGuid(),
                NumGoalsTeam = 3,
                NumGoalsOpponent = 0
            };

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var url = $"api/Calendar/{idTeam}/UpdateFinishMatch";

            var response = await client.PutAsync(url, content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            mockFinishClient.Verify(m => m.InitializeAsync(), Times.Once);
            mockFinishClient.Verify(m => m.EditResultMatchAsync(It.Is<ResultMatchDto>(r => r.IdTeam == idTeam && r.IdMatch == dto.IdMatch)), Times.Once);
        }

        [Test]
        public async Task UpdateFinishMatch_IdTeamEmpty_ShouldReturnBadRequest()
        {
            var mockFinishClient = new Mock<IFinishMatchHubClientService>();
            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockFinishClient.Object);
                });
            });

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var idTeam = Guid.Empty;
            var dto = new ResultMatchDto { IdTeam = Guid.Empty, IdMatch = Guid.NewGuid() };
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var url = $"api/Calendar/{idTeam}/UpdateFinishMatch";

            var response = await client.PutAsync(url, content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            mockFinishClient.Verify(m => m.EditResultMatchAsync(It.IsAny<ResultMatchDto>()), Times.Never);
        }

        [Test]
        public async Task UpdateFinishMatch_NullBody_ShouldReturnBadRequest()
        {
            var mockFinishClient = new Mock<IFinishMatchHubClientService>();
            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockFinishClient.Object);
                });
            });

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var idTeam = Guid.NewGuid();
            var content = new StringContent("null", Encoding.UTF8, "application/json");
            var url = $"api/Calendar/{idTeam}/UpdateFinishMatch";

            var response = await client.PutAsync(url, content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            mockFinishClient.Verify(m => m.EditResultMatchAsync(It.IsAny<ResultMatchDto>()), Times.Never);
        }

        [Test]
        public async Task UpdateFinishMatch_DtoTeamMismatch_ShouldReturnBadRequest()
        {
            var mockFinishClient = new Mock<IFinishMatchHubClientService>();
            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockFinishClient.Object);
                });
            });

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var idTeam = Guid.NewGuid();
            var dto = new ResultMatchDto
            {
                IdTeam = Guid.NewGuid(), 
                IdMatch = Guid.NewGuid()
            };

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var url = $"api/Calendar/{idTeam}/UpdateFinishMatch";

            var response = await client.PutAsync(url, content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            mockFinishClient.Verify(m => m.EditResultMatchAsync(It.IsAny<ResultMatchDto>()), Times.Never);
        }

        #endregion

        #region LeaveFinishMatch - POST

        [Test]
        public async Task LeaveFinishMatch_ShouldCallLeaveAndReturnOk()
        {
            var mockFinishClient = new Mock<IFinishMatchHubClientService>();
            mockFinishClient.Setup(m => m.InitializeAsync()).Returns(Task.CompletedTask);
            mockFinishClient.Setup(m => m.LeaveFinishMatchAsync()).Returns(Task.CompletedTask).Verifiable();

            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockFinishClient.Object);
                });
            });

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var idTeam = Guid.NewGuid();
            var url = $"api/Calendar/{idTeam}/LeaveFinishMatch";

            var response = await client.PostAsync(url, null);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            mockFinishClient.Verify(m => m.InitializeAsync(), Times.Once);
            mockFinishClient.Verify(m => m.LeaveFinishMatchAsync(), Times.Once);
        }

        #endregion
    }
}
