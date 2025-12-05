using Application.Hubs;
using Application.Interfaces.Services.Hub;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Tests.Integration.ClassTests.HubTests.StartMatchHubTests
{
    public class StartMatchHubIntegrationTests
    {
        private ApiTestAppFactory _factory = null!;
        private WebApplicationFactory<Program> _appFactory = null!;
        private const string HubPath = "/StartMatch";

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

            // Mock do serviço
            var mockManager = new Mock<IManagerStartMatchService>();
            mockManager.SetupSequence(m => m.JoinHubAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(new JoinStartMatchResult { IsFirstAdmin = true, TeamId = firstTeamId, Match = null! })
                .ReturnsAsync(new JoinStartMatchResult { IsFirstAdmin = false, MatchStarted = true, TeamId = secondTeamId, FirstAdminConnectionId = "dummy-id", Match = null! });

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

                    if (!options.Headers.ContainsKey("Authorization"))
                    {
                        options.Headers.Add("Authorization", "Test");
                    }
                })
                .Build();

            var secondConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(baseAddress, HubPath), options =>
                {
                    options.HttpMessageHandlerFactory = _ => clientFactory.Server.CreateHandler();

                    // Igual para o segundo cliente
                    if (!options.Headers.ContainsKey("Authorization"))
                    {
                        options.Headers.Add("Authorization", "Test");
                    }
                })
                .Build();

            string? receivedMessage = null;
            var tcs = new TaskCompletionSource<string>();

            // Listeners
            Action<string> handler = (msg) => {
                receivedMessage = msg;
                tcs.TrySetResult(msg);
            };

            firstConnection.On("ReceiveStartMatch", handler);
            secondConnection.On("ReceiveStartMatch", handler);

            await firstConnection.StartAsync();
            await secondConnection.StartAsync();

            // Act
            await firstConnection.InvokeAsync("JoinStartMatch", matchId, firstTeamId);
            await secondConnection.InvokeAsync("JoinStartMatch", matchId, secondTeamId);

            // Wait
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));

            // Asserts
            Assert.That(completedTask, Is.EqualTo(tcs.Task), "Timeout: ReceiveStartMatch não recebido.");
            Assert.That(receivedMessage, Is.Not.Null.And.Not.Empty);

            // Cleanup
            await firstConnection.StopAsync();
            await secondConnection.StopAsync();
            await firstConnection.DisposeAsync();
            await secondConnection.DisposeAsync();
        }
    }
}