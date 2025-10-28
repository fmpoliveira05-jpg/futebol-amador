using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Entities;
using Moq;
using NUnit.Framework;

namespace Tests.Unit.ApplicationTests.ServicesTests.PlayerServiceTests
{
    [TestFixture]
    public class DeletePlayerTest
    {
        private Mock<IPlayerRepository> playerRepoMock;
        private Mock<ITeamRepository> teamRepoMock;
        private Mock<IUnityOfWork> unitOfWorkMock;
        private PlayerService service;

        [SetUp]
        public void Setup()
        {
            playerRepoMock = new Mock<IPlayerRepository>();
            teamRepoMock = new Mock<ITeamRepository>();
            unitOfWorkMock = new Mock<IUnityOfWork>();

            service = new PlayerService(
                playerRepoMock.Object,
                teamRepoMock.Object,
                unitOfWorkMock.Object
            );
        }

        [Test(Description = "Eliminar um utilizador existente com sucesso")]
        public async Task DeletePlayerAsync_ValidPlayer_DeletesAndSaves()
        {
            var playerId = Guid.NewGuid();
            var player = new Player { 
                Id = playerId 
            };

            playerRepoMock
                .Setup(r => r.GetPlayerByIdAsync(playerId))
                .ReturnsAsync(player);

            unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            await service.DeletePlayerAsync(playerId);

            playerRepoMock.Verify(r => r.GetPlayerByIdAsync(playerId), Times.Once);
            playerRepoMock.Verify(r => r.DeletePlayer(player), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "Tentar eliminar um utilizador que não existe")]
        public void DeletePlayerAsync_PlayerNotFound_ThrowsException()
        {
            var playerId = Guid.NewGuid();

            playerRepoMock
                .Setup(r => r.GetPlayerByIdAsync(playerId))
                .ReturnsAsync((Player?)null);

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.DeletePlayerAsync(playerId));

            playerRepoMock.Verify(r => r.DeletePlayer(It.IsAny<Player>()), Times.Never);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
    }
}
