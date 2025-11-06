using Application.DTOs.MemberShip;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators;
using Application.Services;
using Application.Validators;
using Domain.Entities;
using Domain.Enums;
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
        private Mock<IPlayerValidator> _playerValidatorMock;
        private PlayerAuthorizationValidator authorizationValidator;
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
            _playerValidatorMock = new Mock<IPlayerValidator>();
            authorizationValidator = new PlayerAuthorizationValidator();

            var membershipValidator = new MembershipValidator();
            var notificationServiceMock = new Mock<INotificationService>();

            _sut = new MembershipService(
                _teamRepoMock.Object,
                _playerRepoMock.Object,
                _membershipRequestRepoMock.Object,
                _unitOfWorkMock.Object,
                _teamValidator,
                _playerValidatorMock.Object,
                authorizationValidator,
                membershipValidator,
                notificationServiceMock.Object
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

        private Team BuildValidTeam(Guid? teamId = null)
        {
            return new Team
            {
                Id = teamId ?? Guid.NewGuid(),
                Name = "Equipa Teste",
                Members = new List<Player>()
            };
        }

        private Player BuildValidPlayer(string id = null, Team team = null, bool isAdmin = false, DateTime? creationDate = null)
        {
            var player = new Player
            {
                Id = id ?? $"player-id-{Guid.NewGuid().ToString("N")}",
                Name = "João Silva",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
                Address = "Rua Exemplo 1, Lisboa",
                Email = "joao.silva@example.com",
                Phone = "912345678",
                Position = Position.FORWARD,
                Height = 180,
                CreationDate = creationDate ?? DateTime.UtcNow.AddMonths(-6)
            };

            if (team != null)
            {
                player.Team = team;
                player.IdTeam = team.Id;
                player.IsAdmin = isAdmin;
                team.Members.Add(player);
            }

            return player;
        }

        private List<MemberShipRequestDto> BuildMockMembershipRequestList(Player player)
        {
            return new List<MemberShipRequestDto>
            {
                new MemberShipRequestDto
                {
                    RequestId = Guid.NewGuid(),
                    PlayerName = player.Name,
                    PlayerId = player.Id,
                    TeamId = Guid.NewGuid(),
                    TeamName = "Botafogo",
                    RequestDate = DateTime.Now,
                    IsPlayerSender = false
                },
                new MemberShipRequestDto
                {
                    RequestId = Guid.NewGuid(),
                    PlayerName = player.Name,
                    PlayerId = player.Id,
                    TeamId = Guid.NewGuid(),
                    TeamName = "Fluminense",
                    RequestDate = DateTime.Now,
                    IsPlayerSender = false
                }
            };
        }

        private void FillTeam(Team team)
        {
            for (int i = 0; i < 32; i++)
            {
                team.Members.Add(BuildValidPlayer());
            }
        }
        #endregion

        #region Tests

        #region Team Membership Methods

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
            await _sut.AcceptMembershipRequestTeam(teamId, requestId, adminId);

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
            var nonAdmin = new Player { Id = nonAdminId, IdTeam = team.Id, IsAdmin = false, Team = team};
            team.Members.Add(nonAdmin);
            var player = new Player { Id = playerId, IdTeam = Guid.Empty, Team = null};
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestById(requestId)).ReturnsAsync(request);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdminId)).ReturnsAsync(nonAdmin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.AcceptMembershipRequestTeam(teamId, requestId, nonAdminId);

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
            Func<Task> act = async () => await _sut.AcceptMembershipRequestTeam(teamId, requestId, adminId);

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
            Func<Task> act = async () => await _sut.AcceptMembershipRequestTeam(teamId, requestId, adminId);

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
            await _sut.RejectMembershipRequestTeam(teamId, requestId, admin);

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
            var player = new Player { Id = playerId, IdTeam = teamId, IsAdmin = false, Team = team };
            team.Members.Add(player);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestById(requestId)).ReturnsAsync(request);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.RejectMembershipRequestTeam(teamId, requestId, player);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador da equipa*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        /*
        [Test(Description = "T3GEPA3- RejectMembershipRequestAsync deve lançar exceção quando o pedido de adesão não existe")]
        public async Task RejectMembershipRequestAsync_Should_Throw_When_Request_Not_Found()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = "admin-not-found";
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestById(requestId)).ReturnsAsync((MembershipRequest?)null);

            // ACT
            Func<Task> act = async () => await _sut.RejectMembershipRequestTeam(teamId, requestId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não existe*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        */

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
            var result = await _sut.GetMembershipRequestsByTeam(teamId, admin);

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
            var player = new Player { Id = playerId, IdTeam = teamId, IsAdmin = false, Team = team };
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
            Func<Task> act = async () => await _sut.GetMembershipRequestsByTeam(teamId, player);

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
            Func<Task> act = async () => await _sut.GetMembershipRequestsByTeam(teamId, player);

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
            var result = await _sut.SendMembershipRequestTeam(teamId, playerIdToInvite);

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
            Func<Task> act = async () => await _sut.SendMembershipRequestTeam(teamId, playerIdToInvite);

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
            Func<Task> act = async () => await _sut.SendMembershipRequestTeam(teamId, playerIdToInvite);

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
            Func<Task> act = async () => await _sut.SendMembershipRequestTeam(teamId, playerIdToInvite);

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
            Func<Task> act = async () => await _sut.SendMembershipRequestTeam(teamId, playerIdToInvite);

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
            Func<Task> act = async () => await _sut.SendMembershipRequestTeam(teamId, playerIdToInvite);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("O jogador já pertence a esta equipa.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #endregion

        #region Player Membership Methods

        #region Tests GetMembershipRequestsAsync

        [Test(Description = "Caminho feliz: Get player membership requests should work")]
        public async Task GetMembershipRequests_Should_Work()
        {
            var player = BuildValidPlayer();

            var mockRequests = BuildMockMembershipRequestList(player);

            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);
            _playerValidatorMock.Setup(v => v.PlayerExists(player));
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestsByPlayer(player.Id)).ReturnsAsync(mockRequests);

            var result = await _sut.GetMembershipRequestsAsyncPlayer(player.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].TeamName, Is.EqualTo("Botafogo"));
        }

        [Test(Description = "Validação: Try consulting membership requests while player has team")]
        public async Task GetMembershipRequests_ShouldThrow_WhilePlayerHasTeam()
        {
            var player = BuildValidPlayer(null, BuildValidTeam(), false, null);

            var mockRequests = BuildMockMembershipRequestList(player);

            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);
            _playerValidatorMock
                .Setup(v => v.PlayerExists(player))
                .Throws(new BusinessRuleException("Player already has a team."));
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestsByPlayer(player.Id)).ReturnsAsync(mockRequests);

            Assert.ThrowsAsync<BusinessRuleException>(async () =>
                await _sut.GetMembershipRequestsAsyncPlayer(player.Id)
            );
        }

        [Test(Description = "Validação: GetMembershipRequests_ShouldThrow_TryingToAcessAnotherPlayerRequests")]
        public async Task GetMembershipRequests_ShouldThrow_TryingToAcessAnotherPlayerRequests()
        {
            var otherPlayer = BuildValidPlayer();

            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(otherPlayer.Id)).ReturnsAsync(otherPlayer);
            _playerValidatorMock
                .Setup(v => v.PlayerExists(otherPlayer))
                .Throws(new BusinessRuleException("Player already has a team."));

            Assert.ThrowsAsync<BusinessRuleException>(async () =>
                await _sut.GetMembershipRequestsAsyncPlayer(otherPlayer.Id)
            );
        }

        #endregion

        #region Test SendMemberShipRequestAsync
        [Test(Description = "Caminho feliz: SendMembershipRequestAsync_ShouldWork")]
        public async Task SendMembershipRequestAsync_ShouldWork()
        {
            var player = BuildValidPlayer();
            var team = BuildValidTeam();

            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(team.Id)).ReturnsAsync(team);

            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestByPlayerAndTeam(player.Id, team.Id))
                .ReturnsAsync((MembershipRequest)null);

            _playerValidatorMock.Setup(v => v.SendMembershipRequestValidator(player, team, null)).Verifiable();

            _membershipRequestRepoMock.Setup(r => r.AddMembershipRequest(It.IsAny<MembershipRequest>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            var result = await _sut.SendMembershipRequestAsyncPlayer(player.Id, team.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.PlayerId, Is.EqualTo(player.Id));
            Assert.That(result.TeamId, Is.EqualTo(team.Id));
            Assert.That(result.IsPlayerSender, Is.True);

            _membershipRequestRepoMock.Verify(r => r.AddMembershipRequest(It.IsAny<MembershipRequest>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _playerValidatorMock.Verify(v => v.SendMembershipRequestValidator(player, team, null), Times.Once);
        }

        [Test(Description = "Validação: SendMembershipRequestAsync_ShouldThrow_WhilePlayerHasTeam")]
        public void SendMembershipRequestAsync_ShouldThrow_WhilePlayerHasTeam()
        {
            var player = BuildValidPlayer(null, BuildValidTeam());
            var team = BuildValidTeam();
            var request = (MembershipRequest)null;

            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(team.Id)).ReturnsAsync(team);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestByPlayerAndTeam(player.Id, team.Id)).ReturnsAsync(request);

            _playerValidatorMock
                .Setup(v => v.SendMembershipRequestValidator(player, team, request))
                .Throws(new BusinessRuleException("Player is already on a team and can't send membership requests."));

            Assert.ThrowsAsync<BusinessRuleException>(async () =>
                await _sut.SendMembershipRequestAsyncPlayer(player.Id, team.Id)
            );
        }


        [Test(Description = "Validação: SendMembershipRequestAsync_ShouldThrow_WhileTeamIsFull")]
        public void SendMembershipRequestAsync_ShouldThrow_WhileTeamIsFull()
        {
            var player = BuildValidPlayer();
            var team = BuildValidTeam();
            FillTeam(team);
            var request = (MembershipRequest)null;

            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(team.Id)).ReturnsAsync(team);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestByPlayerAndTeam(player.Id, team.Id)).ReturnsAsync(request);

            _playerValidatorMock
                .Setup(v => v.SendMembershipRequestValidator(player, team, request))
                .Throws(new BusinessRuleException("Team is full and cannot accept new requests."));

            Assert.ThrowsAsync<BusinessRuleException>(async () =>
                await _sut.SendMembershipRequestAsyncPlayer(player.Id, team.Id)
            );
        }

        [Test(Description = "Validação: SendMembershipRequestAsync_ShouldThrow_WhileAlreadySentRequestToSameTeam")]
        public void SendMembershipRequestAsync_ShouldThrow_WhileAlreadySentRequestToSameTeam()
        {
            var player = BuildValidPlayer();
            var team = BuildValidTeam();
            var request = new MembershipRequest
            {
                Id = Guid.NewGuid(),
                IdPlayer = player.Id,
                IdTeam = team.Id
            };

            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(team.Id)).ReturnsAsync(team);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestByPlayerAndTeam(player.Id, team.Id)).ReturnsAsync(request);

            _playerValidatorMock
                .Setup(v => v.SendMembershipRequestValidator(player, team, request))
                .Throws(new BusinessRuleException("Player already has a pending request for this Team."));

            Assert.ThrowsAsync<BusinessRuleException>(async () =>
                await _sut.SendMembershipRequestAsyncPlayer(player.Id, team.Id)
            );
        }

        #endregion

        #region Test RejectMembershipRequests
        [Test(Description = "Caminho feliz: RejectMembershipRequest_Should_Work_When_PlayerWithoutTeam")]
        public async Task RejectMembershipRequest_Should_Work_When_PlayerWithoutTeam()
        {
            var playerId = "player-rejecting-1";
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var membershipRequest = new MembershipRequest
            {
                Id = requestId,
                IdPlayer = playerId,
                IdTeam = teamId,
                InviteDate = DateTime.UtcNow,
                IsPlayerSender = false
            };

            var player = new Player
            {
                Id = playerId,
                Name = "John",
                Team = null,
                MembershipRequests = new List<MembershipRequest> { membershipRequest }
            };

            var team = new Team { Id = teamId, Name = "Team A" };

            _playerRepoMock.Setup(r => r.GetPlayerByIdWithRequestsAsync(playerId))
                .ReturnsAsync(player);
            _playerValidatorMock.Setup(v => v.PlayerExists(player));
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId))
                .ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamByIdAsync(teamId))
                .ReturnsAsync(team);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            var result = await _sut.RejectMembershipRequestAsyncPlayer(playerId, requestId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RequestId, Is.EqualTo(requestId));
            Assert.That(result.TeamName, Is.EqualTo("Team A"));
            Assert.That(player.MembershipRequests, Is.Empty, "The membership request should be removed from the player's list.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "Validação: RejectMembershipRequest_Should_Throw_When_PlayerHasTeam")]
        public void RejectMembershipRequest_Should_Throw_When_PlayerHasTeam()
        {
            var playerId = "player-rejecting-2-has-team";
            var requestId = Guid.NewGuid();

            var player = new Player
            {
                Id = playerId,
                Name = "PlayerWithTeam",
                Team = new Team { Id = Guid.NewGuid(), Name = "ExistingTeam" }
            };

            _playerRepoMock.Setup(r => r.GetPlayerByIdWithRequestsAsync(playerId))
                .ReturnsAsync(player);

            _playerValidatorMock.Setup(v => v.PlayerExists(player))
                .Throws(new BusinessRuleException("Jogador já pertence a uma equipa"));

            Assert.ThrowsAsync<BusinessRuleException>(async () =>
                await _sut.RejectMembershipRequestAsyncPlayer(playerId, requestId));
        }

        [Test(Description = "Caminho feliz: RejectMembershipRequest_Should_RemoveRequestFromList")]
        public async Task RejectMembershipRequest_Should_RemoveRequestFromList()
        {
            var playerId = "player-rejecting-3";
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var membershipRequest = new MembershipRequest
            {
                Id = requestId,
                IdPlayer = playerId,
                IdTeam = teamId,
                InviteDate = DateTime.UtcNow
            };

            var player = new Player
            {
                Id = playerId,
                Name = "John",
                MembershipRequests = new List<MembershipRequest> { membershipRequest }
            };

            var team = new Team { Id = teamId, Name = "Team A" };

            _playerRepoMock.Setup(r => r.GetPlayerByIdWithRequestsAsync(playerId))
                 .ReturnsAsync(player);
            _playerValidatorMock.Setup(v => v.PlayerExists(player));
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId))
                .ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamByIdAsync(teamId))
                .ReturnsAsync(team);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            await _sut.RejectMembershipRequestAsyncPlayer(playerId, requestId);

            Assert.That(player.MembershipRequests.Count, Is.EqualTo(0), "The request should be removed after rejection.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        #endregion

        #region Test AcceptMembershipRequest
        [Test(Description = "Caminho feliz: AcceptMembershipRequest_Should_Work_When_PlayerWithoutTeam")]
        public async Task AcceptMembershipRequest_Should_Work_When_PlayerWithoutTeam()
        {
            var playerId = "player-accepting-1";
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var membershipRequest = new MembershipRequest
            {
                Id = requestId,
                IdPlayer = playerId,
                IdTeam = teamId,
                InviteDate = DateTime.UtcNow,
                IsPlayerSender = false
            };

            var player = new Player
            {
                Id = playerId,
                Name = "John",
                Team = null,
                MembershipRequests = new List<MembershipRequest> { membershipRequest }
            };

            var team = new Team
            {
                Id = teamId,
                Name = "Team A",
                Members = new List<Player>(),
                MembershipRequests = new List<MembershipRequest> { membershipRequest }
            };

            _playerRepoMock.Setup(r => r.GetPlayerByIdWithRequestsAsync(playerId)).ReturnsAsync(player);
            _playerValidatorMock.Setup(v => v.PlayerExists(player));
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamByIdAsync(teamId)).ReturnsAsync(team);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            var result = await _sut.AcceptMembershipRequestAsyncPlayer(playerId, requestId);

            Assert.That(result, Is.Not.Null);
            Assert.That(player.IdTeam, Is.EqualTo(teamId));
            Assert.That(team.Members, Does.Contain(player));
            Assert.That(player.MembershipRequests, Is.Empty);
            Assert.That(team.MembershipRequests, Is.Empty);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "Validação: AcceptMembershipRequest_Should_Throw_When_PlayerAlreadyHasTeam")]
        public void AcceptMembershipRequest_Should_Throw_When_PlayerAlreadyHasTeam()
        {
            var playerId = "player-accepting-2-has-team";
            var requestId = Guid.NewGuid();

            var player = new Player
            {
                Id = playerId,
                Name = "PlayerWithTeam",
                Team = new Team { Id = Guid.NewGuid(), Name = "ExistingTeam" }
            };

            _playerRepoMock.Setup(r => r.GetPlayerByIdWithRequestsAsync(playerId)).ReturnsAsync(player);
            _playerValidatorMock.Setup(v => v.PlayerExists(player))
                .Throws(new BusinessRuleException("Jogador já pertence a uma equipa"));

            Assert.ThrowsAsync<BusinessRuleException>(async () =>
                await _sut.AcceptMembershipRequestAsyncPlayer(playerId, requestId));
        }

        [Test(Description = "Caminho feliz: AcceptMembershipRequest_Should_RemoveRequestFromLists")]
        public async Task AcceptMembershipRequest_Should_RemoveRequestFromLists()
        {
            var playerId = "player-accepting-3";
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var membershipRequest = new MembershipRequest
            {
                Id = requestId,
                IdPlayer = playerId,
                IdTeam = teamId
            };

            var player = new Player
            {
                Id = playerId,
                Name = "Player",
                MembershipRequests = new List<MembershipRequest> { membershipRequest }
            };

            var team = new Team
            {
                Id = teamId,
                Name = "Team",
                Members = new List<Player>(),
                MembershipRequests = new List<MembershipRequest> { membershipRequest }
            };

            _playerRepoMock.Setup(r => r.GetPlayerByIdWithRequestsAsync(playerId)).ReturnsAsync(player);
            _playerValidatorMock.Setup(v => v.PlayerExists(player));
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamByIdAsync(teamId)).ReturnsAsync(team);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            await _sut.AcceptMembershipRequestAsyncPlayer(playerId, requestId);

            Assert.That(player.MembershipRequests, Is.Empty);
            Assert.That(team.MembershipRequests, Is.Empty);
        }

        [Test(Description = "BUG: Este teste deve falhar. O serviço não limpa os outros pedidos.")]
        public async Task AcceptMembershipRequest_Should_RemoveOtherRequests_When_JoiningTeam()
        {
            var playerId = "player-accepting-4";
            var teamId1 = Guid.NewGuid();
            var teamId2 = Guid.NewGuid();
            var acceptedRequestId = Guid.NewGuid();

            var acceptedRequest = new MembershipRequest { Id = acceptedRequestId, IdPlayer = playerId, IdTeam = teamId1 };
            var otherRequest = new MembershipRequest { Id = Guid.NewGuid(), IdPlayer = playerId, IdTeam = teamId2 };

            var player = new Player
            {
                Id = playerId,
                Name = "Player",
                MembershipRequests = new List<MembershipRequest> { acceptedRequest, otherRequest }
            };

            var team = new Team
            {
                Id = teamId1,
                Name = "Team 1",
                Members = new List<Player>(),
                MembershipRequests = new List<MembershipRequest> { acceptedRequest }
            };

            _playerRepoMock.Setup(r => r.GetPlayerByIdWithRequestsAsync(playerId)).ReturnsAsync(player);
            _playerValidatorMock.Setup(v => v.PlayerExists(player));
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId1)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamByIdAsync(teamId1)).ReturnsAsync(team);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            await _sut.AcceptMembershipRequestAsyncPlayer(playerId, acceptedRequestId);

            Assert.That(player.MembershipRequests, Is.Empty, "All other membership requests should be cleared when joining a team.");
            Assert.That(player.IdTeam, Is.EqualTo(teamId1));
        }
        #endregion

        #endregion

        #endregion
    }
}