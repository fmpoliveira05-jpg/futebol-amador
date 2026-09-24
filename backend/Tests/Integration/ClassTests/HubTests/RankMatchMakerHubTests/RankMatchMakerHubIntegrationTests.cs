using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Services.Hub;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration.ClassTests.HubTests.RankMatchMakerHubTests
{
    public class RankMatchMakerHubIntegrationTests
    {
        #region Variables
        private ApiTestAppFactory _factory = null!;
        private WebApplicationFactory<Program> _appFactory = null!;
        private const string HubPath = "/MatchMaker";
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
        [Test(Description = "Quando dois teams entram no hub, o manager.JoinRankMatchMaker é invocado para cada join.")]
        public async Task RankMatchMakerHub_TwoTeams_CallsManagerJoin()
        {
            // Arrange
            var team1 = Guid.NewGuid();
            var team2 = Guid.NewGuid();

            var mockManager = new Mock<IManagerRankMatchMakerService>();

            int call = 0;
            mockManager.Setup(m => m.JoinRankMatchMaker(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<TimeOnly>(),
                    It.IsAny<string>()))
                .Returns((string idPlayer, Guid idTeam, TimeOnly hours, string connId) =>
                {
                    call++;
                    if (call == 1)
                    {
                        var entry = new EntryRankMatchMakerHub
                        {
                            ConnectionId = connId,
                            Team = new InfoTeamRankMatchMakerDto { IdTeam = idTeam }
                        };
                        return Task.FromResult(entry);
                    }
                    else
                    {
                        var entry = new EntryRankMatchMakerHub
                        {
                            ConnectionId = "other-conn",
                            Team = new InfoTeamRankMatchMakerDto { IdTeam = team1 }
                        };
                        return Task.FromResult(entry);
                    }
                });

            var clientFactory = _app_factory_with_mock(_appFactory, mockManager.Object);
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

            Assert.That(firstConnection.State, Is.EqualTo(HubConnectionState.Connected));
            Assert.That(secondConnection.State, Is.EqualTo(HubConnectionState.Connected));

            // Act: first join
            var dto1 = new StartSearchDto { IdTeam = team1, HoursGame = TimeOnly.FromDateTime(DateTime.UtcNow) };
            var dto2 = new StartSearchDto { IdTeam = team2, HoursGame = TimeOnly.FromDateTime(DateTime.UtcNow) };

            await firstConnection.InvokeAsync("JoinRankMatchMaker", dto1);
            await secondConnection.InvokeAsync("JoinRankMatchMaker", dto2);
            await Task.Delay(200);
            await firstConnection.StopAsync();
            await secondConnection.StopAsync();
            await firstConnection.DisposeAsync();
            await secondConnection.DisposeAsync();

            mockManager.Verify(m => m.JoinRankMatchMaker(It.IsAny<string>(), team1, It.IsAny<TimeOnly>(), It.IsAny<string>()), Times.Once);
            mockManager.Verify(m => m.JoinRankMatchMaker(It.IsAny<string>(), team2, It.IsAny<TimeOnly>(), It.IsAny<string>()), Times.Once);
        }
        #endregion

        #region Helpers
        private WebApplicationFactory<Program> _app_factory_with_mock(WebApplicationFactory<Program> factory, IManagerRankMatchMakerService mock)
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
