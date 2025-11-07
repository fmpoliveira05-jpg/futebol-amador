using Api.Hubs;
using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Hub;
using Application.Services.BackGroundServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Reflection;

namespace Tests.Unit.ApplicationTests.BackGroundServiceTests
{
    [TestFixture]
    public class RankMatchMakerBackGroundServiceTests
    {
        #region Helper
        private RankMatchMakerBackGroundService CreateService(
            out Mock<IHubContext<RankMatchMakerHub, IRankMatchMakerHub>> hubContextMock,
            out Mock<IHubClients<IRankMatchMakerHub>> clientsMock,
            out Mock<IRankMatchMakerHub> clientProxyMock,
            out Mock<IGroupManager> groupsMock,
            out Mock<ILogger<RankMatchMakerBackGroundService>> loggerMock)
        {
            hubContextMock = new Mock<IHubContext<RankMatchMakerHub, IRankMatchMakerHub>>();
            clientsMock = new Mock<IHubClients<IRankMatchMakerHub>>();
            clientProxyMock = new Mock<IRankMatchMakerHub>();
            groupsMock = new Mock<IGroupManager>();
            loggerMock = new Mock<ILogger<RankMatchMakerBackGroundService>>();

            hubContextMock.SetupGet(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            hubContextMock.SetupGet(h => h.Groups).Returns(groupsMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();

            var service = new RankMatchMakerBackGroundService(scopeFactoryMock.Object, hubContextMock.Object, loggerMock.Object);
            return service;
        }

        #endregion

        #region Tests
        [Test(Description = "HandleCloseGroups deve chamar Clients.Group(...).OnGroupClosed(...) e Groups.RemoveFromGroupAsync(...) para cada entry.")]
        public async Task HandleCloseGroups_InvokesHubContextMethods()
        {
            // Arrange
            var service = CreateService(
                out var hubContextMock,
                out var clientsMock,
                out var clientProxyMock,
                out var groupsMock,
                out var loggerMock
            );

            clientProxyMock.Setup(p => p.OnGroupClosed(It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            groupsMock.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var e1 = new EntryRankMatchMakerHub { ConnectionId = "conn1", Team = new InfoTeamRankMatchMakerDto { IdTeam = Guid.NewGuid() } };
            var e2 = new EntryRankMatchMakerHub { ConnectionId = "conn2", Team = new InfoTeamRankMatchMakerDto { IdTeam = Guid.NewGuid() } };

            var dict = new Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub>
            {
                [e1] = e2
            };

            // Act - invoke private method HandleCloseGroups via reflection
            var method = typeof(RankMatchMakerBackGroundService).GetMethod("HandleCloseGroups", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "HandleCloseGroups method not found via reflection.");

            var task = (Task)method.Invoke(service, new object[] { dict, CancellationToken.None })!;
            await task; 

            // Assert - verify calls
            clientProxyMock.Verify(p => p.OnGroupClosed(It.IsAny<string>()), Times.AtLeastOnce);
            groupsMock.Verify(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Test(Description = "ProcessEntry deve ignorar entradas inválidas e chamar hub para entradas válidas.")]
        public async Task ProcessEntry_SkipsWhenInvalid_And_InvokesWhenValid()
        {
            // Arrange
            var service = CreateService(
                out var hubContextMock,
                out var clientsMock,
                out var clientProxyMock,
                out var groupsMock,
                out var loggerMock
            );

            clientProxyMock.Setup(p => p.OnGroupClosed(It.IsAny<string>())).Returns(Task.CompletedTask).Verifiable();
            groupsMock.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask).Verifiable();

            var invalidEntry = new EntryRankMatchMakerHub { ConnectionId = "", Team = new InfoTeamRankMatchMakerDto { IdTeam = Guid.Empty } };

            var processMethod = typeof(RankMatchMakerBackGroundService).GetMethod("ProcessEntry", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(processMethod, Is.Not.Null, "ProcessEntry method not found via reflection.");

            var taskInvalid = (Task)processMethod.Invoke(service, new object[] { invalidEntry, CancellationToken.None })!;
            await taskInvalid;

            clientProxyMock.Verify(p => p.OnGroupClosed(It.IsAny<string>()), Times.Never);
            groupsMock.Verify(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

            var validTeamId = Guid.NewGuid();
            var validEntry = new EntryRankMatchMakerHub { ConnectionId = "conn-valid", Team = new InfoTeamRankMatchMakerDto { IdTeam = validTeamId } };

            clientProxyMock.Invocations.Clear();
            groupsMock.Invocations.Clear();

            var taskValid = (Task)processMethod.Invoke(service, new object[] { validEntry, CancellationToken.None })!;
            await taskValid;

            clientProxyMock.Verify(p => p.OnGroupClosed(It.IsAny<string>()), Times.AtLeastOnce);
            groupsMock.Verify(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
        #endregion
    }
}