using Application.DTOs.MemberShip;
using Application.Interfaces.Repositories;
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
    public class MembershipServiceTests
    {
        #region Variables
        private Mock<IPlayerRepository> _playerRepoMock;
        private Mock<ITeamRepository> _teamRepoMock;
        private Mock<IMembershipRequestRepository> _membershipRequestRepoMock;
        private Mock<IUnityOfWork> _unitOfWorkMock;
        private MembershipService _sut;
        private TeamValidator _teamValidator;
        private PlayerValidator _playerValidator;
        #endregion

        #region SetUp
        [SetUp]
        public void SetUp()
        {
            _playerRepoMock = new Mock<IPlayerRepository>();
            _teamRepoMock = new Mock<ITeamRepository>();
            _membershipRequestRepoMock = new Mock<IMembershipRequestRepository>();
            _unitOfWorkMock = new Mock<IUnityOfWork>();
            _teamValidator = new TeamValidator();
            _playerValidator = new PlayerValidator();

            _sut = new MembershipService(
                _teamRepoMock.Object,
                _playerRepoMock.Object,
                _membershipRequestRepoMock.Object,
                _unitOfWorkMock.Object,
                _teamValidator,
                _playerValidator
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

        private static MembershipRequest CreateMembershipRequest(Guid requestId, string playerId, Guid teamId)
        {
            return new MembershipRequest
            {
                Id = requestId,
                IdPlayer = playerId,
                IdTeam = teamId,
                InviteDate = DateTime.UtcNow,
                IsPlayerSender = true
            };
        }
        #endregion

        #region Tests

        #region AcceptMembershipRequestTests

        [Test(Description = "T1GEPA2- AcceptMembershipRequestAsync deve aceitar o pedido de adesão com sucesso quando o utilizador é admin")]
        public async Task AcceptMembershipRequestAsync_Should_Add_Player_To_Team_When_Admin_Accepts()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = "fake-firebase-uid-admin";
            var playerId = "player-id-abc";
            var rank = new TestRank();
            var team = new Team("FC Unity", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var request = CreateMembershipRequest(requestId, playerId, teamId);
            team.MembershipRequests.Add(request);
            var admin = new Player { Id = adminId, IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(admin);
            var player = new Player { Id = playerId, IdTeam = Guid.Empty };
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestById(requestId)).ReturnsAsync(request);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            // ACT
            await _sut.AcceptMembershipRequest(teamId, requestId, adminId);

            //ASSERT
            team.Members.Should().Contain(player);
            _membershipRequestRepoMock.Verify(r => r.RemoveMembershipRequest(request), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "T2GEPA2- AcceptMembershipRequestAsync deve lançar exceção quando o jogador que tenta aceitar não é administrador da equipa")]
        public async Task AcceptMembershipRequestAsync_Should_Throw_When_NonAdmin_Tries_To_Accept()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var nonAdminId = "non-admin-id-1";
            var playerId = "player-id-1";
            var rank = new TestRank();
            var team = new Team("FC Unity", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var request = CreateMembershipRequest(requestId, playerId, teamId);
            team.MembershipRequests.Add(request);
            var nonAdmin = new Player { Id = nonAdminId, IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(nonAdmin);
            var player = new Player { Id = playerId, IdTeam = Guid.Empty };
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestById(requestId)).ReturnsAsync(request);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdminId)).ReturnsAsync(nonAdmin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.AcceptMembershipRequest(teamId, requestId, nonAdminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T3GEPA2- AcceptMembershipRequestAsync deve lançar exceção quando a equipa já atingiu o número máximo de jogadores")]
        public async Task AcceptMembershipRequestAsync_Should_Throw_When_Team_Is_Full()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = "admin-id-full-team";
            var playerId = "player-id-full-team";
            var rank = new TestRank();
            var team = new Team("FC Full", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            for (int i = 0; i < 31; i++)
                team.Members.Add(new Player { Id = Guid.NewGuid().ToString(), IdTeam = team.Id });
            var request = CreateMembershipRequest(requestId, playerId, teamId);
            team.MembershipRequests.Add(request);
            var admin = new Player { Id = adminId, IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(admin);
            var player = new Player { Id = playerId };
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestById(requestId)).ReturnsAsync(request);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.AcceptMembershipRequest(teamId, requestId, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*máximo de jogadores*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T4GEPA2- AcceptMembershipRequestAsync deve lançar exceção quando o pedido de adesão não existe")]
        public async Task AcceptMembershipRequestAsync_Should_Throw_When_Request_Not_Found()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = "admin-id-req-not-found";
            var rank = new TestRank();
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestById(requestId)).ReturnsAsync((MembershipRequest?)null);

            // ACT
            Func<Task> act = async () => await _sut.AcceptMembershipRequest(teamId, requestId, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não existe*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region RejectMembershipRequestTests

        [Test(Description = "T1GEPA3- RejectMembershipRequestAsync deve rejeitar o pedido de adesão com sucesso quando o utilizador é admin")]
        public async Task RejectMembershipRequestAsync_Should_Remove_Request_When_Admin_Rejects()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = "admin-id-123";
            var playerId = "player-id-abc";
            var rank = new TestRank();
            var team = new Team("FC Reject", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var request = CreateMembershipRequest(requestId, playerId, teamId);
            team.MembershipRequests.Add(request);
            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true };
            team.Members.Add(admin);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestById(requestId)).ReturnsAsync(request);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);

            // ACT
            await _sut.RejectMembershipRequest(teamId, requestId, adminId);

            // ASSERT
            _membershipRequestRepoMock.Verify(r => r.RemoveMembershipRequest(request), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "T2GEPA3- RejectMembershipRequestAsync deve lançar exceção quando o pedido de adesão é rejeitado por um jogador não administrador")]
        public async Task RejectMembershipRequestAsync_Should_Throw_When_Not_Admin()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var playerId = "player-id-abc";
            var rank = new TestRank();
            var team = new Team("FC Reject", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var request = CreateMembershipRequest(requestId, playerId, teamId);
            team.MembershipRequests.Add(request);
            var player = new Player { Id = playerId, IdTeam = teamId, IsAdmin = false };
            team.Members.Add(player);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestById(requestId)).ReturnsAsync(request);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.RejectMembershipRequest(teamId, requestId, playerId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador da equipa*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T3GEPA3- RejectMembershipRequestAsync deve lançar exceção quando o pedido de adesão não existe")]
        public async Task RejectMembershipRequestAsync_Should_Throw_When_Request_Not_Found()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = "admin-not-found";
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestById(requestId)).ReturnsAsync((MembershipRequest?)null);

            // ACT
            Func<Task> act = async () => await _sut.RejectMembershipRequest(teamId, requestId, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não existe*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region GetMembershipRequestsTests

        [Test(Description = "T1GEPA1- GetMembershipRequestsAsync deve devolver os pedidos de adesão quando o utilizador é admin")]
        public async Task GetMembershipRequestsAsync_Should_Return_Requests_When_Admin()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var adminId = "player-id-abc";
            var rank = new TestRank();
            var team = new Team("FC Requests", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true };
            team.Members.Add(admin);
            var request1 = new MembershipRequest { Id = Guid.NewGuid(), IdPlayer = "p1", IdTeam = teamId };
            var request2 = new MembershipRequest { Id = Guid.NewGuid(), IdPlayer = "p2", IdTeam = teamId };
            team.MembershipRequests.Add(request1);
            team.MembershipRequests.Add(request2);
            var requests = new List<MemberShipRequestDto>
            {
                new MemberShipRequestDto { PlayerName = "Jogador 1" },
                new MemberShipRequestDto { PlayerName = "Jogador 2" }
            };
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestsByTeam(teamId)).ReturnsAsync(requests);

            // ACT
            var result = await _sut.GetMembershipRequestsByTeam(teamId, adminId);

            // ASSERT
            result.Should().HaveCount(2);
            result.Select(r => r.PlayerName).Should().Contain(new[] { "Jogador 1", "Jogador 2" });
        }

        [Test(Description = "T2GEPA1- Tentar consultar a lista de pedidos de adesão da equipa sendo jogador não administrador")]
        public async Task GetMembershipRequestsAsync_Should_Throw_ValidationException_When_Not_Admin()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerId = "player-id";
            var rank = new TestRank();
            var team = new Team("FC Requests", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var player = new Player { Id = playerId, IdTeam = teamId, IsAdmin = false };
            team.Members.Add(player);
            var requests = new List<MemberShipRequestDto>
            {
                new MemberShipRequestDto { PlayerName = "Jogador 1" },
                new MemberShipRequestDto { PlayerName = "Jogador 2" }
            };
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestsByTeam(teamId)).ReturnsAsync(requests);

            // ACT
            Func<Task> act = async () => await _sut.GetMembershipRequestsByTeam(teamId, playerId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador da equipa*");
        }

        [Test(Description = "T3GEPA1- Consultar a lista de pedidos de adesão quando o jogador não pertence à equipa")]
        public async Task GetMembershipRequestsAsync_Should_Throw_ValidationException_When_Player_Does_Not_Belong_To_Team()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerId = "player-id";
            var rank = new TestRank();
            var team = new Team("FC Requests", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var player = new Player { Id = playerId, IdTeam = Guid.NewGuid() };
            team.Members.Add(new Player { Id = "admin-id", IsAdmin = true });
            var requests = new List<MemberShipRequestDto>
            {
                new MemberShipRequestDto { PlayerName = "Jogador 1" },
                new MemberShipRequestDto { PlayerName = "Jogador 2" }
            };
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestsByTeam(teamId)).ReturnsAsync(requests);

            // ACT
            Func<Task> act = async () => await _sut.GetMembershipRequestsByTeam(teamId, playerId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence à equipa*");
        }

        #endregion

        #region SendMembershipRequestTests

        [Test(Description = "T1GEPA4- Enviar um pedido de adesão sendo administrador da equipa, a equipa não está cheia e o jogador convidado está sem clube")]
        public async Task SendMembershipRequest_Should_Return_Request_When_Valid()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerIdToInvite = "player-id-to-invite";
            var adminId = "admin-id";
            var rank = new TestRank();
            var team = new Team("FC Unity", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true };
            team.Members.Add(admin);
            var playerToInvite = new Player { Id = playerIdToInvite, IdTeam = Guid.Empty };
            var existingRequest = (MembershipRequest)null;
            var requestDto = new MemberShipRequestDto
            {
                PlayerId = playerToInvite.Id,
                PlayerName = playerToInvite.Name,
                TeamId = team.Id,
                TeamName = team.Name
            };
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId)).ReturnsAsync(existingRequest);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerIdToInvite)).ReturnsAsync(playerToInvite);
            _membershipRequestRepoMock.Setup(r => r.AddMembershipRequest(It.IsAny<MembershipRequest>()));

            // ACT
            var result = await _sut.SendMembershipRequest(teamId, playerIdToInvite, adminId);

            // ASSERT
            result.Should().BeEquivalentTo(requestDto, options => options.Excluding(r => r.RequestDate).Excluding(r => r.RequestId));
            _membershipRequestRepoMock.Verify(r => r.AddMembershipRequest(It.IsAny<MembershipRequest>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "T2GEPA4- Tentar enviar um pedido de adesão não sendo um administrador da equipa, mas sendo jogador comum da equipa")]
        public async Task SendMembershipRequest_Should_Throw_ValidationException_When_Not_Admin()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerIdToInvite = "player-id-to-invite";
            var nonAdminId = "non-admin-id";
            var rank = new TestRank();
            var team = new Team("FC Unity", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var nonAdmin = new Player { Id = nonAdminId, IdTeam = teamId, IsAdmin = false };
            team.Members.Add(nonAdmin);
            var playerToInvite = new Player { Id = playerIdToInvite, IdTeam = Guid.Empty };

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdminId)).ReturnsAsync(nonAdmin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerIdToInvite)).ReturnsAsync(playerToInvite);

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequest(teamId, playerIdToInvite, nonAdminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("O jogador com o Id '" + nonAdminId + "' não é administrador da equipa.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T3GEPA4- Tentar enviar um pedido de adesão sendo administrador da equipa, mas a equipa já está cheia")]
        public async Task SendMembershipRequest_Should_Throw_ValidationException_When_Team_Is_Full()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerIdToInvite = "player-id-to-invite";
            var adminId = "admin-id";
            var rank = new TestRank();
            var team = new Team("FC Unity", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true };
            team.Members.Add(admin);
            foreach (var player in Enumerable.Range(0, 31).Select(i => new Player { Id = $"player{i}" }))
            {
                team.Members.Add(player);
            }
            var playerToInvite = new Player { Id = playerIdToInvite, IdTeam = Guid.Empty };
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(new Player { Id = adminId, IsAdmin = true });
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerIdToInvite)).ReturnsAsync(playerToInvite);

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequest(teamId, playerIdToInvite, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*já atingiu o número máximo de jogadores*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T4GEPA4- Tentar enviar um pedido de adesão sendo administrador da equipa, mas o jogador convidado já pertence a outra equipa")]
        public async Task SendMembershipRequest_Should_Throw_ValidationException_When_Player_Belongs_Another_Team()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerIdToInvite = "player-id-to-invite";
            var adminId = "admin-id";
            var rank = new TestRank();
            var team = new Team("FC Unity", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var playerToInvite = new Player { Id = playerIdToInvite, IdTeam = Guid.NewGuid() };
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(new Player { Id = adminId, IsAdmin = true });
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerIdToInvite)).ReturnsAsync(playerToInvite);

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequest(teamId, playerIdToInvite, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("O jogador já pertence a outra equipa e não pode ser convidado.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T5GEPA4- Tentar enviar um pedido de adesão sendo administrador da equipa a um jogador sem clube que já foi convidado pela mesma equipa")]
        public async Task SendMembershipRequest_Should_Throw_ValidationException_When_ExistingRequest()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerIdToInvite = "player-id-to-invite";
            var adminId = "admin-id";
            var rank = new TestRank();
            var team = new Team("FC Unity", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var playerToInvite = new Player { Id = playerIdToInvite, IdTeam = Guid.Empty };
            var existingRequest = new MembershipRequest { IdPlayer = playerIdToInvite, IdTeam = teamId };
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId)).ReturnsAsync(existingRequest);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(new Player { Id = adminId, IsAdmin = true });
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerIdToInvite)).ReturnsAsync(playerToInvite);

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequest(teamId, playerIdToInvite, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("Já existe um pedido pendente entre a equipa e este jogador.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T6GEPA4- Tentar enviar um pedido de adesão sendo administrador da equipa, mas o jogador convidado já pertence à equipa")]
        public async Task SendMembershipRequest_Should_Throw_ValidationException_When_Player_Belongs_To_Team()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerIdToInvite = "player-id-to-invite";
            var adminId = "admin-id";
            var rank = new TestRank();
            var team = new Team("FC Unity", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var playerToInvite = new Player { Id = playerIdToInvite, IdTeam = teamId };
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(new Player { Id = adminId, IsAdmin = true });
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerIdToInvite)).ReturnsAsync(playerToInvite);

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequest(teamId, playerIdToInvite, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("O jogador já pertence a esta equipa.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #endregion
    }
}