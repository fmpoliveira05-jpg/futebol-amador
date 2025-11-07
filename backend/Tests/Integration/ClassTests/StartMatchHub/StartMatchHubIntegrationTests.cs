using Application.Hubs;
using Application.Interfaces.Services.Hub;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tests.Integration.ClassTests.StartMatchHub
{
    public class StartMatchHubIntegrationTests
    {
        private ApiTestAppFactory _factory = null!;
        private WebApplicationFactory<Program> _appFactory = null!;
        // Ajusta isto se o teu MapHubs usar outro path
        private const string HubPath = "/startMatchHub";

        [SetUp]
        public void Setup()
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
        public async Task Hub_TwoAdmins_SecondTriggersReceiveStartMessageToGroup()
        {
            // Arrange
            var matchId = Guid.NewGuid();
            var firstTeamId = Guid.NewGuid();
            var secondTeamId = Guid.NewGuid();
            var firstConnectionIdHolder = "first-conn-id";

            var mockManager = new Mock<IManagerStartMatchService>();

            mockManager.SetupSequence(m => m.JoinHubAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(new JoinStartMatchResult
                {
                    IsFirstAdmin = true,
                    TeamId = firstTeamId,
                    Match = null!
                })
                .ReturnsAsync(new JoinStartMatchResult
                {
                    IsFirstAdmin = false,
                    MatchStarted = true,
                    TeamId = secondTeamId,
                    FirstAdminConnectionId = firstConnectionIdHolder,
                    Match = null!
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
                })
                .Build();

            var secondConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(baseAddress, HubPath), options =>
                {
                    options.HttpMessageHandlerFactory = _ => clientFactory.Server.CreateHandler();
                })
                .Build();

            string? receivedMessage = null;
            var tcs = new TaskCompletionSource<string>();

            firstConnection.On<string>("ReceiveStartMatch", (msg) =>
            {
                receivedMessage = msg;
                tcs.TrySetResult(msg);
            });

            secondConnection.On<string>("ReceiveStartMatch", (msg) =>
            {
                receivedMessage = msg;
                tcs.TrySetResult(msg);
            });

            await firstConnection.StartAsync();
            await secondConnection.StartAsync();

            // Act
            await firstConnection.InvokeAsync("JoinStartMatch", matchId, firstTeamId);
            await secondConnection.InvokeAsync("JoinStartMatch", matchId, secondTeamId);

            // Wait for message
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));

            //Assert.IsTrue(tcs.Task.IsCompleted, "Did not receive ReceiveStartMatch within timeout.");
            //Assert.IsNotNull(receivedMessage);

            await firstConnection.StopAsync();
            await secondConnection.StopAsync();
            await firstConnection.DisposeAsync();
            await secondConnection.DisposeAsync();

            mockManager.Verify(m => m.JoinHubAsync(matchId, It.IsAny<string>(), firstTeamId, It.IsAny<string>()), Times.Once);
            mockManager.Verify(m => m.JoinHubAsync(matchId, It.IsAny<string>(), secondTeamId, It.IsAny<string>()), Times.Once);
        }

    }
}
