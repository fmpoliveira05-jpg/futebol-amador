using Application.DTOs.Filters;
using Application.DTOs.Pitch;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators;
using Application.Services;
using Application.Validators;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Moq;
using NUnit.Framework;


namespace Unit.ApplicationTests.ServicesTests
{
    [TestFixture]
    public class TeamServiceTests
    {
        #region Variables
        private Mock<ITeamRepository> _teamRepoMock;
        private Mock<IPlayerRepository> _playerRepoMock;
        private Mock<IUnityOfWork> _unitOfWorkMock;
        private Mock<IRankRepository> _rankRepoMock;
        private Mock<IMembershipRequestRepository> _membershipRequestRepoMock;
        private Mock<INotificationService> _notificationServiceMock;
        private Mock<INotificationFirebaseService> _notificationFireBaseServiceMock;
        private Mock<IPlayerAuthorizationValidator> _playerAuthValidatorMock;
        private TeamValidator _teamValidator;
        private TeamService _sut;
        #endregion

        #region SetUp
        [SetUp]
        public void SetUp()
        {
            _teamRepoMock = new Mock<ITeamRepository>();
            _playerRepoMock = new Mock<IPlayerRepository>();
            _unitOfWorkMock = new Mock<IUnityOfWork>();
            _rankRepoMock = new Mock<IRankRepository>();
            _membershipRequestRepoMock = new Mock<IMembershipRequestRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            _notificationFireBaseServiceMock = new Mock<INotificationFirebaseService>();
            _playerAuthValidatorMock = new Mock<IPlayerAuthorizationValidator>();
            _teamValidator = new TeamValidator();

            _sut = new TeamService(
                _teamRepoMock.Object,
                _playerRepoMock.Object,
                _unitOfWorkMock.Object,
                _teamValidator,
                _rankRepoMock.Object,
                _playerAuthValidatorMock.Object,
                _notificationServiceMock.Object,
                _notificationFireBaseServiceMock.Object
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

        private static MembershipRequest CreateMembershipRequest(Guid id, string playerId, Guid teamId)
        {
            var request = (MembershipRequest)Activator.CreateInstance(typeof(MembershipRequest), nonPublic: true)!;
            request.GetType().GetProperty(nameof(MembershipRequest.Id))!.SetValue(request, id);
            request.GetType().GetProperty(nameof(MembershipRequest.IdPlayer))!.SetValue(request, playerId);
            request.GetType().GetProperty(nameof(MembershipRequest.IdTeam))!.SetValue(request, teamId);
            return request;
        }
        #endregion

        #region Tests

        #region CreateTeamTests
        [Test(Description = "T1GE1- CreateTeamAsync deve criar uma nova equipa com Rank 'Unranked'")]
        public async Task CreateTeamAsync_Should_Create_Team_With_DefaultRank_Unranked()
        {
            // ARRANGE
            var dto = new CreateTeamDto
            {
                Name = "FC Teste",
                Description = "Equipa de teste",
                Icon = "",
                HomePitch = new PitchDto { Name = "Campo Central", Address = "Rua Principal" }
            };
            var unrankedRank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var adminPlayer = new Player { Id = "admin-id-123", IsAdmin = true };

            // Setups
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminPlayer.Id)).ReturnsAsync(adminPlayer);
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync((Team)null);
            _rankRepoMock.Setup(r => r.GetDefaultRankAsync()).ReturnsAsync(unrankedRank);
            _teamRepoMock.Setup(r => r.AddAsync(It.IsAny<Team>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            var result = await _sut.CreateTeamAsync(dto, adminPlayer.Id);

            // ASSERT
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty("porque deve retornar o ID da equipa criada");

            _teamRepoMock.Verify(r => r.AddAsync(It.Is<Team>(t => t.Rank.Name == "Unranked")), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "T2GE1- CreateTeamAsync deve lançar exceção se já existir uma equipa com o mesmo nome")]
        public async Task CreateTeamAsync_Should_Throw_When_Team_Name_Already_Exists()
        {
            // ARRANGE
            var dto = new CreateTeamDto
            {
                Name = "FC Repetido",
                Description = "Equipa duplicada",
                Icon = "",
                HomePitch = new PitchDto { Name = "Campo Velho", Address = "Rua da Bola" }
            };
            var rank = new TestRank("Unranked");
            var existingTeam = new Team(dto.Name, "Outra equipa", "", new Pitch("Campo Antigo", "Rua Antiga"), rank);
            var adminPlayer = new Player { Id = "admin-id-123", IsAdmin = true };
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync(existingTeam);
            _rankRepoMock.Setup(r => r.GetDefaultRankAsync()).ReturnsAsync(rank);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminPlayer.Id)).ReturnsAsync(adminPlayer);
 
            // ACT
            Func<Task> act = async () => await _sut.CreateTeamAsync(dto, adminPlayer.Id);

            // ASSERT
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("*Já existe uma equipa com o nome*");
            _teamRepoMock.Verify(r => r.AddAsync(It.IsAny<Team>()), Times.Never, "porque não deve tentar adicionar uma equipa duplicada");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque nenhuma alteração deve ser persistida");
        }

        [Test(Description = "T3GE1- CreateTeamAsync deve lançar exceção se o jogador já for administrador de uma equipa")]
        public async Task CreateTeamAsync_Should_Throw_When_Player_Is_Already_Admin()
        {
            // ARRANGE
            var rank = new TestRank("Unranked");
            var existingTeam = new Team("FC Alpha", "Equipa atual", "", new Pitch("Campo 1", "Rua 1"), rank);
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
                Icon = "",
                HomePitch = new PitchDto { Name = "Campo Novo", Address = "Rua Nova" }
            };

            // ACT
            Func<Task> act = async () => await _sut.CreateTeamAsync(dto, adminPlayer.Id);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Apenas jogadores sem equipa podem aceder a este recurso!");

            // ASSERT
            _teamRepoMock.Verify(r => r.AddAsync(It.IsAny<Team>()), Times.Never, "porque a criação deve ser bloqueada");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #region UpdateTeamTests
        [Test(Description = "T1GE2- UpdateTeamInfoAsync deve atualizar os dados da equipa com sucesso")]
        public async Task UpdateTeamInfoAsync_Should_Update_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Antigo Nome", "Desc", "", new Pitch("Campo", "Rua"), rank)
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

            var dto = new CreateTeamDto
            {
                Name = "Novo Nome",
                Description = "Nova desc",
                HomePitch = new PitchDto { Name = "Novo Campo", Address = "Nova Rua" }
            };

            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync((Team)null);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.UpdateTeamInfoAsync(team.Id, dto, player.Id);

            // ASSERT
            team.Name.Should().Be("Novo Nome");
            team.Description.Should().Be("Nova desc");
            team.Pitch.Name.Should().Be("Novo Campo");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "T2GE2- UpdateTeamInfoAsync deve lançar exceção se o jogador não for administrador")]
        public async Task UpdateTeamInfoAsync_Should_Throw_If_Not_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Antigo Nome", "Desc", "", new Pitch("Campo", "Rua"), rank)
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
            var dto = new CreateTeamDto { Name = "Novo Nome", Description = "Nova desc" };
            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdmin.Id)).ReturnsAsync(nonAdmin);
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync((Team)null);
            
            // ACT
            Func<Task> act = async () => await _sut.UpdateTeamInfoAsync(team.Id, dto, nonAdmin.Id);
            
            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Apenas administradores de equipa têm acesso a este recurso.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque a atualização não deve ser persistida");
        }

        [Test(Description = "T3GE2- UpdateTeamInfoAsync deve lançar exceção se o admin pertencer a outra equipa")]
        public async Task UpdateTeamInfoAsync_Should_Throw_If_Admin_From_Another_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("Team A", "Desc A", "", new Pitch("Campo A", "Rua A"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var teamB = new Team("Team B", "Desc B", "", new Pitch("Campo B", "Rua B"), rank)
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
            var dto = new CreateTeamDto { Name = "Novo Nome" };
            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(teamB.Id)).ReturnsAsync(teamB);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminOfTeamA.Id)).ReturnsAsync(adminOfTeamA);

            // ACT
            Func<Task> act = async () => await _sut.UpdateTeamInfoAsync(teamB.Id, dto, adminOfTeamA.Id);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("O Utilizador não tem autorização para aceder a este recurso (não faz parte da equipa).");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T4GE2- UpdateTeamInfoAsync deve lançar exceção se a equipa não existir")]
        public async Task UpdateTeamInfoAsync_Should_Throw_If_Team_Not_Found()
        {
            // ARRANGE
            var rank = new TestRank();
            var dto = new CreateTeamDto { Name = "Novo Nome" };
            var teamId = Guid.NewGuid(); // Criar ID fixo
            var admin = new Player { Id = "admin-id-123", IdTeam = teamId, IsAdmin = true };

            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(teamId)).ReturnsAsync((Team)null!);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);

            // ACT
            Func<Task> act = async () => await _sut.UpdateTeamInfoAsync(teamId, dto, admin.Id);

            // ASSERT
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("*não existe*", "porque a equipa não existe na base de dados");

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque não deve persistir alterações");
        }

        [Test(Description = "T5GE2- UpdateTeamInfoAsync deve lançar exceção se o novo nome já pertencer a outra equipa")]
        public async Task UpdateTeamInfoAsync_Should_Throw_When_NewName_Already_Exists()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("FC Original", "Desc A", "", new Pitch("Campo A", "Rua A"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var teamB = new Team("FC Existente", "Desc B", "", new Pitch("Campo B", "Rua B"), rank)
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
            var dto = new CreateTeamDto { Name = "FC Existente" };
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
        [Test(Description = "T1GE3- DeleteTeamAsync deve eliminar a equipa quando o utilizador é administrador")]
        public async Task DeleteTeamAsync_Should_Delete_When_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("ToDelete", "Desc", "", new Pitch("Campo", "Rua"), rank);
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

        [Test(Description = "T2GE3- DeleteTeamAsync deve lançar exceção quando o jogador pertence à equipa mas não é administrador")]
        public async Task DeleteTeamAsync_Should_Throw_When_Player_Is_Not_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("FailDelete", "Desc", "", new Pitch("Campo", "Rua"), rank);
            var player = new Player { Id = "player-id-123", IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(player);
            _teamRepoMock.Setup(r => r.GetTeamForDeletionAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.DeleteTeamAsync(team.Id, player.Id);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Apenas administradores de equipa têm acesso a este recurso.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T3GE3- DeleteTeamAsync deve lançar exceção quando o jogador é admin de outra equipa e tenta eliminar uma equipa que não administra")]
        public async Task DeleteTeamAsync_Should_Throw_When_Player_Is_Admin_Of_Another_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamX = new Team("TeamX", "Desc X", "", new Pitch("Campo X", "Rua X"), rank);
            var teamY = new Team("TeamY", "Desc Y", "", new Pitch("Campo Y", "Rua Y"), rank);
            var playerAdmin = new Player { Id = "admin-id-999", IdTeam = teamX.Id, IsAdmin = true };
            teamX.Members.Add(playerAdmin);
            _teamRepoMock.Setup(r => r.GetTeamForDeletionAsync(teamY.Id)).ReturnsAsync(teamY);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerAdmin.Id)).ReturnsAsync(playerAdmin);

            // ACT
            Func<Task> act = async () => await _sut.DeleteTeamAsync(teamY.Id, playerAdmin.Id);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("O Utilizador não tem autorização para aceder a este recurso (não faz parte da equipa).");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T4GE3- DeleteTeamAsync deve lançar exceção quando a equipa não existe")]
        public async Task DeleteTeamAsync_Should_Throw_When_Team_Not_Found()
        {
            // ARRANGE
            var player = new Player { Id = "player-id-generic", IsAdmin = true };
            _teamRepoMock.Setup(r => r.GetTeamForDeletionAsync(It.IsAny<Guid>()))
                         .ReturnsAsync((Team?)null);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id))
                           .ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.DeleteTeamAsync(Guid.NewGuid(), player.Id);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Apenas jogadores com equipa podem aceder a este recurso!");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T5GE3- DeleteTeamAsync deve permitir a exclusão da equipa mesmo que esteja vazia quando o jogador for administrador")]
        public async Task DeleteTeamAsync_Should_Delete_When_Team_Is_Empty_And_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("EmptyTeam", "Desc", "", new Pitch("Campo", "Rua"), rank);
            var admin = new Player { Id = "admin-id-123", IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForDeletionAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.DeleteTeamAsync(team.Id, admin.Id);

            // ASSERT
            _teamRepoMock.Verify(r => r.DeleteTeam(team), Times.Once, "porque o repositório deve eliminar a equipe");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once, "porque a operação deve ser persistida na base de dados");
        }

        [Test(Description = "T6GE3- DeleteTeamAsync deve lançar exceção quando um jogador sem equipa tenta eliminar uma equipa")]
        public async Task DeleteTeamAsync_Should_Throw_When_Player_Without_Team_Tries_To_Delete_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("TeamToDelete", "Desc", "", new Pitch("Campo", "Rua"), rank)
            {
                Id = Guid.NewGuid()
            };
            var playerWithoutTeam = new Player
            {
                Id = "player-no-team-123",
                IdTeam = null,
                IsAdmin = true
            };
            _teamRepoMock.Setup(r => r.GetTeamForDeletionAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerWithoutTeam.Id)).ReturnsAsync(playerWithoutTeam);

            // ACT
            Func<Task> act = async () => await _sut.DeleteTeamAsync(team.Id, playerWithoutTeam.Id);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Apenas jogadores com equipa podem aceder a este recurso!");
            _teamRepoMock.Verify(r => r.DeleteTeam(It.IsAny<Team>()), Times.Never,
                "porque o jogador sem equipa não deve conseguir eliminar a equipa");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never,
                "porque nenhuma alteração deve ser persistida");
        }

        [Test(Description = "T7GE3- DeleteTeamAsync deve lançar exceção quando o jogador pertence a outra equipa e não é administrador")]
        public async Task DeleteTeamAsync_Should_Throw_When_Player_Belongs_To_Other_Team_And_Not_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamX = new Team("TeamX", "Desc X", "", new Pitch("Campo X", "Rua X"), rank);
            var teamY = new Team("TeamY", "Desc Y", "", new Pitch("Campo Y", "Rua Y"), rank);
            var player = new Player
            {
                Id = "player-id-888",
                IdTeam = teamX.Id,
                IsAdmin = false
            };
            _teamRepoMock.Setup(r => r.GetTeamForDeletionAsync(teamY.Id)).ReturnsAsync(teamY);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.DeleteTeamAsync(teamY.Id, player.Id);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Apenas administradores de equipa têm acesso a este recurso.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #region RemovePlayerTests
        [Test(Description = "T1GEJ1- RemovePlayerFromTeamAsync deve remover o jogador da equipa com sucesso")]
        public async Task RemovePlayerFromTeamAsync_Should_Remove_Player()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("FC Test", "desc", "", new Pitch("campo", "morada"), rank)
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

        [Test(Description = "T2GEJ1- RemovePlayerFromTeamAsync deve lançar exceção se o jogador não for administrador")]
        public async Task RemovePlayerFromTeamAsync_Should_Throw_If_NonAdmin_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("FC Test", "desc", "", new Pitch("campo", "morada"), rank)
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
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Apenas administradores de equipa têm acesso a este recurso.");
            team.Members.Should().Contain(playerToRemove, "porque o jogador não deve ser removido");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque não deve haver persistência sem permissões");
        }

        [Test(Description = "T3GEJ1- RemovePlayerFromTeamAsync deve lançar exceção se o jogador for admin de outra equipa")]
        public async Task RemovePlayerFromTeamAsync_Should_Throw_When_Admin_Of_Other_Team_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("Team A", "Desc A", "", new Pitch("Campo A", "Rua A"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var teamB = new Team("Team B", "Desc B", "", new Pitch("Campo B", "Rua B"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var playerToRemove = new Player { Id = "player-b-1", Name = "Jogador B", IdTeam = teamB.Id };
            var adminOtherTeam = new Player { Id = "admin-a-1", Name = "Admin A", IdTeam = teamA.Id, IsAdmin = true };
            teamB.Members.Add(playerToRemove);
            teamA.Members.Add(adminOtherTeam);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamB.Id)).ReturnsAsync(teamB);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToRemove.Id)).ReturnsAsync(playerToRemove);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminOtherTeam.Id)).ReturnsAsync(adminOtherTeam);

            // ACT
            Func<Task> act = async () => await _sut.RemovePlayerFromTeamAsync(teamB.Id, playerToRemove.Id, adminOtherTeam.Id);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("O Utilizador não tem autorização para aceder a este recurso (não faz parte da equipa).");
            teamB.Members.Should().Contain(playerToRemove);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T4GEJ1- RemovePlayerFromTeamAsync deve lançar exceção se o jogador a remover pertencer a outra equipa")]
        public async Task RemovePlayerFromTeamAsync_Should_Throw_When_Player_To_Remove_Belongs_To_Other_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team X", "Desc X", "", new Pitch("Campo X", "Rua X"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var otherTeam = new Team("Team Y", "Desc Y", "", new Pitch("Campo Y", "Rua Y"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var admin = new Player { Id = "admin-x-1", Name = "Admin X", IdTeam = team.Id, IsAdmin = true };
            var playerToRemove = new Player { Id = "player-y-1", Name = "Jogador Y", IdTeam = otherTeam.Id };
            team.Members.Add(admin);
            otherTeam.Members.Add(playerToRemove);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToRemove.Id)).ReturnsAsync(playerToRemove);

            // ACT
            Func<Task> act = async () => await _sut.RemovePlayerFromTeamAsync(team.Id, playerToRemove.Id, admin.Id);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("O Utilizador não tem autorização para aceder a este recurso (não faz parte da equipa).");
            team.Members.Should().NotContain(playerToRemove);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #region PromotePlayerTests
        [Test(Description = "T1GAE1 - Adicionar um administrador de equipa com sucesso")]
        public async Task PromotePlayerToAdminAsync_Should_Work_When_Admin_Promotes_Member()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("FC Unity", "Desc", "", new Pitch("Campo", "Rua"), rank);
            var admin = new Player { Id = "admin-id-123", IdTeam = team.Id, IsAdmin = true, Team = team};
            var member = new Player { Id = "player-id-123", IdTeam = team.Id, IsAdmin = false, Team = team};
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

        [Test(Description = "T2GAE1 - Tentar adicionar à equipa um administrador já adicionado")]
        public async Task PromotePlayerToAdminAsync_Should_Throw_If_NonAdmin_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team", "Desc", "", new Pitch("Campo", "Rua"), rank);
            var memberPromoting = new Player { Id = "non-admin-id-123", IdTeam = team.Id, IsAdmin = true, Team = team };
            var memberToPromote = new Player { Id = "user-id-123", IdTeam = team.Id, IsAdmin = true, Team = team};
            team.Members.Add(memberPromoting);
            team.Members.Add(memberToPromote);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(memberPromoting.Id)).ReturnsAsync(memberPromoting);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(memberToPromote.Id)).ReturnsAsync(memberToPromote);

            // ACT
            Func<Task> act = async () => await _sut.PromotePlayerToAdminAsync(team.Id, memberToPromote.Id, memberPromoting.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("O jogador alvo já é administrador da equipa.");
        }

        [Test(Description = "T3GAE1 - O Jogador que tenta promover outro jogador não é admin da equipe, mas pertence à equipe")]
        public async Task PromotePlayerToAdminAsync_Should_Throw_When_Player_Already_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team A", "Desc", "", new Pitch("Campo", "Rua"), rank);
            var adminPromoter = new Player { Id = "admin-promoter-1", IdTeam = team.Id, IsAdmin = false, Team = team};
            var playerAlreadyAdmin = new Player { Id = "already-admin-1", IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(adminPromoter);
            team.Members.Add(playerAlreadyAdmin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminPromoter.Id)).ReturnsAsync(adminPromoter);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerAlreadyAdmin.Id)).ReturnsAsync(playerAlreadyAdmin);

            // ACT
            Func<Task> act = async () => await _sut.PromotePlayerToAdminAsync(team.Id, playerAlreadyAdmin.Id, adminPromoter.Id);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Apenas administradores de equipa têm acesso a este recurso.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T4GAE1 - O Admin que tenta promover um jogador da equipe pertence a outra equipe")]
        public async Task PromotePlayerToAdminAsync_Should_Throw_When_Admin_From_Another_Team_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("Team A", "Desc A", "", new Pitch("Campo A", "Rua A"), rank) { Id = Guid.NewGuid() };
            var teamB = new Team("Team B", "Desc B", "", new Pitch("Campo B", "Rua B"), rank) { Id = Guid.NewGuid() };
            var adminOtherTeam = new Player { Id = "admin-other-team-1", IdTeam = teamA.Id, IsAdmin = true };
            var playerToPromote = new Player { Id = "player-to-promote-b", IdTeam = teamB.Id, IsAdmin = false };
            teamA.Members.Add(adminOtherTeam);
            teamB.Members.Add(playerToPromote);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamB.Id)).ReturnsAsync(teamB);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminOtherTeam.Id)).ReturnsAsync(adminOtherTeam);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToPromote.Id)).ReturnsAsync(playerToPromote);

            // ACT
            Func<Task> act = async () => await _sut.PromotePlayerToAdminAsync(teamB.Id, playerToPromote.Id, adminOtherTeam.Id);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("O Utilizador não tem autorização para aceder a este recurso (não faz parte da equipa).");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T5GAE1 - O Jogador alvo da promoção não pertence a nenhuma equipa")]
        public async Task PromotePlayerToAdminAsync_Should_Throw_When_Player_Has_No_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("Team A", "Desc A", "", new Pitch("Campo A", "Rua A"), rank)
            {
                Id = Guid.NewGuid()
            };
            var admin = new Player
            {
                Id = "admin-a-2",
                IdTeam = teamA.Id,
                IsAdmin = true
            };
            var playerToPromote = new Player
            {
                Id = "player-no-team",
                IdTeam = null,
                IsAdmin = false
            };
            teamA.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamA.Id)).ReturnsAsync(teamA);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToPromote.Id)).ReturnsAsync(playerToPromote);

            // ACT
            Func<Task> act = async () => await _sut.PromotePlayerToAdminAsync(teamA.Id, playerToPromote.Id, admin.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("O jogador alvo da promoção não pertence a nenhuma equipa!");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never,
                "porque o jogador não pertence a nenhuma equipa e não deve ser promovido");
        }

        [Test(Description = "T6GAE1 - Tentar promover um jogador, sendo administrador da equipe, mas a equipe já tem 3 administradores")]
        public async Task PromotePlayerToAdminAsync_Should_Throw_When_Team_Already_Has_Three_Admins()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team Y", "Desc Y", "", new Pitch("Campo", "Rua"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var admin1 = new Player { Id = "admin-y-1", IdTeam = team.Id, IsAdmin = true };
            var admin2 = new Player { Id = "admin-y-2", IdTeam = team.Id, IsAdmin = true };
            var admin3 = new Player { Id = "admin-y-3", IdTeam = team.Id, IsAdmin = true };
            var playerToPromote = new Player { Id = "player-y-4", IdTeam = team.Id, IsAdmin = false };
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

        [Test(Description = "T7GAE1 - O Jogador alvo da promoção pertence a outra equipa")]
        public async Task PromotePlayerToAdminAsync_Should_Throw_When_Player_Belongs_To_Another_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("Team A", "Desc A", "", new Pitch("Campo A", "Rua A"), rank) { Id = Guid.NewGuid() };
            var teamB = new Team("Team B", "Desc B", "", new Pitch("Campo B", "Rua B"), rank) { Id = Guid.NewGuid() };
            var admin = new Player { Id = "admin-a-2", IdTeam = teamA.Id, IsAdmin = true };
            var playerToPromote = new Player { Id = "player-b-2", IdTeam = teamB.Id, IsAdmin = false, Team = teamB};
            teamA.Members.Add(admin);
            teamB.Members.Add(playerToPromote);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamA.Id)).ReturnsAsync(teamA);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToPromote.Id)).ReturnsAsync(playerToPromote);

            // ACT
            Func<Task> act = async () => await _sut.PromotePlayerToAdminAsync(teamA.Id, playerToPromote.Id, admin.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("O jogador alvo da promoção não pertence à equipa!");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #region DemoteAdminTests
        [Test(Description = "T1GAE2 - Remover um administrador da equipa com tudo válido")]
        [Ignore("Teste desativado temporariamente devido a alterações na lógica de validação.")]
        public async Task DemoteAdminToPlayerAsync_Should_Work_When_Admin_Relegates_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team", "Desc", "", new Pitch("Campo", "Rua"), rank);

            // CORREÇÃO: Definir datas para garantir hierarquia
            var admin1 = new Player
            {
                Id = "admin-id-123",
                IdTeam = team.Id,
                IsAdmin = true,
                IsAdminLastChangedAt = DateTime.UtcNow.AddYears(-2)
            };

            var admin2 = new Player
            {
                Id = "admin2-id-123",
                IdTeam = team.Id,
                IsAdmin = true,
                IsAdminLastChangedAt = DateTime.UtcNow.AddMonths(-1)
            };

            team.Members.Add(admin1);
            team.Members.Add(admin2);

            // Mocks
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin1.Id)).ReturnsAsync(admin1);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin2.Id)).ReturnsAsync(admin2);
            _notificationServiceMock.Setup(n => n.SendUserAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.DemoteAdminToPlayerAsync(team.Id, admin2.Id, admin1.Id);

            // ASSERT
            admin2.IsAdmin.Should().BeFalse("porque foi rebaixado por um administrador mais antigo");
            admin1.IsAdmin.Should().BeTrue();
        }

        [Test(Description = "T2GAE2 - Tentar remover um administrador da equipa com um jogador da equipa que não seja administrador")]
        public async Task DemoteAdminToPlayerAsync_Should_Throw_If_NonAdmin_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team", "Desc", "", new Pitch("Campo", "Rua"), rank);
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
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Apenas administradores de equipa têm acesso a este recurso.");
        }

        [Test(Description = "T3GAE2 - Tentar remover um administrador da equipa com um jogador de equipe de uma equipe diferente")]
        public async Task DemoteAdminToPlayerAsync_Should_Throw_When_Admin_From_Another_Team_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("Team A", "Desc A", "", new Pitch("Campo A", "Rua A"), rank) { Id = Guid.NewGuid() };
            var teamB = new Team("Team B", "Desc B", "", new Pitch("Campo B", "Rua B"), rank) { Id = Guid.NewGuid() };
            var adminOtherTeam = new Player { Id = "admin-a-3", IdTeam = teamA.Id, IsAdmin = true };
            var adminToDemote = new Player { Id = "admin-b-3", IdTeam = teamB.Id, IsAdmin = true };
            teamA.Members.Add(adminOtherTeam);
            teamB.Members.Add(adminToDemote);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamB.Id)).ReturnsAsync(teamB);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminOtherTeam.Id)).ReturnsAsync(adminOtherTeam);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminToDemote.Id)).ReturnsAsync(adminToDemote);

            // ACT
            Func<Task> act = async () => await _sut.DemoteAdminToPlayerAsync(teamB.Id, adminToDemote.Id, adminOtherTeam.Id);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("O Utilizador não tem autorização para aceder a este recurso (não faz parte da equipa).");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T4GAE2 - Um administrador da equipa tenta remover-se a si próprio")]
        public async Task DemoteAdminToPlayerAsync_Should_Throw_When_Admin_Tries_To_Demote_Self()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team Self", "Desc", "", new Pitch("Campo", "Rua"), rank) { Id = Guid.NewGuid() };
            var admin = new Player { Id = "admin-self-1", IdTeam = team.Id, IsAdmin = true };
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

        [Test(Description = "T5GAE2 - Um jogador sem clube tenta remover um administrador de uma equipa já existente")]
        public async Task DemoteAdminToPlayerAsync_Should_Throw_When_Player_Without_Team_Tries_To_Demote_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Team("Team A", "Desc A", "", new Pitch("Campo A", "Rua A"), rank)
            {
                Id = Guid.NewGuid()
            };
            var playerWithoutTeam = new Player
            {
                Id = "player-no-team-1",
                IdTeam = null,
                IsAdmin = true
            };
            var adminTeam = new Player
            {
                Id = "admin-a-4",
                IdTeam = team.Id,
                IsAdmin = true,
                Team = team
            };

            team.Members.Add(adminTeam);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerWithoutTeam.Id)).ReturnsAsync(playerWithoutTeam);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminTeam.Id)).ReturnsAsync(adminTeam);

            // ACT
            Func<Task> act = async () => await _sut.DemoteAdminToPlayerAsync(team.Id, adminTeam.Id, playerWithoutTeam.Id);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Apenas jogadores com equipa podem aceder a este recurso!");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never,
                "porque o jogador sem clube não deve conseguir rebaixar um administrador de equipa");
        }

        [Test(Description = "T6GAE2 - O Jogador alvo pertence a outra equipa")]
        public async Task DemoteAdminToPlayerAsync_Should_Throw_When_Target_Player_Belongs_To_Other_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Team("Team A", "Desc A", "", new Pitch("Campo A", "Rua A"), rank) { Id = Guid.NewGuid() };
            var teamB = new Team("Team B", "Desc B", "", new Pitch("Campo B", "Rua B"), rank) { Id = Guid.NewGuid() };
            var adminTeamA = new Player { Id = "admin-a-5", IdTeam = teamA.Id, IsAdmin = true, Team = teamA };
            var playerFromOtherTeam = new Player { Id = "player-b-5", IdTeam = teamB.Id, IsAdmin = false, Team = teamB };
            teamA.Members.Add(adminTeamA);
            teamB.Members.Add(playerFromOtherTeam);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamA.Id)).ReturnsAsync(teamA);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminTeamA.Id)).ReturnsAsync(adminTeamA);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerFromOtherTeam.Id)).ReturnsAsync(playerFromOtherTeam);

            // ACT
            Func<Task> act = async () => await _sut.DemoteAdminToPlayerAsync(teamA.Id, playerFromOtherTeam.Id, adminTeamA.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("O jogador alvo pertence a outra equipa!");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never,
                "porque o jogador alvo pertence a outra equipa e não pode ser rebaixado");
        }
        #endregion

        #region GetTeamTests
        [Test(Description = "T1GE4- GetTeamByIdAsync deve devolver os detalhes de uma equipa existente")]
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

        [Test(Description = "T2GE4- GetTeamByIdAsync deve lançar NotFoundException se a equipa não existir")]
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
        [Test(Description = "T1GEJ2- GetTeamPlayersAsync deve devolver a lista de jogadores da equipa")]
        public async Task GetTeamPlayersAsync_Should_Return_Player_List()
        {
            var team = new Team("T", "Desc", "", new Pitch("Campo", "Rua"), new TestRank());
            var p1 = new Player { Id = "player-a", Name = "A", IdTeam = team.Id };
            var p2 = new Player { Id = "player-b", Name = "B", IdTeam = team.Id };
            team.Members.Add(p1);
            team.Members.Add(p2);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            
            var result = await _sut.GetTeamPlayersAsync(team.Id);
            
            result.Should().HaveCount(2);
            result.Select(p => p.Name).Should().Contain(new[] { "A", "B" });
        }

        [Test(Description = "T2GEJ2- GetTeamPlayersAsync deve lançar exceção quando a equipa não existe")]
        public async Task GetTeamPlayersAsync_Should_Throw_When_Team_Not_Found()
        {
            var teamId = Guid.NewGuid();
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId))
                         .ReturnsAsync((Team?)null);

            Func<Task> act = async () => await _sut.GetTeamPlayersAsync(teamId);
            
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("A equipa não existe.");
        }
        #endregion

        #region GetAdminPlayersTests
        [Test(Description = "T1GAE3 - Consultar a lista de administradores de uma equipa válida")]
        public async Task GetTeamPlayersAsyncWithFilters_Should_Return_Admins_When_Team_Exists()
        {
            // ARRANGE
            var team = new Team("Team", "Desc", "", new Pitch("Campo", "Rua"), new TestRank())
            {
                Id = Guid.NewGuid()
            };
            var admin1 = new Player { Id = "admin-1", Name = "Admin 1", IdTeam = team.Id, IsAdmin = true };
            var admin2 = new Player { Id = "admin-2", Name = "Admin 2", IdTeam = team.Id, IsAdmin = true };
            var player = new Player { Id = "player-1", Name = "Player", IdTeam = team.Id, IsAdmin = false };
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

        [Test(Description = "T2GAE3 - Tentar consultar a lista de administradores de uma equipa que não existe")]
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
        [Test(Description = "T3GAE3 - Tentar consultar a lista de administradores da equipa sendo jogador não administrador dessa equipa")]
        public async Task GetTeamPlayersAsyncWithFilters_Should_Throw_When_Player_Is_Not_Admin()
        {
            // ARRANGE
            var team = new Team("Team", "Desc", "", new Pitch("Campo", "Rua"), new TestRank())
            {
                Id = Guid.NewGuid()
            };
            var nonAdminPlayer = new Player { Id = "player-1", Name = "Player", IdTeam = team.Id, IsAdmin = false };
            var filters = new FilterTeamPlayers { IsAdmin = true };
            team.Members.Add(nonAdminPlayer);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdminPlayer.Id)).ReturnsAsync(nonAdminPlayer);

            // ACT
            Func<Task> act = async () => await _sut.GetTeamPlayersAsyncWithFilters(team.Id, filters);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*");
        }

        [Test(Description = "T4GAE3 - Tentar consultar a lista de administradores de uma equipa já existente não pertencendo à equipa")]
        public async Task GetTeamPlayersAsyncWithFilters_Should_Throw_When_Player_Belongs_To_Other_Team()
        {
            // ARRANGE
            var teamA = new Team("Team A", "Desc A", "", new Pitch("Campo A", "Rua A"), new TestRank()) { Id = Guid.NewGuid() };
            var teamB = new Team("Team B", "Desc B", "", new Pitch("Campo B", "Rua B"), new TestRank()) { Id = Guid.NewGuid() };
            var playerTeamA = new Player { Id = "player-a", Name = "Player A", IdTeam = teamA.Id, IsAdmin = false };
            var filters = new FilterTeamPlayers { IsAdmin = true };
            teamA.Members.Add(playerTeamA);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamA.Id)).ReturnsAsync(teamA);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerTeamA.Id)).ReturnsAsync(playerTeamA);

            // ACT
            Func<Task> act = async () => await _sut.GetTeamPlayersAsyncWithFilters(teamB.Id, filters);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("O jogador não pertence à equipa.");
        }
        */
        #endregion

        #endregion
    }
}