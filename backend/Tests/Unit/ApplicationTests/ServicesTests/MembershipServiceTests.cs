using Application.DTOs.Membership;
using Application.DTOs.MemberShip;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Services;
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
        #endregion

        #region SetUp
        [SetUp]
        public void SetUp()
        {
            _playerRepoMock = new Mock<IPlayerRepository>();
            _teamRepoMock = new Mock<ITeamRepository>();
            _membershipRequestRepoMock = new Mock<IMembershipRequestRepository>();
            _unitOfWorkMock = new Mock<IUnityOfWork>();

            _sut = new MembershipService(
                _playerRepoMock.Object,
                _teamRepoMock.Object,
                _membershipRequestRepoMock.Object,
                _unitOfWorkMock.Object
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

        /*
        #region AcceptMembershipRequestTests

        [Test(Description = "T1GEPA2- AcceptMembershipRequestAsync deve aceitar o pedido de adesão com sucesso quando o utilizador é admin")]
        public async Task AcceptMembershipRequestAsync_Should_Add_Player_To_Team_When_Admin_Accepts()
        {
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = "fake-firebase-uid-admin";
            var playerId = "player-id-abc";
            var rank = new TestRank();
            var team = new Team("FC Unity", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };

            var request = CreateMembershipRequest(requestId, playerId, teamId);

            team.MembershipRequests = new List<MembershipRequest> { request };
            var admin = new Player { Id = adminId, IdTeam = team.Id, IsAdmin = true };
            var player = new Player { Id = playerId, IdTeam = Guid.Empty, MembershipRequests = new List<MembershipRequest> { request } };

            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            await _sut.AcceptMembershipRequest(teamId, requestId);

            team.Members.Should().Contain(player);
            team.MembershipRequests.Should().BeEmpty();
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "T2GEPA2- AcceptMembershipRequestAsync deve lançar exceção quando o jogador que tenta aceitar não é administrador da equipa")]
        public async Task AcceptMembershipRequestAsync_Should_Throw_When_NonAdmin_Tries_To_Accept()
        {
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var nonAdminId = "non-admin-id-1";
            var playerId = "player-id-1";
            var rank = new TestRank();
            var team = new Team("FC Unity", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var request = CreateMembershipRequest(requestId, playerId, teamId);

            team.MembershipRequests.Add(request);
            var nonAdmin = new Player { Id = nonAdminId, IdTeam = team.Id, IsAdmin = false };
            var player = new Player { Id = playerId, IdTeam = Guid.Empty, MembershipRequests = new List<MembershipRequest> { request } };

            team.Members.Add(nonAdmin);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdminId)).ReturnsAsync(nonAdmin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            Func<Task> act = async () => await _sut.AcceptMembershipRequest(teamId, requestId);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador da equipa*");
            team.Members.Should().NotContain(player);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T3GEPA2- AcceptMembershipRequestAsync deve lançar exceção quando a equipa já atingiu o número máximo de jogadores")]
        public async Task AcceptMembershipRequestAsync_Should_Throw_When_Team_Is_Full()
        {
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = "admin-id-full-team";
            var playerId = "player-id-full-team";
            var rank = new TestRank();
            var team = new Team("FC Full", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var request = CreateMembershipRequest(requestId, playerId, teamId);

            team.MembershipRequests.Add(request);
            for (int i = 0; i < 32; i++)
                team.Members.Add(new Player { Id = Guid.NewGuid().ToString(), IdTeam = team.Id });

            var admin = new Player { Id = adminId, IdTeam = team.Id, IsAdmin = true };
            var player = new Player { Id = playerId, IdTeam = Guid.Empty, MembershipRequests = new List<MembershipRequest> { request } };
            team.Members.Add(admin);

            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            Func<Task> act = async () => await _sut.AcceptMembershipRequest(teamId, requestId);

            await act.Should().ThrowAsync<Domain.Exceptions.ValidationException>()
                     .WithMessage("*já atingiu o número máximo de jogadores*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T4GEPA2- AcceptMembershipRequestAsync deve lançar exceção quando o pedido de adesão não existe")]
        public async Task AcceptMembershipRequestAsync_Should_Throw_When_Request_Not_Found()
        {
            var teamId = Guid.NewGuid();
            var invalidRequestId = Guid.NewGuid();
            var adminId = "admin-id-req-not-found";
            var playerId = "player-id-req-not-found";
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

            Func<Task> act = async () => await _sut.AcceptMembershipRequest(teamId, invalidRequestId);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não possui um pedido de adesão*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region RejectMembershipRequestTests
        [Test(Description = "T1GEPA3- RejectMembershipRequestAsync deve rejeitar o pedido de adesão com sucesso quando o utilizador é admin")]
        public async Task RejectMembershipRequestAsync_Should_Remove_Request_When_Admin_Rejects()
        {
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = "admin-id-123";
            var playerId = "player-id-abc";
            var rank = new TestRank();
            var team = new Team("FC Reject", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var request = CreateMembershipRequest(requestId, playerId, teamId);

            team.MembershipRequests.Add(request);
            var admin = new Player { Id = adminId, IdTeam = team.Id, IsAdmin = true };
            var player = new Player { Id = playerId, IdTeam = Guid.Empty, MembershipRequests = new List<MembershipRequest> { request } };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            await _sut.RefuseMembershipRequest(teamId, requestId);

            team.MembershipRequests.Should().BeEmpty("porque o pedido foi removido da equipa");
            player.MembershipRequests.Should().BeEmpty("porque o pedido foi removido do jogador");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "T2GEPA3- RejectMembershipRequestAsync deve lançar exceção se o utilizador não for admin")]
        public async Task RejectMembershipRequestAsync_Should_Throw_If_Not_Admin()
        {
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var nonAdminId = "nonadmin-id-123";
            var playerId = "player-id-abc";
            var rank = new TestRank();
            var team = new Team("FC Reject", "desc", new byte[1], new Pitch("Campo", "Rua"), rank)
            {
                Id = teamId,
                MembershipRequests = new List<MembershipRequest> { CreateMembershipRequest(requestId, playerId, teamId) }
            };
            var nonAdmin = new Player { Id = nonAdminId, IdTeam = team.Id, IsAdmin = false };
            var player = new Player { Id = playerId };

            team.Members.Add(nonAdmin);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdminId)).ReturnsAsync(nonAdmin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            Func<Task> act = async () => await _sut.RefuseMembershipRequest(teamId, requestId);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*");
        }

        [Test(Description = "T3GEPA3- RejectMembershipRequestAsync deve lançar exceção quando o pedido de adesão não existe")]
        public async Task RejectMembershipRequestAsync_Should_Throw_When_Request_Not_Found()
        {
            var teamId = Guid.NewGuid();
            var invalidRequestId = Guid.NewGuid();
            var adminId = "admin-id-reject-not-found";
            var playerId = "player-id-reject-not-found";
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

            Func<Task> act = async () => await _sut.RefuseMembershipRequest(teamId, invalidRequestId);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não possui um pedido de adesão*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque não deve persistir alterações");
        }
        #endregion

        #region GetMembershipRequestsTests
        [Test(Description = "T1GEPA1- GetMembershipRequestsAsync deve devolver os pedidos de adesão quando o utilizador é admin")]
        public async Task GetMembershipRequestsAsync_Should_Return_Requests_When_Admin()
        {
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

            var result = await _sut.GetRequestsSentByPlayer(teamId, adminId);

            result.Should().HaveCount(2, "porque há dois pedidos pendentes");
            result.Select(r => r.PlayerName).Should().Contain(new[] { "Jogador 1", "Jogador 2" });
        }

        [Test(Description = "T2GEPA2- GetMembershipRequestsAsync deve lançar exceção quando o jogador não é administrador da equipa")]
        public async Task GetMembershipRequestsAsync_Should_Throw_When_Player_Is_Not_Admin()
        {
            var teamId = Guid.NewGuid();
            var nonAdminId = "non-admin-id-2";
            var rank = new TestRank();
            var team = new Team("FC Requests", "desc", new byte[1], new Pitch("Campo", "Rua"), rank)
            {
                Id = teamId
            };
            var nonAdmin = new Player { Id = nonAdminId, IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(nonAdmin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdminId)).ReturnsAsync(nonAdmin);

            Func<Task> act = async () => await _sut.GetRequestsSentByPlayer(teamId, nonAdminId);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador da equipa*");
            _teamRepoMock.Verify(r => r.GetMembershipRequestsDtoAsync(It.IsAny<Guid>()), Times.Never,
                "porque apenas administradores podem consultar pedidos");
        }

        [Test(Description = "T3GEPA2- GetMembershipRequestsAsync deve lançar exceção quando o admin pertence a outra equipa")]
        public async Task GetMembershipRequestsAsync_Should_Throw_When_Admin_From_Another_Team_Tries()
        {
            var teamA = new Team("Team A", "desc", new byte[1], new Pitch("Campo A", "Rua A"), new TestRank())
            { Id = Guid.NewGuid() };
            var teamB = new Team("Team B", "desc", new byte[1], new Pitch("Campo B", "Rua B"), new TestRank())
            { Id = Guid.NewGuid() };
            var adminOfTeamA = new Player { Id = "admin-a-5", IdTeam = teamA.Id, IsAdmin = true };
            teamA.Members.Add(adminOfTeamA);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamB.Id)).ReturnsAsync(teamB);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminOfTeamA.Id)).ReturnsAsync(adminOfTeamA);

            Func<Task> act = async () => await _sut.GetRequestsSentByPlayer(teamB.Id, adminOfTeamA.Id);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence à equipa*");
            _teamRepoMock.Verify(r => r.GetMembershipRequestsDtoAsync(It.IsAny<Guid>()), Times.Never,
                "porque um admin de outra equipa não deve aceder aos pedidos desta equipa");
        }
        #endregion

        #region SendMembershipRequestTests
        [Test(Description = "T2GEPA4- SendMembershipRequestAsync deve lançar exceção quando o jogador que tenta enviar não é administrador da equipa")]
        public async Task SendMembershipRequestAsync_Should_Throw_When_NonAdmin_Tries()
        {
            var teamId = Guid.NewGuid();
            var nonAdminId = "non-admin-id-3";
            var playerId = "player-to-invite-id-3";
            var rank = new TestRank();
            var team = new Team("FC Invite", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var nonAdmin = new Player { Id = nonAdminId, IdTeam = teamId, IsAdmin = false };
            var playerToInvite = new Player { Id = playerId, IdTeam = Guid.Empty };
            team.Members.Add(nonAdmin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdminId)).ReturnsAsync(nonAdmin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(playerToInvite);

            Func<Task> act = async () => await _sut.SendMembershipRequest(dto);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador da equipa*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T3GEPA4- SendMembershipRequestAsync deve lançar exceção quando a equipa já atingiu o número máximo de jogadores")]
        public async Task SendMembershipRequestAsync_Should_Throw_When_Team_Is_Full()
        {
            var teamId = Guid.NewGuid();
            var adminId = "admin-id-full-team-send";
            var playerId = "player-id-full-team-send";
            var rank = new TestRank();
            var team = new Team("FC Full", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            for (int i = 0; i < 32; i++)
                team.Members.Add(new Player { Id = Guid.NewGuid().ToString(), IdTeam = teamId });
            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true };
            var playerToInvite = new Player { Id = playerId, IdTeam = Guid.Empty };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(playerToInvite);

            Func<Task> act = async () => await _sut.SendMembershipRequest(dto);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*já atingiu o número máximo de jogadores*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T4GEPA4- SendMembershipRequestAsync deve lançar exceção quando o jogador pertence a outra equipa")]
        public async Task SendMembershipRequestAsync_Should_Throw_When_Player_Belongs_To_Another_Team()
        {
            var teamId = Guid.NewGuid();
            var adminId = "admin-a-6";
            var playerId = "player-b-6";
            var rank = new TestRank();
            var team = new Team("Team A", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var otherTeamId = Guid.NewGuid();
            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true };
            var player = new Player { Id = playerId, IdTeam = otherTeamId };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            Func<Task> act = async () => await _sut.SendMembershipRequest(dto);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("O jogador já pertence a outra equipa e não pode ser convidado.");
        }

        [Test(Description = "T6GEPA4- SendMembershipRequestAsync deve lançar exceção quando o jogador convidado já pertence à equipa")]
        public async Task SendMembershipRequestAsync_Should_Throw_When_Player_Already_In_Same_Team()
        {
            var teamId = Guid.NewGuid();
            var adminId = "admin-id-same-team";
            var playerId = "player-id-same-team";
            var rank = new TestRank();

            var team = new Team("FC Invite", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };

            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true };
            var playerToInvite = new Player { Id = playerId, IdTeam = teamId };

            team.Members.Add(admin);
            team.Members.Add(playerToInvite);

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(playerToInvite);

            Func<Task> act = async () => await _sut.SendMembershipRequest(dto);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("O jogador já pertence a esta equipa.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T5GEPA4- SendMembershipRequestAsync deve lançar exceção quando já existe um pedido pendente entre o jogador e a equipa")]
        public async Task SendMembershipRequestAsync_Should_Throw_When_Request_Already_Exists()
        {
            var teamId = Guid.NewGuid();
            var adminId = "admin-id-req-exists";
            var playerId = "player-id-req-exists";
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

            Func<Task> act = async () => await _sut.SendMembershipRequest(dto);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("Já existe um pedido pendente entre a equipa e este jogador.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion
        */

        #endregion

    }
}
