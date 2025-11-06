using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Services.Hub.ClienteService;
using Domain.Constants;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration.ClassTests
{
    [TestFixture]
    public class CompetitiveMatchesControllerTests
    {
        private ApiTestAppFactory _factory;
        private HttpClient _client;
        private Mock<IRankMatchMakerHubClientService> _mockHubClientService;

        [SetUp]
        public void Setup()
        {
            _factory = new ApiTestAppFactory();
            _mockHubClientService = new Mock<IRankMatchMakerHubClientService>();
            _mockHubClientService.Setup(s => s.InitializeAsync()).Returns(Task.CompletedTask);
            _mockHubClientService.Setup(s => s.JoinRankMatchMakerAsync(It.IsAny<StartSearchDto>())).Returns(Task.CompletedTask);
            _mockHubClientService.Setup(s => s.LeaveRankMatchMakerAsync()).Returns(Task.CompletedTask);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var realService = services.SingleOrDefault(d => d.ServiceType == typeof(IRankMatchMakerHubClientService));
                    if (realService != null)
                    {
                        services.Remove(realService);
                    }

                    services.AddScoped<IRankMatchMakerHubClientService>(_ => _mockHubClientService.Object);
                });
            }).CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task StartMatch_ComDtoValido_DeveRetornarOkEChamarOServico()
        {
            var idDaEquipa = Guid.NewGuid();
            var startSearchDto = new StartSearchDto
            {
                IdTeam = idDaEquipa,
                HoursGame = ModelConstants.HoursValidToCompetitiveMatch.MORNING 
            };

            // ACT (Agir)
            var response = await _client.PostAsJsonAsync("/api/CompetitiveMatches/Search", startSearchDto);

            response.EnsureSuccessStatusCode(); 
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.That(responseString, Is.EqualTo("Conseguiu entrar no hub!"));
            _mockHubClientService.Verify(s => s.InitializeAsync(), Times.Once());

            _mockHubClientService.Verify(s => s.JoinRankMatchMakerAsync(
                It.Is<StartSearchDto>(dto => dto.IdTeam == idDaEquipa)
            ), Times.Once());

            _mockHubClientService.Verify(s => s.LeaveRankMatchMakerAsync(), Times.Never());
        }

        [Test]
        public async Task LeaveStartMatch_ComIdValido_DeveRetornarOkEChamarOServico()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();

            // ACT
            var response = await _client.PostAsJsonAsync("/api/CompetitiveMatches/CancelSearch", teamId);

            // ASSERT
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.That(responseString, Is.EqualTo("Saiu do Hub com sucesso!"));

            _mockHubClientService.Verify(s => s.InitializeAsync(), Times.Once());
            _mockHubClientService.Verify(s => s.LeaveRankMatchMakerAsync(), Times.Once());
            _mockHubClientService.Verify(s => s.JoinRankMatchMakerAsync(It.IsAny<StartSearchDto>()), Times.Never());
        }
    }
}
