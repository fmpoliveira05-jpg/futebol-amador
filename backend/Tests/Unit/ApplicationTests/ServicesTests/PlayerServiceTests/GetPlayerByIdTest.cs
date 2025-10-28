using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
using NUnit.Framework;

namespace Tests.Unit.ApplicationTests.ServicesTests.PlayerServiceTests
{
    [TestFixture]
    public class GetPlayerByIdTest
    {
        private Mock<IPlayerRepository> playerRepoMock;
        private Mock<ITeamRepository> teamRepoMock;
        private Mock<IUnityOfWork> unitOfWorkMock;
        private IPlayerService service;

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

        private Player ValidPlayer()
        {
            var playerId = Guid.NewGuid();
            return new Player
            {
                Id = playerId,
                Name = "Joao Silva",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
                Address = "Rua A, Guimarães",
                Email = "joao.silva@example.com",
                Password = "StrongP@ss1",
                Phone = "912345678",
                Position = Position.MIDFIELDER,
                Height = 180
            };
        }

        [Test(Description = "Consultar um player existente retorna PlayerDetailsDTO corretamente")]
        public async Task GetPlayerByIdAsync_ValidPlayer_ReturnsPlayerDetails()
        {
            var player = ValidPlayer();

            playerRepoMock
                .Setup(p => p.GetPlayerByIdAsync(player.Id))
                .ReturnsAsync(player);

            var result = await service.GetPlayerByIdAsync(player.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo(player.Name));
            Assert.That(result.DateOfBirth, Is.EqualTo(player.DateOfBirth));
            Assert.That(result.Address, Is.EqualTo(player.Address));
            Assert.That(result.Position, Is.EqualTo(player.Position));
            Assert.That(result.Height, Is.EqualTo(player.Height));

            playerRepoMock.Verify(r => r.GetPlayerByIdAsync(player.Id), Times.Once);
        }

        [Test(Description = "Consultar um player que não existe lança exceção")]
        public void GetPlayerByIdAsync_PlayerNotFound_ThrowsException()
        {
            var playerId = Guid.NewGuid();

            playerRepoMock
                .Setup(p => p.GetPlayerByIdAsync(playerId))
                .ReturnsAsync((Player?)null);

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.GetPlayerByIdAsync(playerId));

            playerRepoMock.Verify(r => r.GetPlayerByIdAsync(playerId), Times.Once);
        }
    }
}
