using Application.DTOs;
using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.Pitch;
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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unit.ApplicationTests.ServicesTests
{
    /// <summary>
    /// Testes unitários para a classe <see cref="TeamService"/>.
    /// Estes testes validam o comportamento da camada de aplicação responsável pela gestão de equipas, gestão de jogadores e gestão de administradores.
    /// 
    /// - Utiliza o padrão "AAA" (Arrange, Act, Assert).
    /// - Usa mocks (via Moq) para simular dependências externas.
    /// - Aplica FluentAssertions para maior legibilidade dos asserts.
    /// </summary>
    [TestFixture]
    public class TeamServiceTests
    {
        #region Variables
        private Mock<ITeamRepository> _teamRepoMock;
        private Mock<IPlayerRepository> _playerRepoMock;
        private Mock<IUnityOfWork> _unitOfWorkMock;
        private Mock<IRankRepository> _rankRepoMock;
        private ITeamValidator _validatorReal;
        private TeamService _sut;
        private readonly Mock<IMembershipRequestRepository> _membershipRequestRepoMock = new();
        private readonly Mock<IPlayerValidator> _playerValidatorMock = new();
        #endregion

        #region SetUp
        [SetUp]
        public void SetUp()
        {
            _teamRepoMock = new Mock<ITeamRepository>();
            _playerRepoMock = new Mock<IPlayerRepository>();
            _unitOfWorkMock = new Mock<IUnityOfWork>();
            _rankRepoMock = new Mock<IRankRepository>();
            _validatorReal = new TeamValidator();

            _sut = new TeamService(
            _teamRepoMock.Object,
            _playerRepoMock.Object,
            _unitOfWorkMock.Object,
            _validatorReal,
            _rankRepoMock.Object,
            _membershipRequestRepoMock.Object,
            _playerValidatorMock.Object
            );
        }
        #endregion

        #region Methods Support
        private class TestRank : Rank
        {
            public TestRank(string name = "Bronze")
            {
                typeof(Rank).GetProperty(nameof(Name))!.SetValue(this, name);
                typeof(Rank).GetProperty(nameof(WinPoints))!.SetValue(this, 3);
                typeof(Rank).GetProperty(nameof(DrawPoints))!.SetValue(this, 1);
                typeof(Rank).GetProperty(nameof(LosePoints))!.SetValue(this, 0);
                typeof(Rank).GetProperty(nameof(PointsToPromotion))!.SetValue(this, 100);
            }
        }

        private static MembershipRequest CreateMembershipRequest(Guid id, Guid playerId)
        {
            var request = (MembershipRequest)Activator.CreateInstance(typeof(MembershipRequest), nonPublic: true)!;
            request.GetType().GetProperty(nameof(MembershipRequest.Id))!.SetValue(request, id);
            request.GetType().GetProperty(nameof(MembershipRequest.IdPlayer))!.SetValue(request, playerId);
            return request;
        }
        #endregion

        #region Tests
        #region CreateTeamTests
        [Test(Description = "CreateTeamAsync deve criar uma nova equipa com Rank 'Unranked'")]
        public async Task CreateTeamAsync_Should_Create_Team_With_DefaultRank_Unranked()
        {
            // ARRANGE
            var dto = new CreateTeamDto
            {
                Name = "FC Teste",
                Description = "Equipa de teste",
                icon = new byte[] { 1, 2, 3, 4 },
                HomePitch = new PitchDto { Name = "Campo Central", Address = "Rua Principal" }
            };

            var unrankedRank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var adminPlayer = new Player { Id = "admin-id-123", IsAdmin = true };
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminPlayer.Id)).ReturnsAsync(adminPlayer);

            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync((Team)null);
            _rankRepoMock.Setup(r => r.GetDefaultRankAsync()).ReturnsAsync(unrankedRank);
            _teamRepoMock.Setup(r => r.AddAsync(It.IsAny<Team>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            var result = await _sut.CreateTeamAsync(dto, adminPlayer.Id);

            // ASSERT
            result.Should().NotBeEmpty("porque deve retornar o ID da equipa criada");
            _teamRepoMock.Verify(r => r.AddAsync(It.Is<Team>(t => t.Rank.Name == "Unranked")), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "CreateTeamAsync deve lançar exceção se já existir uma equipa com o mesmo nome")]
        public async Task CreateTeamAsync_Should_Throw_When_Team_Name_Already_Exists()
        {
            // ARRANGE
            var dto = new CreateTeamDto
            {
                Name = "FC Repetido",
                Description = "Equipa duplicada",
                icon = new byte[] { 1, 1, 1 },
                HomePitch = new PitchDto { Name = "Campo Velho", Address = "Rua da Bola" }
            };
            var rank = new TestRank("Unranked");
            var existingTeam = new Team(dto.Name, "Outra equipa", new byte[] { 9, 9 }, new Pitch("Campo Antigo", "Rua Antiga"), rank);
            var adminPlayer = new Player { Id = "admin-id-123", IsAdmin = true };
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync(existingTeam);
            _rankRepoMock.Setup(r => r.GetDefaultRankAsync()).ReturnsAsync(rank);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminPlayer.Id)).ReturnsAsync(adminPlayer);

            // ACT
            Func<Task> act = async () => await _sut.CreateTeamAsync(dto, adminPlayer.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*já existe*", "porque o nome da equipa não pode ser duplicado");
            _teamRepoMock.Verify(r => r.AddAsync(It.IsAny<Team>()), Times.Never, "porque não deve tentar adicionar uma equipa duplicada");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque nenhuma alteração deve ser persistida");
        }

        [Test(Description = "CreateTeamAsync deve lançar exceção se o jogador já for administrador de uma equipa")]
        public async Task CreateTeamAsync_Should_Throw_When_Player_Is_Already_Admin()
        {
            // ARRANGE
            var rank = new TestRank("Unranked");
            var existingTeam = new Team("FC Alpha", "Equipa atual", new byte[] { 1 }, new Pitch("Campo 1", "Rua 1"), rank);
            var adminPlayer = new Player
            {
                Id = "admin-id-123",
                IdTeam = existingTeam.Id,
                IsAdmin = true
            };
            existingTeam.Members.Add(adminPlayer);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminPlayer.Id)).ReturnsAsync(adminPlayer);
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync("FC Nova")).ReturnsAsync((Team)null);
            _rankRepoMock.Setup(r => r.GetDefaultRankAsync()).ReturnsAsync(rank);

            var dto = new CreateTeamDto
            {
                Name = "FC Nova",
                Description = "Nova equipa criada por admin indevido",
                icon = new byte[] { 5, 5, 5 },
                HomePitch = new PitchDto { Name = "Campo Novo", Address = "Rua Nova" }
            };

            // ACT
            Func<Task> act = async () => await _sut.CreateTeamAsync(dto, adminPlayer.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*ja possui uma equipa*", "porque um jogador admin não pode criar uma nova equipa");

            _teamRepoMock.Verify(r => r.AddAsync(It.IsAny<Team>()), Times.Never, "porque a criação deve ser bloqueada");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "CreateTeamAsync deve lançar ValidationException quando o Rank padrão não existe")]
        public async Task CreateTeamAsync_Should_Throw_When_DefaultRank_IsMissing()
        {
            // ARRANGE
            var dto = new CreateTeamDto
            {
                Name = "FC SemRank",
                Description = "Teste",
                icon = new byte[] { 9, 9, 9 },
                HomePitch = new PitchDto { Name = "Campo", Address = "Rua" }
            };
            var adminPlayer = new Player { Id = "admin-id-123", IsAdmin = true };
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync((Team)null);
            _rankRepoMock.Setup(r => r.GetDefaultRankAsync()).ReturnsAsync((Rank)null!);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminPlayer.Id)).ReturnsAsync(adminPlayer);

            // ACT
            Func<Task> act = async () => await _sut.CreateTeamAsync(dto, adminPlayer.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*classificação padrão*");
        }
        #endregion

        #region UpdateTeamTests
        [Test(Description = "UpdateTeamInfoAsync deve atualizar os dados da equipa com sucesso")]
        public async Task UpdateTeamInfoAsync_Should_Update_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Antigo Nome", "Desc", new byte[] { 1, 2, 3 }, new Pitch("Campo", "Rua"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var player = new Player
            {
                Id = "player-id-123",
                IsAdmin = true,
                IdTeam = team.Id
            };
            team.Members.Add(player);
            var dto = new UpdateTeamDto { Name = "Novo Nome", Description = "Nova desc" };
            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync((Team)null);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.UpdateTeamInfoAsync(team.Id, dto, player.Id);

            // ASSERT
            team.Name.Should().Be("Novo Nome");
            team.Description.Should().Be("Nova desc");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "UpdateTeamInfoAsync deve lançar exceção se o jogador não for administrador")]
        public async Task UpdateTeamInfoAsync_Should_Throw_If_Not_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Antigo Nome", "Desc", new byte[] { 1, 2, 3 }, new Pitch("Campo", "Rua"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var nonAdmin = new Player
            {
                Id = "player-id-123",
                IsAdmin = false,
                IdTeam = team.Id
            };
            team.Members.Add(nonAdmin);
            var dto = new UpdateTeamDto { Name = "Novo Nome", Description = "Nova desc" };
            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdmin.Id)).ReturnsAsync(nonAdmin);
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync((Team)null);

            // ACT
            Func<Task> act = async () => await _sut.UpdateTeamInfoAsync(team.Id, dto, nonAdmin.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*", "porque apenas administradores podem atualizar os dados da equipa");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque a atualização não deve ser persistida");
        }

        [Test(Description = "UpdateTeamInfoAsync deve lançar exceção se o admin pertencer a outra equipa")]
        public async Task UpdateTeamInfoAsync_Should_Throw_If_Admin_From_Another_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("Team A", "Desc A", new byte[] { 1 }, new Pitch("Campo A", "Rua A"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var teamB = new Team("Team B", "Desc B", new byte[] { 2 }, new Pitch("Campo B", "Rua B"), rank)
            {
                Id = Guid.NewGuid()
            };
            var adminOfTeamA = new Player
            {
                Id = "admin-id-123",
                IdTeam = teamA.Id,
                IsAdmin = true
            };
            teamA.Members.Add(adminOfTeamA);
            var dto = new UpdateTeamDto { Name = "Novo Nome" };
            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(teamB.Id)).ReturnsAsync(teamB);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminOfTeamA.Id)).ReturnsAsync(adminOfTeamA);

            // ACT
            Func<Task> act = async () => await _sut.UpdateTeamInfoAsync(teamB.Id, dto, adminOfTeamA.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence à equipa*", "porque o admin pertence a outra equipa e não deve ter acesso");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "UpdateTeamInfoAsync deve lançar exceção se a equipa não existir")]
        public async Task UpdateTeamInfoAsync_Should_Throw_If_Team_Not_Found()
        {
            // ARRANGE
            var rank = new TestRank();
            var dto = new UpdateTeamDto { Name = "Novo Nome" };
            var admin = new Player { Id = "admin-id-123", IdTeam = Guid.NewGuid(), IsAdmin = true };
            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(It.IsAny<Guid>())).ReturnsAsync((Team)null!);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);

            // ACT
            Func<Task> act = async () => await _sut.UpdateTeamInfoAsync(Guid.NewGuid(), dto, admin.Id);

            // ASSERT
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("*não existe*", "porque a equipa não existe na base de dados");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque não deve persistir alterações");
        }

        [Test(Description = "UpdateTeamInfoAsync deve lançar exceção se o novo nome já pertencer a outra equipa")]
        public async Task UpdateTeamInfoAsync_Should_Throw_When_NewName_Already_Exists()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("FC Original", "Desc A", new byte[] { 1 }, new Pitch("Campo A", "Rua A"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var teamB = new Team("FC Existente", "Desc B", new byte[] { 2 }, new Pitch("Campo B", "Rua B"), rank)
            {
                Id = Guid.NewGuid()
            };
            var admin = new Player
            {
                Id = "admin-id-123",
                IdTeam = teamA.Id,
                IsAdmin = true
            };
            teamA.Members.Add(admin);
            var dto = new UpdateTeamDto { Name = "FC Existente" };
            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(teamA.Id)).ReturnsAsync(teamA);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync(teamB);

            // ACT
            Func<Task> act = async () => await _sut.UpdateTeamInfoAsync(teamA.Id, dto, admin.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*já existe*", "porque o nome da equipa já está a ser usado por outra equipa");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque a alteração não deve ser persistida");
        }
        #endregion

        #region DeleteTeamTests
        [Test(Description = "DeleteTeamAsync deve eliminar a equipa quando o utilizador é administrador")]
        public async Task DeleteTeamAsync_Should_Delete_When_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("ToDelete", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank);
            var admin = new Player { Id = "admin-id-123", IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForDeletionAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.DeleteTeamAsync(team.Id, admin.Id);

            // ASSERT
            _teamRepoMock.Verify(r => r.DeleteTeam(team), Times.Once, "porque o repositório deve eliminar a equipa");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once, "porque a operação deve ser persistida na base de dados");
        }

        [Test(Description = "DeleteTeamAsync deve lançar exceção quando o jogador pertence à equipa mas não é administrador")]
        public async Task DeleteTeamAsync_Should_Throw_When_Player_Is_Not_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("FailDelete", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank);
            var player = new Player { Id = "player-id-123", IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(player);
            _teamRepoMock.Setup(r => r.GetTeamForDeletionAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.DeleteTeamAsync(team.Id, player.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*não é administrador*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "DeleteTeamAsync deve lançar exceção quando a equipa não existe")]
        public async Task DeleteTeamAsync_Should_Throw_When_Team_Not_Found()
        {
            // ARRANGE
            var player = new Player { Id = Guid.NewGuid(), IsAdmin = true };
            _teamRepoMock.Setup(r => r.GetTeamForDeletionAsync(It.IsAny<Guid>()))
                         .ReturnsAsync((Team?)null);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id))
                           .ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.DeleteTeamAsync(Guid.NewGuid(), player.Id);

            // ASSERT
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*não existe*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "DeleteTeamAsync deve lançar exceção quando o jogador é admin de outra equipa e tenta eliminar uma equipa que não administra")]
        public async Task DeleteTeamAsync_Should_Throw_When_Player_Is_Admin_Of_Another_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamX = new Team("TeamX", "Desc X", new byte[1], new Pitch("Campo X", "Rua X"), rank);
            var teamY = new Team("TeamY", "Desc Y", new byte[1], new Pitch("Campo Y", "Rua Y"), rank);
            var playerAdmin = new Player { Id = Guid.NewGuid(), IdTeam = teamX.Id, IsAdmin = true };
            teamX.Members.Add(playerAdmin);
            _teamRepoMock.Setup(r => r.GetTeamForDeletionAsync(teamY.Id)).ReturnsAsync(teamY);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerAdmin.Id)).ReturnsAsync(playerAdmin);

            // ACT
            Func<Task> act = async () => await _sut.DeleteTeamAsync(teamY.Id, playerAdmin.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*não pertence*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "DeleteTeamAsync deve lançar exceção quando o jogador pertence a outra equipa e não é administrador")]
        public async Task DeleteTeamAsync_Should_Throw_When_Player_Belongs_To_Other_Team_And_Not_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamX = new Team("TeamX", "Desc X", new byte[1], new Pitch("Campo X", "Rua X"), rank);
            var teamY = new Team("TeamY", "Desc Y", new byte[1], new Pitch("Campo Y", "Rua Y"), rank);
            var player = new Player
            {
                Id = Guid.NewGuid(),
                IdTeam = teamX.Id,
                IsAdmin = false
            };
            _teamRepoMock.Setup(r => r.GetTeamForDeletionAsync(teamY.Id)).ReturnsAsync(teamY);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.DeleteTeamAsync(teamY.Id, player.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*não pertence à equipa*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #region RemovePlayerTests
        [Test(Description = "RemovePlayerFromTeamAsync deve remover o jogador da equipa com sucesso")]
        public async Task RemovePlayerFromTeamAsync_Should_Remove_Player()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("FC Test", "desc", new byte[] { 1, 2, 3 }, new Pitch("campo", "morada"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var playerToRemove = new Player { Id = "player-id-123", Name = "Jogador 1", IdTeam = team.Id };
            var playerRemoving = new Player { Id = "admin-id-123", Name = "Admin", IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(playerToRemove);
            team.Members.Add(playerRemoving);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToRemove.Id)).ReturnsAsync(playerToRemove);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerRemoving.Id)).ReturnsAsync(playerRemoving);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.RemovePlayerFromTeamAsync(team.Id, playerToRemove.Id, playerRemoving.Id);

            // ASSERT
            team.Members.Should().NotContain(playerToRemove, "porque o jogador foi removido da equipa");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "RemovePlayerFromTeamAsync deve lançar exceção se o jogador não for administrador")]
        public async Task RemovePlayerFromTeamAsync_Should_Throw_If_NonAdmin_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("FC Test", "desc", new byte[] { 1, 2, 3 }, new Pitch("campo", "morada"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var playerToRemove = new Player { Id = "player-id-123", Name = "Jogador 1", IdTeam = team.Id };
            var playerRemoving = new Player { Id = "player2-id-123", Name = "Jogador Comum", IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(playerToRemove);
            team.Members.Add(playerRemoving);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToRemove.Id)).ReturnsAsync(playerToRemove);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerRemoving.Id)).ReturnsAsync(playerRemoving);

            // ACT
            Func<Task> act = async () => await _sut.RemovePlayerFromTeamAsync(team.Id, playerToRemove.Id, playerRemoving.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*", "porque apenas administradores podem remover jogadores da equipa");
            team.Members.Should().Contain(playerToRemove, "porque o jogador não deve ser removido");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque não deve haver persistência sem permissões");
        }

        [Test(Description = "RemovePlayerFromTeamAsync deve lançar exceção se o jogador for admin de outra equipa")]
        public async Task RemovePlayerFromTeamAsync_Should_Throw_When_Admin_Of_Other_Team_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("Team A", "Desc A", new byte[] { 1 }, new Pitch("Campo A", "Rua A"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var teamB = new Team("Team B", "Desc B", new byte[] { 2 }, new Pitch("Campo B", "Rua B"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var playerToRemove = new Player { Id = Guid.NewGuid(), Name = "Jogador B", IdTeam = teamB.Id };
            var adminOtherTeam = new Player { Id = Guid.NewGuid(), Name = "Admin A", IdTeam = teamA.Id, IsAdmin = true };
            teamB.Members.Add(playerToRemove);
            teamA.Members.Add(adminOtherTeam);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamB.Id)).ReturnsAsync(teamB);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToRemove.Id)).ReturnsAsync(playerToRemove);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminOtherTeam.Id)).ReturnsAsync(adminOtherTeam);

            // ACT
            Func<Task> act = async () => await _sut.RemovePlayerFromTeamAsync(teamB.Id, playerToRemove.Id, adminOtherTeam.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence à equipa*");
            teamB.Members.Should().Contain(playerToRemove);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "RemovePlayerFromTeamAsync deve lançar exceção se o jogador a remover pertencer a outra equipa")]
        public async Task RemovePlayerFromTeamAsync_Should_Throw_When_Player_To_Remove_Belongs_To_Other_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team X", "Desc X", new byte[] { 1 }, new Pitch("Campo X", "Rua X"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var otherTeam = new Team("Team Y", "Desc Y", new byte[] { 2 }, new Pitch("Campo Y", "Rua Y"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var admin = new Player { Id = Guid.NewGuid(), Name = "Admin X", IdTeam = team.Id, IsAdmin = true };
            var playerToRemove = new Player { Id = Guid.NewGuid(), Name = "Jogador Y", IdTeam = otherTeam.Id };
            team.Members.Add(admin);
            otherTeam.Members.Add(playerToRemove);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToRemove.Id)).ReturnsAsync(playerToRemove);

            // ACT
            Func<Task> act = async () => await _sut.RemovePlayerFromTeamAsync(team.Id, playerToRemove.Id, admin.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence à equipa*");
            team.Members.Should().NotContain(playerToRemove);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "RemovePlayerFromTeamAsync deve lançar exceção se o jogador a remover não existir")]
        public async Task RemovePlayerFromTeamAsync_Should_Throw_When_Player_To_Remove_Not_Found()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team W", "Desc W", new byte[] { 1 }, new Pitch("Campo W", "Rua W"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var admin = new Player { Id = Guid.NewGuid(), Name = "Admin W", IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(It.Is<Guid>(id => id != admin.Id))).ReturnsAsync((Player?)null);

            // ACT
            Func<Task> act = async () => await _sut.RemovePlayerFromTeamAsync(team.Id, Guid.NewGuid(), admin.Id);

            // ASSERT
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("Player doesn't exist.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #region PromotePlayerTests
        [Test(Description = "PromotePlayerToAdminAsync deve promover um jogador comum a admin quando o promotor é admin")]
        public async Task PromotePlayerToAdminAsync_Should_Work_When_Admin_Promotes_Member()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("FC Unity", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank);
            var admin = new Player { Id = "admin-id-123", IdTeam = team.Id, IsAdmin = true };
            var member = new Player { Id = "player-id-123", IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(admin);
            team.Members.Add(member);

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(member.Id)).ReturnsAsync(member);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.PromotePlayerToAdminAsync(team.Id, member.Id, admin.Id);

            // ASSERT
            member.IsAdmin.Should().BeTrue("porque o jogador foi promovido por um administrador");
        }

        [Test(Description = "PromotePlayerToAdminAsync deve lançar exceção se o promotor não for admin")]
        public async Task PromotePlayerToAdminAsync_Should_Throw_If_NonAdmin_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank);
            var memberPromoting = new Player { Id = "admin-id-123", IdTeam = team.Id, IsAdmin = false };
            var memberToPromote = new Player { Id = "user-id-123", IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(memberPromoting);
            team.Members.Add(memberToPromote);

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(memberPromoting.Id)).ReturnsAsync(memberPromoting);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(memberToPromote.Id)).ReturnsAsync(memberToPromote);

            // ACT
            Func<Task> act = async () => await _sut.PromotePlayerToAdminAsync(team.Id, memberToPromote.Id, memberPromoting.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*");
        }

        [Test(Description = "PromotePlayerToAdminAsync deve lançar exceção se o jogador já for administrador")]
        public async Task PromotePlayerToAdminAsync_Should_Throw_When_Player_Already_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team A", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank);
            var adminPromoter = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = true };
            var playerAlreadyAdmin = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(adminPromoter);
            team.Members.Add(playerAlreadyAdmin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminPromoter.Id)).ReturnsAsync(adminPromoter);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerAlreadyAdmin.Id)).ReturnsAsync(playerAlreadyAdmin);

            // ACT
            Func<Task> act = async () => await _sut.PromotePlayerToAdminAsync(team.Id, playerAlreadyAdmin.Id, adminPromoter.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*já é administrador da equipa*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "PromotePlayerToAdminAsync deve lançar exceção se o administrador pertencer a outra equipa")]
        public async Task PromotePlayerToAdminAsync_Should_Throw_When_Admin_From_Another_Team_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("Team A", "Desc A", new byte[1], new Pitch("Campo A", "Rua A"), rank) { Id = Guid.NewGuid() };
            var teamB = new Team("Team B", "Desc B", new byte[1], new Pitch("Campo B", "Rua B"), rank) { Id = Guid.NewGuid() };
            var adminOtherTeam = new Player { Id = Guid.NewGuid(), IdTeam = teamA.Id, IsAdmin = true };
            var playerToPromote = new Player { Id = Guid.NewGuid(), IdTeam = teamB.Id, IsAdmin = false };
            teamA.Members.Add(adminOtherTeam);
            teamB.Members.Add(playerToPromote);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamB.Id)).ReturnsAsync(teamB);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminOtherTeam.Id)).ReturnsAsync(adminOtherTeam);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToPromote.Id)).ReturnsAsync(playerToPromote);

            // ACT
            Func<Task> act = async () => await _sut.PromotePlayerToAdminAsync(teamB.Id, playerToPromote.Id, adminOtherTeam.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence à equipa*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "PromotePlayerToAdminAsync deve lançar exceção se o jogador a promover pertencer a outra equipa")]
        public async Task PromotePlayerToAdminAsync_Should_Throw_When_Player_Belongs_To_Other_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("Team A", "Desc A", new byte[1], new Pitch("Campo A", "Rua A"), rank) { Id = Guid.NewGuid() };
            var teamB = new Team("Team B", "Desc B", new byte[1], new Pitch("Campo B", "Rua B"), rank) { Id = Guid.NewGuid() };
            var admin = new Player { Id = Guid.NewGuid(), IdTeam = teamA.Id, IsAdmin = true };
            var playerToPromote = new Player { Id = Guid.NewGuid(), IdTeam = teamB.Id, IsAdmin = false };
            teamA.Members.Add(admin);
            teamB.Members.Add(playerToPromote);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamA.Id)).ReturnsAsync(teamA);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToPromote.Id)).ReturnsAsync(playerToPromote);

            // ACT
            Func<Task> act = async () => await _sut.PromotePlayerToAdminAsync(teamA.Id, playerToPromote.Id, admin.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence a equipa*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "PromotePlayerToAdminAsync deve lançar exceção se a equipa já tiver 3 administradores")]
        public async Task PromotePlayerToAdminAsync_Should_Throw_When_Team_Already_Has_Three_Admins()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team Y", "Desc Y", new byte[1], new Pitch("Campo", "Rua"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var admin1 = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = true };
            var admin2 = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = true };
            var admin3 = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = true };
            var playerToPromote = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(admin1);
            team.Members.Add(admin2);
            team.Members.Add(admin3);
            team.Members.Add(playerToPromote);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin1.Id)).ReturnsAsync(admin1);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToPromote.Id)).ReturnsAsync(playerToPromote);

            // ACT
            Func<Task> act = async () => await _sut.PromotePlayerToAdminAsync(team.Id, playerToPromote.Id, admin1.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*já tem o número máximo de administradores*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #region DemoteAdminTests
        [Test(Description = "DemoteAdminToPlayerAsync deve permitir que um admin rebaixe outro admin")]
        public async Task DemoteAdminToPlayerAsync_Should_Work_When_Admin_Relegates_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank);
            var admin1 = new Player { Id = "admin-id-123", IdTeam = team.Id, IsAdmin = true };
            var admin2 = new Player { Id = "admin2-id-123", IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(admin1);
            team.Members.Add(admin2);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin1.Id)).ReturnsAsync(admin1);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin2.Id)).ReturnsAsync(admin2);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.DemoteAdminToPlayerAsync(team.Id, admin2.Id, admin1.Id);

            // ASSERT
            admin2.IsAdmin.Should().BeFalse("porque foi rebaixado por outro administrador");
        }

        [Test(Description = "DemoteAdminToPlayerAsync deve lançar exceção se o jogador não for admin")]
        public async Task DemoteAdminToPlayerAsync_Should_Throw_If_NonAdmin_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank);
            var nonAdmin = new Player { Id = "nonadmin-id-123", IdTeam = team.Id, IsAdmin = false };
            var admin = new Player { Id = "admin-id-123", IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(nonAdmin);
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdmin.Id)).ReturnsAsync(nonAdmin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);

            // ACT
            Func<Task> act = async () => await _sut.DemoteAdminToPlayerAsync(team.Id, admin.Id, nonAdmin.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*");
        }

        [Test(Description = "DemoteAdminToPlayerAsync deve lançar exceção se o administrador pertencer a outra equipa")]
        public async Task DemoteAdminToPlayerAsync_Should_Throw_When_Admin_From_Another_Team_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("Team A", "Desc A", new byte[1], new Pitch("Campo A", "Rua A"), rank) { Id = Guid.NewGuid() };
            var teamB = new Team("Team B", "Desc B", new byte[1], new Pitch("Campo B", "Rua B"), rank) { Id = Guid.NewGuid() };
            var adminOtherTeam = new Player { Id = Guid.NewGuid(), IdTeam = teamA.Id, IsAdmin = true };
            var adminToDemote = new Player { Id = Guid.NewGuid(), IdTeam = teamB.Id, IsAdmin = true };
            teamA.Members.Add(adminOtherTeam);
            teamB.Members.Add(adminToDemote);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamB.Id)).ReturnsAsync(teamB);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminOtherTeam.Id)).ReturnsAsync(adminOtherTeam);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminToDemote.Id)).ReturnsAsync(adminToDemote);

            // ACT
            Func<Task> act = async () => await _sut.DemoteAdminToPlayerAsync(teamB.Id, adminToDemote.Id, adminOtherTeam.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence à equipa*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "DemoteAdminToPlayerAsync deve lançar exceção se o administrador tentar rebaixar-se a si próprio")]
        public async Task DemoteAdminToPlayerAsync_Should_Throw_When_Admin_Tries_To_Demote_Self()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team Self", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = Guid.NewGuid() };
            var admin = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);

            // ACT
            Func<Task> act = async () => await _sut.DemoteAdminToPlayerAsync(team.Id, admin.Id, admin.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pode rebaixar-se a si próprio*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "DemoteAdminToPlayerAsync deve lançar exceção quando o jogador alvo pertence a outra equipa")]
        public async Task DemoteAdminToPlayerAsync_Should_Throw_When_Target_Player_From_Other_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("Team A", "Desc A", new byte[1], new Pitch("Campo A", "Rua A"), rank) { Id = Guid.NewGuid() };
            var teamB = new Team("Team B", "Desc B", new byte[1], new Pitch("Campo B", "Rua B"), rank) { Id = Guid.NewGuid() };
            var adminTeamA = new Player { Id = Guid.NewGuid(), IdTeam = teamA.Id, IsAdmin = true };
            var adminTeamB = new Player { Id = Guid.NewGuid(), IdTeam = teamB.Id, IsAdmin = true };
            teamA.Members.Add(adminTeamA);
            teamB.Members.Add(adminTeamB);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamA.Id)).ReturnsAsync(teamA);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminTeamA.Id)).ReturnsAsync(adminTeamA);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminTeamB.Id)).ReturnsAsync(adminTeamB);

            // ACT
            Func<Task> act = async () => await _sut.DemoteAdminToPlayerAsync(teamA.Id, adminTeamB.Id, adminTeamA.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence à equipa*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never,
                "porque o jogador alvo pertence a outra equipa e não deve ser rebaixado");
        }
        #endregion

        #region GetTeamTests
        [Test(Description = "GetTeamByIdAsync deve devolver os detalhes de uma equipa existente")]
        public async Task GetTeamByIdAsync_Should_Return_Details()
        {
            // ARRANGE
            var teamDetails = new TeamDetailsDto { Name = "Team X" };
            _teamRepoMock.Setup(r => r.GetTeamDetailsDtoAsync(It.IsAny<Guid>())).ReturnsAsync(teamDetails);

            // ACT
            var result = await _sut.GetTeamByIdAsync(Guid.NewGuid());

            // ASSERT
            result.Should().BeEquivalentTo(teamDetails);
        }

        [Test(Description = "GetTeamByIdAsync deve lançar NotFoundException se a equipa não existir")]
        public async Task GetTeamByIdAsync_Should_Throw_If_NotFound()
        {
            // ARRANGE
            _teamRepoMock.Setup(r => r.GetTeamDetailsDtoAsync(It.IsAny<Guid>())).ReturnsAsync((TeamDetailsDto)null!);

            // ACT
            Func<Task> act = async () => await _sut.GetTeamByIdAsync(Guid.NewGuid());

            // ASSERT
            await act.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region GetTeamPlayersTests
        [Test(Description = "GetTeamPlayersAsync deve devolver a lista de jogadores da equipa")]
        public async Task GetTeamPlayersAsync_Should_Return_Player_List()
        {
            // ARRANGE
            var team = new Team("T", "Desc", new byte[1], new Pitch("Campo", "Rua"), new TestRank());
            var p1 = new Player { Name = "A", IdTeam = team.Id };
            var p2 = new Player { Name = "B", IdTeam = team.Id };
            team.Members.Add(p1);
            team.Members.Add(p2);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);

            // ACT
            var result = await _sut.GetTeamPlayersAsync(team.Id);

            // ASSERT
            result.Should().HaveCount(2);
            result.Select(p => p.Name).Should().Contain(new[] { "A", "B" });
        }

        [Test(Description = "GetTeamPlayersAsync deve lançar exceção quando a equipa não existe")]
        public async Task GetTeamPlayersAsync_Should_Throw_When_Team_Not_Found()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId))
                         .ReturnsAsync((Team?)null);

            // ACT
            Func<Task> act = async () => await _sut.GetTeamPlayersAsync(teamId);

            // ASSERT
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("A equipa não existe.");
        }
        #endregion

        #region GetAdminPlayersTests
        [Test(Description = "GetTeamPlayersAsyncWithFilters deve devolver apenas os administradores da equipa válida")]
        public async Task GetTeamPlayersAsyncWithFilters_Should_Return_Admins_When_Team_Exists()
        {
            // ARRANGE
            var team = new Team("Team", "Desc", new byte[1], new Pitch("Campo", "Rua"), new TestRank())
            {
                Id = Guid.NewGuid()
            };
            var admin1 = new Player { Name = "Admin 1", IdTeam = team.Id, IsAdmin = true };
            var admin2 = new Player { Name = "Admin 2", IdTeam = team.Id, IsAdmin = true };
            var player = new Player { Name = "Player", IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(admin1);
            team.Members.Add(admin2);
            team.Members.Add(player);
            var filters = new FilterTeamPlayers { IsAdmin = true };
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _teamRepoMock.Setup(r => r.GetTeamPlayersDtoAsyncWithFilters(team.Id, filters))
                         .ReturnsAsync(new List<PlayerDetailsDto>
                         {
                     new() { Name = "Admin 1", IsAdmin = true },
                     new() { Name = "Admin 2", IsAdmin = true }
                         });

            // ACT
            var result = await _sut.GetTeamPlayersAsyncWithFilters(team.Id, filters);

            // ASSERT
            result.Should().HaveCount(2);
            result.Should().OnlyContain(p => (bool)p.IsAdmin);
            result.Select(p => p.Name).Should().Contain(new[] { "Admin 1", "Admin 2" });
        }

        [Test(Description = "GetTeamPlayersAsyncWithFilters deve lançar exceção quando a equipa não existe")]
        public async Task GetTeamPlayersAsyncWithFilters_Should_Throw_When_Team_Not_Found()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var filters = new FilterTeamPlayers { IsAdmin = true };
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId))
                         .ReturnsAsync((Team?)null);

            // ACT
            Func<Task> act = async () => await _sut.GetTeamPlayersAsyncWithFilters(teamId, filters);

            // ASSERT
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("A equipa não existe.");
        }

        /*
        [Test(Description = "GetTeamPlayersAsyncWithFilters deve lançar exceção se o jogador não for administrador da equipa")]
        public async Task GetTeamPlayersAsyncWithFilters_Should_Throw_When_NonAdmin_Player_Tries()
        {
            // ARRANGE
            var team = new Teams("Team X", "Desc", new byte[1], new Pitch("Campo", "Rua"), new TestRank())
            {
                Id = Guid.NewGuid()
            };
            var nonAdmin = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(nonAdmin);
            var filters = new FilterTeamPlayers { IsAdmin = true };
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _validatorReal.Setup(v => v.GetTeamMembersValidation(team))
                              .Throws(new ValidationException("O jogador não é administrador"));

            // ACT
            Func<Task> act = async () => await _sut.GetTeamPlayersAsyncWithFilters(team.Id, filters);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*");
        }
        */
        #endregion

        #region AcceptMembershipRequestTests
        [Test(Description = "AcceptMembershipRequestAsync deve aceitar o pedido de adesão com sucesso quando o utilizador é admin")]
        public async Task AcceptMembershipRequestAsync_Should_Add_Player_To_Team_When_Admin_Accepts()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = "fake-firebase-uid-admin";
            var playerId = "player-id-abc";
            var rank = new TestRank();
            var team = new Team("FC Unity", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var request = CreateMembershipRequest(requestId, playerId);
            team.MembershipRequests = team.MembershipRequests ?? new List<MembershipRequest>();
            team.MembershipRequests.Add(request);
            var admin = new Player { Id = adminId, IdTeam = team.Id, IsAdmin = true };
            var player = new Player { Id = playerId, IdTeam = Guid.Empty, MembershipRequests = new List<MembershipRequest> { request } };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.AcceptMembershipRequestAsync(teamId, requestId, adminId);

            // ASSERT
            team.Members.Should().Contain(player);
            team.MembershipRequests.Should().BeEmpty();
            player.MembershipRequests.Should().BeEmpty();
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "AcceptMembershipRequestAsync deve lançar exceção quando o jogador que tenta aceitar não é administrador da equipa")]
        public async Task AcceptMembershipRequestAsync_Should_Throw_When_NonAdmin_Tries_To_Accept()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var nonAdminId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var rank = new TestRank();
            var team = new Team("FC Unity", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var request = CreateMembershipRequest(requestId, playerId);
            team.MembershipRequests.Add(request);
            var nonAdmin = new Player { Id = nonAdminId, IdTeam = team.Id, IsAdmin = false };
            var player = new Player { Id = playerId, IdTeam = Guid.Empty, MembershipRequests = new List<MembershipRequest> { request } };
            team.Members.Add(nonAdmin);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdminId)).ReturnsAsync(nonAdmin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.AcceptMembershipRequestAsync(teamId, requestId, nonAdminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador da equipa*");
            team.Members.Should().NotContain(player, "porque o jogador não deve ser adicionado à equipa");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque a operação deve ser bloqueada");
        }

        [Test(Description = "AcceptMembershipRequestAsync deve lançar exceção quando a equipa já atingiu o número máximo de jogadores")]
        public async Task AcceptMembershipRequestAsync_Should_Throw_When_Team_Is_Full()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var rank = new TestRank();
            var team = new Team("FC Full", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var request = CreateMembershipRequest(requestId, playerId);
            team.MembershipRequests.Add(request);
            for (int i = 0; i < 32; i++)
                team.Members.Add(new Player { Id = Guid.NewGuid(), IdTeam = team.Id });
            var admin = new Player { Id = adminId, IdTeam = team.Id, IsAdmin = true };
            var player = new Player { Id = playerId, IdTeam = Guid.Empty, MembershipRequests = new List<MembershipRequest> { request } };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.AcceptMembershipRequestAsync(teamId, requestId, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*já atingiu o número máximo de jogadores*");
            team.Members.Should().NotContain(player, "porque não deve ser possível adicionar mais jogadores");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "AcceptMembershipRequestAsync deve lançar exceção quando o pedido de adesão não existe")]
        public async Task AcceptMembershipRequestAsync_Should_Throw_When_Request_Not_Found()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var invalidRequestId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var rank = new TestRank();
            var team = new Team("FC Unity", "desc", new byte[1], new Pitch("Campo", "Rua"), rank)
            {
                Id = teamId,
                MembershipRequests = new List<MembershipRequest>()
            };
            var admin = new Player { Id = adminId, IdTeam = team.Id, IsAdmin = true };
            var player = new Player { Id = playerId, IdTeam = Guid.Empty };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.AcceptMembershipRequestAsync(teamId, invalidRequestId, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não possui um pedido de adesão*");
            team.Members.Should().NotContain(player, "porque o pedido não existe e o jogador não deve ser adicionado");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque não deve persistir alterações");
        }
        #endregion

        #region RejectMembershipRequestTests
        [Test(Description = "RejectMembershipRequestAsync deve rejeitar o pedido de adesão com sucesso quando o utilizador é admin")]
        public async Task RejectMembershipRequestAsync_Should_Remove_Request_When_Admin_Rejects()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = "admin-id-123";
            var playerId = "player-id-abc";
            var rank = new TestRank();
            var team = new Team("FC Reject", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var request = CreateMembershipRequest(requestId, playerId);
            team.MembershipRequests.Add(request);
            var admin = new Player { Id = adminId, IdTeam = team.Id, IsAdmin = true };
            var player = new Player { Id = playerId, IdTeam = Guid.Empty, MembershipRequests = new List<MembershipRequest> { request } };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.RejectMembershipRequestAsync(teamId, requestId, adminId);

            // ASSERT
            team.MembershipRequests.Should().BeEmpty("porque o pedido foi removido da equipa");
            player.MembershipRequests.Should().BeEmpty("porque o pedido foi removido do jogador");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "RejectMembershipRequestAsync deve lançar exceção se o utilizador não for admin")]
        public async Task RejectMembershipRequestAsync_Should_Throw_If_Not_Admin()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var nonAdminId = "nonadmin-id-123";
            var playerId = "player-id-abc";
            var rank = new TestRank();
            var team = new Team("FC Reject", "desc", new byte[1], new Pitch("Campo", "Rua"), rank)
            {
                Id = teamId,
                MembershipRequests = new List<MembershipRequest> { CreateMembershipRequest(requestId, playerId) }
            };
            var nonAdmin = new Player { Id = nonAdminId, IdTeam = team.Id, IsAdmin = false };
            var player = new Player { Id = playerId };
            team.Members.Add(nonAdmin);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdminId)).ReturnsAsync(nonAdmin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.RejectMembershipRequestAsync(teamId, requestId, nonAdminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*");
        }

        [Test(Description = "RejectMembershipRequestAsync deve lançar exceção quando o pedido de adesão não existe")]
        public async Task RejectMembershipRequestAsync_Should_Throw_When_Request_Not_Found()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var invalidRequestId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var rank = new TestRank();
            var team = new Team("FC Reject", "desc", new byte[1], new Pitch("Campo", "Rua"), rank)
            {
                Id = teamId,
                MembershipRequests = new List<MembershipRequest>()
            };
            var admin = new Player { Id = adminId, IdTeam = team.Id, IsAdmin = true };
            var player = new Player { Id = playerId, IdTeam = Guid.Empty };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.RejectMembershipRequestAsync(teamId, invalidRequestId, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não possui um pedido de adesão*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque não deve persistir alterações");
        }
        #endregion

        #region GetMembershipRequestsTests
        [Test(Description = "GetMembershipRequestsAsync deve devolver os pedidos de adesão quando o utilizador é admin")]
        public async Task GetMembershipRequestsAsync_Should_Return_Requests_When_Admin()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var adminId = "player-id-abc";
            var rank = new TestRank();
            var team = new Team("FC Requests", "desc", new byte[1], new Pitch("Campo", "Rua"), rank)
            {
                Id = teamId,
                MembershipRequests = new List<MembershipRequest>()
                {
                    new MembershipRequest { Id = Guid.NewGuid() },
                    new MembershipRequest { Id = Guid.NewGuid() }
                }
                    };
                    var admin = new Player { Id = adminId, IdTeam = team.Id, IsAdmin = true };
                    var requests = new List<MemberShipRequestDto>
            {
                new MemberShipRequestDto { PlayerName = "Jogador 1" },
                new MemberShipRequestDto { PlayerName = "Jogador 2" }
            };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _teamRepoMock.Setup(r => r.GetMembershipRequestsDtoAsync(teamId)).ReturnsAsync(requests);

            // ACT
            var result = await _sut.GetMembershipRequestsAsync(teamId, adminId);

            // ASSERT
            result.Should().HaveCount(2, "porque há dois pedidos pendentes");
            result.Select(r => r.PlayerName).Should().Contain(new[] { "Jogador 1", "Jogador 2" });
        }

        [Test(Description = "GetMembershipRequestsAsync deve lançar exceção quando o jogador não é administrador da equipa")]
        public async Task GetMembershipRequestsAsync_Should_Throw_When_Player_Is_Not_Admin()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var nonAdminId = Guid.NewGuid();
            var rank = new TestRank();
            var team = new Team("FC Requests", "desc", new byte[1], new Pitch("Campo", "Rua"), rank)
            {
                Id = teamId
            };
            var nonAdmin = new Player { Id = nonAdminId, IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(nonAdmin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdminId)).ReturnsAsync(nonAdmin);

            // ACT
            Func<Task> act = async () => await _sut.GetMembershipRequestsAsync(teamId, nonAdminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador da equipa*");
            _teamRepoMock.Verify(r => r.GetMembershipRequestsDtoAsync(It.IsAny<Guid>()), Times.Never,
                "porque apenas administradores podem consultar pedidos");
        }

        [Test(Description = "GetMembershipRequestsAsync deve lançar exceção quando o admin pertence a outra equipa")]
        public async Task GetMembershipRequestsAsync_Should_Throw_When_Admin_From_Another_Team_Tries()
        {
            // ARRANGE
            var teamA = new Team("Team A", "desc", new byte[1], new Pitch("Campo A", "Rua A"), new TestRank())
            { Id = Guid.NewGuid() };
            var teamB = new Team("Team B", "desc", new byte[1], new Pitch("Campo B", "Rua B"), new TestRank())
            { Id = Guid.NewGuid() };
            var adminOfTeamA = new Player { Id = Guid.NewGuid(), IdTeam = teamA.Id, IsAdmin = true };
            teamA.Members.Add(adminOfTeamA);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamB.Id)).ReturnsAsync(teamB);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminOfTeamA.Id)).ReturnsAsync(adminOfTeamA);

            // ACT
            Func<Task> act = async () => await _sut.GetMembershipRequestsAsync(teamB.Id, adminOfTeamA.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence à equipa*");
            _teamRepoMock.Verify(r => r.GetMembershipRequestsDtoAsync(It.IsAny<Guid>()), Times.Never,
                "porque um admin de outra equipa não deve aceder aos pedidos desta equipa");
        }

        /*
        [Test(Description = "GetMembershipRequestsAsync deve lançar exceção quando a equipa não existe")]
        public async Task GetMembershipRequestsAsync_Should_Throw_When_Team_Not_Found()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true };
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId))
                         .ReturnsAsync((Teams?)null);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);

            // ACT
            Func<Task> act = async () => await _sut.GetMembershipRequestsAsync(teamId, adminId);

            // ASSERT
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("A equipa não existe.");
            _teamRepoMock.Verify(r => r.GetMembershipRequestsDtoAsync(It.IsAny<Guid>()), Times.Never,
                "porque não deve tentar consultar pedidos de uma equipa inexistente");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never,
                "porque nenhuma operação deve ser persistida");
        }
        #endregion

        #region SendMembershipRequestTests
        [Test(Description = "SendMembershipRequestAsync deve criar um pedido de adesão com sucesso quando o utilizador é admin e o jogador não tem equipa")]
        public async Task SendMembershipRequestAsync_Should_Create_Request_When_Admin_And_Player_Without_Team()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var rank = new TestRank();
            var team = new Teams("FC Invite", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true };
            var playerToInvite = new Player { Id = playerId, IdTeam = Guid.Empty };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(playerToInvite);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestByPlayerAndTeam(playerId, teamId))
                               .ReturnsAsync((MembershipRequests?)null);
            _membershipRequestRepoMock.Setup(r => r.AddMembershipRequest(It.IsAny<MembershipRequests>()))
                               .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            var result = await _sut.SendMembershipRequestAsync(teamId, playerId, adminId);

            // ASSERT
            result.Should().NotBeNull();
            result.TeamId.Should().Be(teamId);
            result.PlayerId.Should().Be(playerId);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        */

        [Test(Description = "SendMembershipRequestAsync deve lançar exceção quando o jogador que tenta enviar não é administrador da equipa")]
        public async Task SendMembershipRequestAsync_Should_Throw_When_NonAdmin_Tries()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var nonAdminId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var rank = new TestRank();
            var team = new Team("FC Invite", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var nonAdmin = new Player { Id = nonAdminId, IdTeam = teamId, IsAdmin = false };
            var playerToInvite = new Player { Id = playerId, IdTeam = Guid.Empty };
            team.Members.Add(nonAdmin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdminId)).ReturnsAsync(nonAdmin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(playerToInvite);

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequestAsync(teamId, playerId, nonAdminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador da equipa*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "SendMembershipRequestAsync deve lançar exceção quando a equipa já atingiu o número máximo de jogadores")]
        public async Task SendMembershipRequestAsync_Should_Throw_When_Team_Is_Full()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var rank = new TestRank();
            var team = new Team("FC Full", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            for (int i = 0; i < 32; i++)
                team.Members.Add(new Player { Id = Guid.NewGuid(), IdTeam = teamId });
            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true };
            var playerToInvite = new Player { Id = playerId, IdTeam = Guid.Empty };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(playerToInvite);

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequestAsync(teamId, playerId, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*já atingiu o número máximo de jogadores*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "SendMembershipRequestAsync deve lançar exceção quando o jogador pertence a outra equipa")]
        public async Task SendMembershipRequestAsync_Should_Throw_When_Player_Belongs_To_Another_Team()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var rank = new TestRank();
            var team = new Team("Team A", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var otherTeamId = Guid.NewGuid();
            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true };
            var player = new Player { Id = playerId, IdTeam = otherTeamId };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequestAsync(teamId, playerId, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("O jogador já pertence a outra equipa e não pode ser convidado.");
        }

        [Test(Description = "SendMembershipRequestAsync deve lançar exceção quando o jogador convidado já pertence à equipa")]
        public async Task SendMembershipRequestAsync_Should_Throw_When_Player_Already_In_Same_Team()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var rank = new TestRank();

            var team = new Team("FC Invite", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };

            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true };
            var playerToInvite = new Player { Id = playerId, IdTeam = teamId };

            team.Members.Add(admin);
            team.Members.Add(playerToInvite);

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(playerToInvite);

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequestAsync(teamId, playerId, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("O jogador já pertence a esta equipa.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "SendMembershipRequestAsync deve lançar exceção quando já existe um pedido pendente entre o jogador e a equipa")]
        public async Task SendMembershipRequestAsync_Should_Throw_When_Request_Already_Exists()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var rank = new TestRank();
            var team = new Team("FC Invite", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true };
            var playerToInvite = new Player { Id = playerId, IdTeam = Guid.Empty };
            team.Members.Add(admin);
            var existingRequest = new MembershipRequest
            {
                Id = Guid.NewGuid(),
                IdPlayer = playerId,
                IdTeam = teamId,
                InviteDate = DateTime.UtcNow,
                IsPlayerSender = false
            };
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(playerToInvite);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestByPlayerAndTeam(playerId, teamId))
                               .ReturnsAsync(existingRequest);

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequestAsync(teamId, playerId, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("Já existe um pedido pendente entre a equipa e este jogador.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion
        #endregion
    }
}