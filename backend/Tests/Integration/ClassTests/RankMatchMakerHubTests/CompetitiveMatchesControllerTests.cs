using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Services.Hub.ClienteService;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Tests.Integration.ClassTests.RankMatchMakerHubTests
{
    [TestFixture]
    public class CompetitiveMatchesControllerTests
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

        #region Tests
        [Test(Description = "POST /api/CompetitiveMatches/Search deve chamar InitializeAsync e JoinRankMatchMakerAsync no serviço cliente.")]
        public async Task Search_Post_CallsClientServiceAndReturnsOk()
        {
            // Arrange
            var mockClient = new Mock<IRankMatchMakerHubClientService>();
            mockClient.Setup(m => m.InitializeAsync()).Returns(Task.CompletedTask);
            mockClient.Setup(m => m.JoinRankMatchMakerAsync(It.IsAny<StartSearchDto>())).Returns(Task.CompletedTask).Verifiable();

            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockClient.Object);
                });
            });

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var dto = new StartSearchDto
            {
                IdTeam = Guid.NewGuid(),
                HoursGame = TimeOnly.FromDateTime(DateTime.UtcNow)
            };

            var response = await client.PostAsync("api/CompetitiveMatches/Search",
                new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            mockClient.Verify(m => m.InitializeAsync(), Times.Once);
            mockClient.Verify(m => m.JoinRankMatchMakerAsync(It.Is<StartSearchDto>(d => d.IdTeam == dto.IdTeam)), Times.Once);
        }

        [Test(Description = "POST /api/CompetitiveMatches/CancelSearch deve chamar InitializeAsync e LeaveRankMatchMakerAsync no serviço cliente.")]
        public async Task CancelSearch_Post_CallsClientServiceAndReturnsOk()
        {
            // Arrange
            var mockClient = new Mock<IRankMatchMakerHubClientService>();
            mockClient.Setup(m => m.InitializeAsync()).Returns(Task.CompletedTask);
            mockClient.Setup(m => m.LeaveRankMatchMakerAsync()).Returns(Task.CompletedTask).Verifiable();

            var clientFactory = _app_factory_with_mock(_appFactory, mockClient.Object);

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var idTeam = Guid.NewGuid();
            var response = await client.PostAsync("api/CompetitiveMatches/CancelSearch",
                new StringContent(JsonSerializer.Serialize(idTeam), Encoding.UTF8, "application/json"));

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            mockClient.Verify(m => m.InitializeAsync(), Times.Once);
            mockClient.Verify(m => m.LeaveRankMatchMakerAsync(), Times.Once);
        }
        #endregion

        #region Private Methods
        private WebApplicationFactory<Program> _app_factory_with_mock(WebApplicationFactory<Program> factory, IRankMatchMakerHubClientService mock)
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mock);
                });
            });
        }

        #endregion
    }
}
