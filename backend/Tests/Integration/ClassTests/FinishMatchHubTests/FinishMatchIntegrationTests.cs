using Application.DTOs.Match;
using Application.Hubs;
using Application.Interfaces.Services.Hub;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration.ClassTests.FinishMatchHubTests
{
    public class FinishMatchIntegrationTests
    {
        #region Variables
        private ApiTestAppFactory _factory = null!;
        private WebApplicationFactory<Program> _appFactory = null!;
        private const string HubPath = "/FinishMatch";
        #endregion

        #region SetUp 
        [SetUp]
        public void Setup()
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

        [Test(Description = "Quando dois admins entram no hub, o segundo finaliza a partida (simulação via mock do serviço).")]
        public async Task FinishMatchHub_TwoAdmins_SecondFinalizesMatch()
        {
            // Arrange
            var matchId = Guid.NewGuid();
            var firstTeamId = Guid.NewGuid();
            var secondTeamId = Guid.NewGuid();
            var firstAdminConnId = "first-admin-conn-id";

            var mockManager = new Mock<IManagerFinishMatchService>();

            mockManager.SetupSequence(m => m.JoinHubAsync(It.IsAny<Guid>(), It.IsAny<ResultMatchDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new JoinFinishMatch
                {
                    IdTeam = firstTeamId,
                    IsFirstAdmin = true,
                    MatchFinish = false,
                    ResultMatch = null
                })
                .ReturnsAsync(new JoinFinishMatch
                {
                    IdTeam = secondTeamId,
                    IsFirstAdmin = false,
                    MatchFinish = true,
                    FirstAdminConnectionId = firstAdminConnId,
                    ResultMatch = null
                });

            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockManager.Object);
                });
            });

            var baseAddress = clientFactory.Server.BaseAddress!;

            var firstConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(baseAddress, HubPath), options =>
                {
                    options.HttpMessageHandlerFactory = _ => clientFactory.Server.CreateHandler();
                    options.Headers.Add("Authorization", "Test");
                })
                .Build();

            var secondConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(baseAddress, HubPath), options =>
                {
                    options.HttpMessageHandlerFactory = _ => clientFactory.Server.CreateHandler();
                    options.Headers.Add("Authorization", "Test");
                })
                .Build();

            await firstConnection.StartAsync();
            await secondConnection.StartAsync();

            Assert.That(firstConnection.State, Is.EqualTo(HubConnectionState.Connected), "First connection is not connected.");
            Assert.That(secondConnection.State, Is.EqualTo(HubConnectionState.Connected), "Second connection is not connected.");

            // Act: first admin joins
            var firstDto = new ResultMatchDto
            {
                IdMatch = matchId,
                IdTeam = firstTeamId,
                IdOpponent = Guid.NewGuid(),
                NumGoalsTeam = 1,
                NumGoalsOpponent = 0
            };

            var secondDto = new ResultMatchDto
            {
                IdMatch = matchId,
                IdTeam = secondTeamId,
                IdOpponent = Guid.NewGuid(),
                NumGoalsTeam = 1,
                NumGoalsOpponent = 0
            };

            await firstConnection.InvokeAsync("JoinFinishMatch", firstDto);
            await secondConnection.InvokeAsync("JoinFinishMatch", secondDto);
            await Task.Delay(200);
            await firstConnection.StopAsync();
            await secondConnection.StopAsync();
            await firstConnection.DisposeAsync();
            await secondConnection.DisposeAsync();

            // Assert: manager called once per join
            mockManager.Verify(m => m.JoinHubAsync(matchId, It.IsAny<ResultMatchDto>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Test(Description = "Ao editar o resultado via hub, o serviço UpdateResult deve ser invocado uma vez.")]
        public async Task FinishMatchHub_EditResult_CallsUpdateResult()
        {
            // Arrange
            var matchId = Guid.NewGuid();
            var teamId = Guid.NewGuid();

            var mockManager = new Mock<IManagerFinishMatchService>();

            mockManager.Setup(m => m.UpdateResult(It.IsAny<Guid>(), It.IsAny<ResultMatchDto>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new JoinFinishMatch
                {
                    IdTeam = teamId,
                    IsFirstAdmin = false,
                    MatchFinish = false,
                    ResultMatch = null
                })
                .Verifiable();

            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockManager.Object);
                });
            });

            var baseAddress = clientFactory.Server.BaseAddress!;

            var connection = new HubConnectionBuilder()
                .WithUrl(new Uri(baseAddress, HubPath), options =>
                {
                    options.HttpMessageHandlerFactory = _ => clientFactory.Server.CreateHandler();
                    options.Headers.Add("Authorization", "Test");
                })
                .Build();

            await connection.StartAsync();

            Assert.That(connection.State, Is.EqualTo(HubConnectionState.Connected), "Connection is not connected.");

            var dto = new ResultMatchDto
            {
                IdMatch = matchId,
                IdTeam = teamId,
                IdOpponent = Guid.NewGuid(),
                NumGoalsTeam = 2,
                NumGoalsOpponent = 2
            };

            // Act
            await connection.InvokeAsync("EditResult", dto);
            await Task.Delay(200);

            await connection.StopAsync();
            await connection.DisposeAsync();

            // Assert
            mockManager.Verify(m => m.UpdateResult(matchId, It.IsAny<ResultMatchDto>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
        #endregion
    }
}
