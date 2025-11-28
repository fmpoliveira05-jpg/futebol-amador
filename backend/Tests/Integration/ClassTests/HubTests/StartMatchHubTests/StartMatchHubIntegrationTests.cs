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
                    options.AccessTokenProvider = () => Task.FromResult<string?>("TestToken"); // Token fictício para passar a Auth
                })
                .Build();

            var secondConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(baseAddress, HubPath), options =>
                {
                    options.HttpMessageHandlerFactory = _ => clientFactory.Server.CreateHandler();
                    options.AccessTokenProvider = () => Task.FromResult<string?>("TestToken"); // Token fictício para passar a Auth
                })
                .Build();

            string? receivedMessage = null;
            var tcs = new TaskCompletionSource<string>();

            // Configurar listeners antes de iniciar a conexão
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
            // Simular o primeiro admin a entrar
            await firstConnection.InvokeAsync("JoinStartMatch", matchId, firstTeamId);

            // Simular o segundo admin a entrar (que deve disparar o início do jogo)
            await secondConnection.InvokeAsync("JoinStartMatch", matchId, secondTeamId);

            // Wait for message (timeout safety)
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Esperar que a TaskCompletionSource seja completada ou que o timeout ocorra
            // Usamos Task.WhenAny para não bloquear indefinidamente se a mensagem nunca chegar
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));

            // Asserts
            Assert.That(completedTask, Is.EqualTo(tcs.Task), "Não foi recebido ReceiveStartMatch dentro do timeout.");
            Assert.That(receivedMessage, Is.Not.Null.And.Not.Empty);

            // Cleanup
            await firstConnection.StopAsync();
            await secondConnection.StopAsync();
            await firstConnection.DisposeAsync();
            await secondConnection.DisposeAsync();

            // Verificar se o serviço foi chamado duas vezes
            mockManager.Verify(m => m.JoinHubAsync(matchId, It.IsAny<string>(), firstTeamId, It.IsAny<string>()), Times.Once);
            mockManager.Verify(m => m.JoinHubAsync(matchId, It.IsAny<string>(), secondTeamId, It.IsAny<string>()), Times.Once);
        }
    }
}