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
    /**
     Depois adaptar o teste qauando tiver o update correto
     */
    [TestFixture]
    public class UpdatePlayerTest
    {
        private Mock<IPlayerRepository> playerRepoMock;
        private Mock<ITeamRepository> teamRepoMock;
        private Mock<IUnityOfWork> unitOfWorkMock;
        private PlayerService service;

        [SetUp]
        public void SetUp()
        {
            playerRepoMock = new Mock<IPlayerRepository>();
            teamRepoMock = new Mock<ITeamRepository>();
            unitOfWorkMock = new Mock<IUnityOfWork>();

            service = new PlayerService(playerRepoMock.Object, teamRepoMock.Object, unitOfWorkMock.Object);
        }

        private UpdatePlayerDTO ValidDto()
        {
            return new UpdatePlayerDTO
            {
                Name = "Joao Silva",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Address = "Rua Principal, 1",
                Email = "joaosilva@example.com",
                Phone = "912345678",
                Position = Position.MIDFIELDER,
                Height = 180
            };
        }

        private Player ExistingPlayer(Guid id, string email = "old@example.com", string phone = "912345678")
        {
            return new Player
            {
                Id = id,
                Name = "Old Name",
                Email = email,
                Phone = phone,
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Address = "Old Addr",
                Position = Position.DEFENDER,
                Height = 175
            };
        }

        [Test(Description = "Atualizar um player de maneira válida")]
        public async Task UpdatePlayerAsync_ValidInput_CallsUpdateAndSaves()
        {
            var id = Guid.NewGuid();
            var dto = ValidDto();

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(id)).ReturnsAsync(ExistingPlayer(id));
            playerRepoMock.Setup(r => r.GetPlayerByEmailAsync(dto.Email)).ReturnsAsync((Player?)null);

            //Ainda não existe
            /*
            playerRepoMock
                .Setup(r => r.GetPlayerByPhoneAsync(dto.Phone))
                .ReturnsAsync((Player?)null);
            */

            playerRepoMock.Setup(r => r.UpdatePlayer(It.IsAny<Player>()));
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            await service.UpdatePlayerAsync(id, dto);

            playerRepoMock.Verify(r => r.UpdatePlayer(It.Is<Player>(p =>
                p.Id == id &&
                p.Name == dto.Name &&
                p.Email == dto.Email &&
                p.Phone == dto.Phone &&
                p.Height == dto.Height &&
                p.Position == dto.Position &&
                p.Address == dto.Address &&
                p.DateOfBirth.Equals(dto.DateOfBirth)
            )), Times.Once);

            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
