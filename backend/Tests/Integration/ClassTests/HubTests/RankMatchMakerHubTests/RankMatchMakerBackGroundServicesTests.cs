using Api.Hubs;
using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Hub;
using Application.Interfaces.Services.Hub;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Tests.Integration.Helpers;

namespace Tests.Integration.ClassTests.HubTests.RankMatchMakerHubTests
{
    public class RankMatchMakerBackGroundServicesTests
    {
        [Test(Description = "One-shot integration: quando manager.MatchMaker devolve matches, o hubContext é chamado para fechar grupos.")]
        public async Task OneShot_Background_RunsAndCallsHubContext()
        {
            // Arrange: mock manager que devolve um match pair
            var mockManager = new Mock<IManagerRankMatchMakerService>();

            var e1 = new EntryRankMatchMakerHub { ConnectionId = "conn1", Team = new InfoTeamRankMatchMakerDto { IdTeam = Guid.NewGuid() } };
            var e2 = new EntryRankMatchMakerHub { ConnectionId = "conn2", Team = new InfoTeamRankMatchMakerDto { IdTeam = Guid.NewGuid() } };

            var dict = new Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub> { [e1] = e2 };

            mockManager.Setup(m => m.MatchMaker(It.IsAny<CriteriaMatchMaker>()))
                .ReturnsAsync(dict);

            var mockHubContext = new Mock<IHubContext<RankMatchMakerHub, IRankMatchMakerHub>>();
            var mockClients = new Mock<IHubClients<IRankMatchMakerHub>>();
            var mockClientProxy = new Mock<IRankMatchMakerHub>();
            var mockGroups = new Mock<IGroupManager>();

            mockHubContext.Setup(c => c.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
            mockHubContext.Setup(c => c.Groups).Returns(mockGroups.Object);
            mockClientProxy.Setup(p => p.OnGroupClosed(It.IsAny<string>())).Returns(Task.CompletedTask).Verifiable();
            mockGroups.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            var factory = new ApiTestAppFactory();
            var clientFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockManager.Object);
                    services.AddSingleton(typeof(IHubContext<RankMatchMakerHub, IRankMatchMakerHub>), mockHubContext.Object);
                    services.AddSingleton<IHostedService, OneShotRankMatchMakerHostedService>();
                });
            });

            // Act: criar client (dispara a inicialização do host e consequentemente do hosted service)
            using var client = clientFactory.CreateClient();

            await Task.Delay(300);

            // Assert: verifique que os métodos do hubContext foram invocados
            mockClientProxy.Verify(p => p.OnGroupClosed(It.IsAny<string>()), Times.AtLeastOnce);
            mockGroups.Verify(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}
