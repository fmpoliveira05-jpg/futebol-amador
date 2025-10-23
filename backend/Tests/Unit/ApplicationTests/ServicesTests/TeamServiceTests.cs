using Application.DTOs;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Validators;
using Application.Services;
using Application.Validators;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Unit.ApplicationTests.ServicesTests
{
    public class TeamServiceTests
    {
        private readonly Mock<ITeamRepository> _teamRepoMock;
        private readonly Mock<IPlayerRepository> _playerRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IUnityOfWork> _unitOfWorkMock;
        private readonly ITeamValidator _validatorReal;
        private readonly TeamService _sut;

        public TeamServiceTests()
        {
            _teamRepoMock = new Mock<ITeamRepository>();
            _playerRepoMock = new Mock<IPlayerRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnityOfWork>();

            // ⚙️ Usa o validator real
            _validatorReal = new TeamValidator();

            _sut = new TeamService(
                _teamRepoMock.Object,
                _playerRepoMock.Object,
                _userRepoMock.Object,
                _unitOfWorkMock.Object,
                _validatorReal
            );
        }

        // --------------------------------------------------------
        // TESTE: Criar equipa com sucesso
        // --------------------------------------------------------
        [Fact(DisplayName = "CreateTeamAsync deve criar uma nova equipa quando os dados são válidos")]
        public async Task CreateTeamAsync_Should_Create_Team_When_Valid()
        {
            // Arrange
            var dto = new CreateTeamDto
            {
                Name = "FC Teste",
                Description = "Equipa de teste",
                icon = new byte[] { 1, 2, 3, 4 },
                HomePitch = new PitchDto { Name = "Campo Central", Address = "Rua Principal" }
            };

            var player = new Player { Id = Guid.NewGuid(), Name = "Jogador 1", IdTeam = null };

            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name))
                .ReturnsAsync((Teams)null);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.AddAsync(It.IsAny<Teams>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // Act
            var result = await _sut.CreateTeamAsync(dto, player.Id);

            // Assert
            result.Should().NotBeEmpty();
            _teamRepoMock.Verify(r => r.AddAsync(It.IsAny<Teams>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // --------------------------------------------------------
        // TESTE: Criar equipa com nome duplicado
        // --------------------------------------------------------
        [Fact(DisplayName = "CreateTeamAsync deve lançar exceção quando o nome da equipa já existe")]
        public async Task CreateTeamAsync_Should_Throw_When_TeamNameExists()
        {
            // Arrange
            var dto = new CreateTeamDto
            {
                Name = "FC Duplicado",
                Description = "Teste",
                icon = new byte[] { 1, 2, 3, 4 },
                HomePitch = new PitchDto { Name = "Campo", Address = "Rua" }
            };

            var existingTeam = new Teams(dto.Name, "desc", new byte[] { 1, 2, 3, 4 }, new Pitch("Campo", "Rua"));
            var player = new Player { Id = Guid.NewGuid(), IdTeam = null };

            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync(existingTeam);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(It.IsAny<Guid>())).ReturnsAsync(player);

            // Act
            Func<Task> act = async () => await _sut.CreateTeamAsync(dto, player.Id);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        // --------------------------------------------------------
        // TESTE: Atualizar equipa
        // --------------------------------------------------------
        [Fact(DisplayName = "UpdateTeamInfoAsync deve atualizar os dados da equipa com sucesso")]
        public async Task UpdateTeamInfoAsync_Should_Update_Team()
        {
            // Arrange
            var team = new Teams("Antigo Nome", "Desc", new byte[] { 1, 2, 3 }, new Pitch("Campo", "Rua"))
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var player = new Player
            {
                Id = Guid.NewGuid(),
                IsAdmin = true,
                IdTeam = team.Id
            };
            team.Members.Add(player);

            var dto = new UpdateTeamDto { Name = "Novo Nome", Description = "Nova desc" };

            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(team.Id))
                .ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id))
                .ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name))
                .ReturnsAsync((Teams)null);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // Act
            await _sut.UpdateTeamInfoAsync(team.Id, dto, player.Id);

            // Assert
            team.Name.Should().Be("Novo Nome");
            team.Description.Should().Be("Nova desc");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // --------------------------------------------------------
        // TESTE: Remover jogador da equipa
        // --------------------------------------------------------
        [Fact(DisplayName = "RemovePlayerFromTeamAsync deve remover o jogador da equipa com sucesso")]
        public async Task RemovePlayerFromTeamAsync_Should_Remove_Player()
        {
            // Arrange
            var team = new Teams("FC Test", "desc", new byte[] { 1, 2, 3 }, new Pitch("campo", "morada"))
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var playerToRemove = new Player { Id = Guid.NewGuid(), Name = "Jogador 1", IdTeam = team.Id };
            var playerRemoving = new Player { Id = Guid.NewGuid(), Name = "Admin", IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(playerToRemove);
            team.Members.Add(playerRemoving);

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToRemove.Id)).ReturnsAsync(playerToRemove);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerRemoving.Id)).ReturnsAsync(playerRemoving);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // Act
            await _sut.RemovePlayerFromTeamAsync(team.Id, playerToRemove.Id, playerRemoving.Id);

            // Assert
            team.Members.Should().NotContain(playerToRemove);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // --------------------------------------------------------
        // TESTE: Promover jogador a admin
        // --------------------------------------------------------
        [Fact(DisplayName = "PromotePlayerToAdminAsync deve promover um jogador a admin")]
        public async Task PromotePlayerToAdminAsync_Should_Promote_Player()
        {
            // Arrange
            var team = new Teams("FC Test", "desc", new byte[] { 1, 2, 3 }, new Pitch("campo", "morada"))
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var playerToPromote = new Player
            {
                Id = Guid.NewGuid(),
                Name = "Jogador 1",
                IsAdmin = false,
                IdTeam = team.Id
            };
            var playerPromoting = new Player
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                IsAdmin = true,
                IdTeam = team.Id
            };
            team.Members.Add(playerToPromote);
            team.Members.Add(playerPromoting);

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToPromote.Id)).ReturnsAsync(playerToPromote);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerPromoting.Id)).ReturnsAsync(playerPromoting);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // Act
            await _sut.PromotePlayerToAdminAsync(team.Id, playerToPromote.Id, playerPromoting.Id);

            // Assert
            playerToPromote.IsAdmin.Should().BeTrue();
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}