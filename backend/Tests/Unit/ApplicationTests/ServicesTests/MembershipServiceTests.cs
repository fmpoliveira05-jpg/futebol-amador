using Application.DTOs.MemberShip;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators;
using Application.Services;
using Application.Validators;
using Domain.Constants;
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
        private Mock<IPlayerValidator> _playerValidatorMock;
        private Mock<IMembershipValidator> _membershipValidatorMock;
        private Mock<INotificationService> _notificationServiceMock;
        private TeamValidator _teamValidator;
        private PlayerAuthorizationValidator _authorizationValidator;

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
            _playerValidatorMock = new Mock<IPlayerValidator>();
            _membershipValidatorMock = new Mock<IMembershipValidator>();
            _notificationServiceMock = new Mock<INotificationService>();

            _teamValidator = new TeamValidator();
            _authorizationValidator = new PlayerAuthorizationValidator();

            _sut = new MembershipService(
                _teamRepoMock.Object,
                _playerRepoMock.Object,
                _membershipRequestRepoMock.Object,
                _unitOfWorkMock.Object,
                _teamValidator,
                _playerValidatorMock.Object,
                _authorizationValidator,
                _membershipValidatorMock.Object,
                _notificationServiceMock.Object
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

        [Test(Description = "Validação: Lança exceção quando um não-admin tenta aceitar")]
        public async Task AcceptMembershipRequestAsync_Should_Throw_When_NonAdmin_Tries_To_Accept()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = "player-nao-admin";
            var team = new Team { Id = teamId };
            var request = new MembershipRequest { Id = requestId, IdTeam = teamId };

            var playerNaoAdmin = new Player
            {
                Id = adminId,
                IsAdmin = false
            };

            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestById(requestId))
                .ReturnsAsync(request);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId))
                .ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId))
                .ReturnsAsync(playerNaoAdmin);
            _membershipValidatorMock
                .Setup(v => v.ValidateAcceptRequestByTeam(team, request, playerNaoAdmin))
                .Throws(new ValidationException("O jogador não é administrador."));

            // ACT
            Func<Task> act = async () => await _sut.AcceptMembershipRequestTeam(teamId, requestId, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*");
        }

        [Test(Description = "Validação: Lança exceção quando a equipa está cheia")]
        public async Task AcceptMembershipRequestAsync_Should_Throw_When_Team_Is_Full()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = "admin-player";

            var teamFull = new Team
            {
                Id = teamId,
                Members = new List<Player>()
            };

            for (int i = 0; i < ModelConstants.TeamConst.MaxMembers; i++)
            {
                teamFull.Members.Add(new Player());
            }

            var request = new MembershipRequest { Id = requestId, IdTeam = teamId };
            var playerAdmin = new Player { Id = adminId, IsAdmin = true };

            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestById(requestId))
                .ReturnsAsync(request);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId))
                .ReturnsAsync(teamFull);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId))
                .ReturnsAsync(playerAdmin);

            _membershipValidatorMock
                .Setup(v => v.ValidateAcceptRequestByTeam(teamFull, request, playerAdmin))
                .Throws(new ValidationException("A equipa está cheia."));

            // ACT
            Func<Task> act = async () => await _sut.AcceptMembershipRequestTeam(teamId, requestId, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*equipa está cheia*");
        }

        [Test(Description = "Validação: Lança exceção quando o pedido não é encontrado")]
        public async Task AcceptMembershipRequestAsync_Should_Throw_When_Request_Not_Found()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = "admin-player";

            var team = new Team { Id = teamId };
            var playerAdmin = new Player { Id = adminId, IsAdmin = true };

            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId))
                .ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId))
                .ReturnsAsync(playerAdmin);

            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestById(requestId))
                .ReturnsAsync((MembershipRequest)null);

            _membershipValidatorMock
                .Setup(v => v.ValidateAcceptRequestByTeam(team, (MembershipRequest)null, playerAdmin))
                .Throws(new ValidationException("Pedido não encontrado."));

            // ACT
            Func<Task> act = async () => await _sut.AcceptMembershipRequestTeam(teamId, requestId, adminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não encontrado*");
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
            await _sut.RejectMembershipRequestTeam(teamId, requestId, admin.Id);

            // ASSERT
            _membershipRequestRepoMock.Verify(r => r.RemoveMembershipRequest(request), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "Validação: Lança exceção quando um não-admin tenta rejeitar")]
        public async Task RejectMembershipRequestAsync_Should_Throw_When_Not_Admin()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var playerId = "player-nao-admin";

            var playerNaoAdmin = new Player
            {
                Id = playerId,
                IsAdmin = false
            };

            var team = new Team { Id = teamId };
            var request = new MembershipRequest { Id = requestId, IdTeam = teamId };

            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestById(requestId))
                .ReturnsAsync(request);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId))
                .ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId))
                .ReturnsAsync(playerNaoAdmin);

            _membershipValidatorMock
                .Setup(v => v.ValidateRejectRequestByTeam(team, request, playerNaoAdmin))
                .Throws(new ValidationException("O jogador não é administrador."));

            // ACT
            Func<Task> act = async () => await _sut.RejectMembershipRequestTeam(teamId, requestId, playerNaoAdmin.Id);

            // ASSERT
            var exception = await act.Should().ThrowAsync<Exception>();

            exception.Which.Should().BeOfType<ValidationException>()
                    .Which.Message.Should().Be("O jogador não é administrador.");

            // VERIFY
            _membershipValidatorMock.Verify(v => v.ValidateRejectRequestByTeam(team, request, playerNaoAdmin), Times.Once);
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
            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true, Team = team };
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
            // CORREÇÃO: Passar admin.Id em vez do objeto admin
            var result = await _sut.GetMembershipRequestsByTeam(teamId, admin.Id);

            // ASSERT
            result.Should().HaveCount(2);
            result.Select(r => r.PlayerName).Should().Contain(new[] { "Jogador 1", "Jogador 2" });
        }

        [Test(Description = "Validação: GetMembershipRequestsAsync_Should_Throw_ValidationException_When_Not_Admin")]
        public async Task GetMembershipRequestsAsync_Should_Throw_ValidationException_When_Not_Admin()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerId = "player-nao-admin";
            var team = new Team { Id = teamId, Name = "Test Team" };

            var playerNaoAdmin = new Player
            {
                Id = playerId,
                IsAdmin = false,
                IdTeam = teamId
            };

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId))
                .ReturnsAsync(team);

            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId))
                .ReturnsAsync(playerNaoAdmin);

            _membershipValidatorMock
                .Setup(v => v.ValidateGetRequestsByTeam(team, playerNaoAdmin))
                .Throws(new ValidationException("O jogador não é administrador."));

            // ACT
            // CORREÇÃO: Passar playerNaoAdmin.Id
            Func<Task> act = async () => await _sut.GetMembershipRequestsByTeam(teamId, playerNaoAdmin.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*");

            _membershipRequestRepoMock.Verify(r => r.GetMembershipRequestsByTeam(It.IsAny<Guid>()), Times.Never);
        }

        [Test(Description = "Validação: GetMembershipRequestsAsync_Should_Throw_ValidationException_When_Player_Does_Not_Belong_To_Team")]
        public async Task GetMembershipRequestsAsync_Should_Throw_ValidationException_When_Player_Does_Not_Belong_To_Team()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerId = "player-outsider";
            var team = new Team { Id = teamId, Name = "Test Team" };
            var outsiderPlayer = new Player
            {
                Id = playerId,
                IsAdmin = true,
                IdTeam = Guid.NewGuid()
            };

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId))
                .ReturnsAsync(team);

            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId))
                .ReturnsAsync(outsiderPlayer);

            _membershipValidatorMock
                .Setup(v => v.ValidateGetRequestsByTeam(team, outsiderPlayer))
                .Throws(new ValidationException("O jogador não pertence a esta equipa."));

            // ACT
            // CORREÇÃO: Passar outsiderPlayer.Id
            Func<Task> act = async () => await _sut.GetMembershipRequestsByTeam(teamId, outsiderPlayer.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence*");

            _membershipRequestRepoMock.Verify(r => r.GetMembershipRequestsByTeam(It.IsAny<Guid>()), Times.Never);
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
            var admin = new Player { Id = adminId, IdTeam = teamId, IsAdmin = true, Team = team };
            team.Members.Add(admin);
            var playerToInvite = new Player { Id = playerIdToInvite, IdTeam = Guid.Empty, Team = null };
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
            var result = await _sut.SendMembershipRequestTeam(teamId, playerIdToInvite, admin.Id);

            // ASSERT
            result.Should().BeEquivalentTo(requestDto, options => options.Excluding(r => r.RequestDate).Excluding(r => r.RequestId));
            _membershipRequestRepoMock.Verify(r => r.AddMembershipRequest(It.IsAny<MembershipRequest>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "Validação: SendMembershipRequest_Should_Throw_ValidationException_When_Not_Admin")]
        public async Task SendMembershipRequest_Should_Throw_ValidationException_When_Not_Admin()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerIdToInvite = "player-to-invite";
            var playerNaoAdmin = new Player { Id = "player-nao-admin", IsAdmin = false };
            var team = new Team { Id = teamId };
            var playerToInvite = new Player { Id = playerIdToInvite };

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerIdToInvite)).ReturnsAsync(playerToInvite);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId))
                .ReturnsAsync((MembershipRequest)null);


            _membershipValidatorMock
                .Setup(v => v.ValidateSendRequestByTeam(team, playerToInvite, null, playerNaoAdmin))
                .Throws(new ValidationException("O jogador não é administrador."));

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequestTeam(teamId, playerIdToInvite, playerNaoAdmin.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*");

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Validação: SendMembershipRequest_Should_Throw_ValidationException_When_Team_Is_Full")]
        public async Task SendMembershipRequest_Should_Throw_ValidationException_When_Team_Is_Full()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerIdToInvite = "player-to-invite";
            var adminPlayer = new Player { Id = "admin", IsAdmin = true };
            var teamFull = new Team { Id = teamId, Members = new List<Player>() };

            for (int i = 0; i < ModelConstants.TeamConst.MaxMembers; i++)
            {
                teamFull.Members.Add(new Player());
            }

            var playerToInvite = new Player { Id = playerIdToInvite };

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(teamFull); // Devolve a equipa cheia
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerIdToInvite)).ReturnsAsync(playerToInvite);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId))
                .ReturnsAsync((MembershipRequest)null);

            _membershipValidatorMock
                .Setup(v => v.ValidateSendRequestByTeam(teamFull, playerToInvite, null, adminPlayer))
                .Throws(new ValidationException("A equipa está cheia."));

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequestTeam(teamId, playerIdToInvite, adminPlayer.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*equipa está cheia*");

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Validação: SendMembershipRequest_Should_Throw_ValidationException_When_Player_Belongs_Another_Team")]
        public async Task SendMembershipRequest_Should_Throw_ValidationException_When_Player_Belongs_Another_Team()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerIdToInvite = "player-com-equipa";
            var adminPlayer = new Player { Id = "admin", IsAdmin = true };

            var playerWithTeam = new Player
            {
                Id = playerIdToInvite,
                IdTeam = Guid.NewGuid()
            };

            var team = new Team { Id = teamId };

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerIdToInvite)).ReturnsAsync(playerWithTeam); // Devolve o jogador com equipa
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId))
                .ReturnsAsync((MembershipRequest)null);

            _membershipValidatorMock
                .Setup(v => v.ValidateSendRequestByTeam(team, playerWithTeam, null, adminPlayer))
                .Throws(new ValidationException("O jogador já pertence a outra equipa."));

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequestTeam(teamId, playerIdToInvite, adminPlayer.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*já pertence a outra equipa*");

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Validação: SendMembershipRequest_Should_Throw_ValidationException_When_ExistingRequest")]
        public async Task SendMembershipRequest_Should_Throw_ValidationException_When_ExistingRequest()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerIdToInvite = "player-to-invite";
            var adminPlayer = new Player { Id = "admin", IsAdmin = true };

            var team = new Team { Id = teamId };
            var playerToInvite = new Player { Id = playerIdToInvite };

            var existingRequest = new MembershipRequest
            {
                Id = Guid.NewGuid(),
                IdTeam = teamId,
                IdPlayer = playerIdToInvite
            };

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerIdToInvite)).ReturnsAsync(playerToInvite);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId))
                .ReturnsAsync(existingRequest);

            _membershipValidatorMock
                .Setup(v => v.ValidateSendRequestByTeam(team, playerToInvite, existingRequest, adminPlayer))
                .Throws(new ValidationException("Já existe um pedido pendente."));

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequestTeam(teamId, playerIdToInvite, adminPlayer.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*já existe um pedido*");

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Validação: SendMembershipRequest_Should_Throw_ValidationException_When_Player_Belongs_To_Team")]
        public async Task SendMembershipRequest_Should_Throw_ValidationException_When_Player_Belongs_To_Team()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var playerIdToInvite = "player-na-equipa";
            var adminPlayer = new Player { Id = "admin", IsAdmin = true };
            var team = new Team { Id = teamId };

            var playerInThisTeam = new Player
            {
                Id = playerIdToInvite,
                IdTeam = teamId
            };

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerIdToInvite)).ReturnsAsync(playerInThisTeam);
            _membershipRequestRepoMock.Setup(r => r.GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId))
                .ReturnsAsync((MembershipRequest)null);

            _membershipValidatorMock
                .Setup(v => v.ValidateSendRequestByTeam(team, playerInThisTeam, null, adminPlayer))
                .Throws(new ValidationException("O jogador já pertence a esta equipa."));

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequestTeam(teamId, playerIdToInvite, adminPlayer.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*já pertence a esta equipa*");

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

        [Test(Description = "Validação: GetMembershipRequests_ShouldThrow_WhilePlayerHasTeam")]
        public void GetMembershipRequests_ShouldThrow_WhilePlayerHasTeam()
        {
            // ARRANGE
            var playerId = "player-with-team";

            var playerWithTeam = new Player
            {
                Id = playerId,
                Team = new Team(),
                IdTeam = Guid.NewGuid()
            };

            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(playerWithTeam);

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.GetMembershipRequestsAsyncPlayer(playerId));
        }

        [Test(Description = "Validação: Lança exceção se o jogador pedido não existir")]
        public void GetMembershipRequests_ShouldThrow_When_PlayerNotFound()
        {
            // ARRANGE
            var playerId = "jogador-que-nao-existe";

            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId))
                           .ReturnsAsync((Player)null);
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.GetMembershipRequestsAsyncPlayer(playerId));

            // VERIFY
            _membershipRequestRepoMock.Verify(r => r.GetMembershipRequestsByPlayer(It.IsAny<string>()), Times.Never);
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
        public async Task SendMembershipRequestAsync_ShouldThrow_WhilePlayerHasTeam()
        {
            // ARRANGE
            var playerId = "player-with-team";
            var teamIdToJoin = Guid.NewGuid();

            var playerWithTeam = new Player
            {
                Id = playerId,
                Team = new Team(),
                IdTeam = Guid.NewGuid()
            };

            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(playerWithTeam);

            // ACT
            Func<Task> act = async () => await _sut.SendMembershipRequestAsyncPlayer(playerId, teamIdToJoin);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Apenas jogadores sem equipa podem aceder a este recurso!");
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
            // ARRANGE
            var playerId = "player-rejecting-1";
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var player = new Player
            {
                Id = playerId,
                Name = "John",
                Team = null, // O validador (real) vai verificar isto e passar
                MembershipRequests = new List<MembershipRequest>()
            };

            var team = new Team
            {
                Id = teamId,
                Name = "Team A",
                Members = new List<Player>() // Necessário para a notificação (linha 228)
            };

            var membershipRequest = new MembershipRequest
            {
                Id = requestId,
                IdPlayer = playerId,
                IdTeam = teamId,
                InviteDate = DateTime.UtcNow,
                IsPlayerSender = false,
                Player = player, // !! CORREÇÃO: Ligar o Player ao Request
                Team = team      // !! CORREÇÃO: Ligar a Team ao Request (para evitar NRE na linha 228)
            };

            player.MembershipRequests.Add(membershipRequest);

            // !! CORREÇÃO: O mock agora devolve o MembershipRequest
            _playerRepoMock.Setup(r => r.GetPlayerByIdWithRequestsAsync(playerId))
                .ReturnsAsync(membershipRequest);

            // Mocks para o DTO de retorno (linhas 223-224)
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId))
                .ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamByIdAsync(teamId))
                .ReturnsAsync(team);

            // !! CORREÇÃO: Mocks em falta para validação e notificação
            _membershipValidatorMock.Setup(v => v.ValidateRejectRequestByPlayer(membershipRequest, player));
            _notificationServiceMock.Setup(n => n.SendUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            var result = await _sut.RejectMembershipRequestAsyncPlayer(playerId, requestId);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.RequestId, Is.EqualTo(requestId));
            Assert.That(result.TeamName, Is.EqualTo("Team A"));
            // O seu serviço (linha 226) remove o pedido da lista em memória
            Assert.That(player.MembershipRequests, Is.Empty, "The membership request should be removed from the player's list.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "Validação: RejectMembershipRequest_Should_Throw_When_PlayerHasTeam")]
        public void RejectMembershipRequest_Should_Throw_When_PlayerHasTeam()
        {
            // ARRANGE
            var playerId = "player-rejecting-2-has-team";
            var requestId = Guid.NewGuid();

            var player = new Player
            {
                Id = playerId,
                Name = "PlayerWithTeam",
                Team = new Team { Id = Guid.NewGuid(), Name = "ExistingTeam" } // O validador real vai apanhar isto
            };

            // !! CORREÇÃO: O mock agora devolve um MembershipRequest que contém o player
            var membershipRequest = new MembershipRequest
            {
                Id = requestId,
                IdPlayer = playerId,
                Player = player // Ligar o player que tem a equipa
            };

            _playerRepoMock.Setup(r => r.GetPlayerByIdWithRequestsAsync(playerId))
                .ReturnsAsync(membershipRequest);

            // (Não precisamos de mais mocks, pois o validador real 'authorizationValidator'
            // deve falhar primeiro)

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.RejectMembershipRequestAsyncPlayer(playerId, requestId));
        }

        [Test(Description = "Caminho feliz: RejectMembershipRequest_Should_RemoveRequestFromList")]
        public async Task RejectMembershipRequest_Should_RemoveRequestFromList()
        {
            // ARRANGE
            var playerId = "player-rejecting-3";
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var player = new Player
            {
                Id = playerId,
                Name = "John",
                MembershipRequests = new List<MembershipRequest>()
            };

            var team = new Team
            {
                Id = teamId,
                Name = "Team A",
                Members = new List<Player>()
            };

            var membershipRequest = new MembershipRequest
            {
                Id = requestId,
                IdPlayer = playerId,
                IdTeam = teamId,
                InviteDate = DateTime.UtcNow,
                Player = player,
                Team = team
            };

            player.MembershipRequests.Add(membershipRequest);

            _playerRepoMock.Setup(r => r.GetPlayerByIdWithRequestsAsync(playerId))
                 .ReturnsAsync(membershipRequest);

            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId))
               .ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamByIdAsync(teamId))
               .ReturnsAsync(team);

            _membershipValidatorMock.Setup(v => v.ValidateRejectRequestByPlayer(membershipRequest, player));
            _notificationServiceMock.Setup(n => n.SendUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.RejectMembershipRequestAsyncPlayer(playerId, requestId);

            // ASSERT
            Assert.That(player.MembershipRequests.Count, Is.EqualTo(0), "The request should be removed after rejection.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        #endregion

        #region Test AcceptMembershipRequest
        [Test(Description = "Caminho feliz: AcceptMembershipRequest_Should_Work_When_PlayerWithoutTeam")]
        public async Task AcceptMembershipRequest_Should_Work_When_PlayerWithoutTeam()
        {
            // ARRANGE
            var playerId = "player-accepting-1";
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var team = new Team
            {
                Id = teamId,
                Name = "Team A",
                Members = new List<Player>(),
                MembershipRequests = new List<MembershipRequest>()
            };

            var player = new Player
            {
                Id = playerId,
                Name = "John",
                Team = null,
                MembershipRequests = new List<MembershipRequest>()
            };

            var membershipRequest = new MembershipRequest
            {
                Id = requestId,
                IdPlayer = playerId,
                IdTeam = teamId,
                InviteDate = DateTime.UtcNow,
                IsPlayerSender = false,
                Team = team,
                Player = player
            };

            player.MembershipRequests.Add(membershipRequest);
            team.MembershipRequests.Add(membershipRequest);

            _playerRepoMock.Setup(r => r.GetPlayerByIdWithRequestsAsync(playerId))
                .ReturnsAsync(membershipRequest);

            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamByIdAsync(teamId)).ReturnsAsync(team);

            _membershipValidatorMock.Setup(v => v.ValidateAcceptRequestByPlayer(player, membershipRequest, team));
            _notificationServiceMock.Setup(n => n.SendUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()
            )).Returns(Task.CompletedTask);

            _membershipRequestRepoMock.Setup(r => r.RemoveAllMemberShipRequestsOfPlayer(playerId))
                .Returns(Task.CompletedTask)
                .Callback(() => player.MembershipRequests.Clear());

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            var result = await _sut.AcceptMembershipRequestAsyncPlayer(playerId, requestId);

            // ASSERT
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
            // ARRANGE
            var playerId = "player-accepting-2-has-team";
            var requestId = Guid.NewGuid();
            var teamId = Guid.NewGuid();

            var teamFromRequest = new Team
            {
                Id = teamId,
                Name = "Team from Request",
                Members = new List<Player>()
            };

            var player = new Player
            {
                Id = playerId,
                Name = "PlayerWithTeam",
                Team = new Team { Id = Guid.NewGuid(), Name = "ExistingTeam" },
                MembershipRequests = new List<MembershipRequest>()
            };

            var requestToFind = new MembershipRequest
            {
                Id = requestId,
                IdPlayer = playerId,
                IdTeam = teamId,
                Team = teamFromRequest,
                Player = player
            };

            player.MembershipRequests.Add(requestToFind);

            _playerRepoMock.Setup(r => r.GetPlayerByIdWithRequestsAsync(playerId))
                .ReturnsAsync(requestToFind);

            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(teamFromRequest);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamByIdAsync(teamId)).ReturnsAsync(teamFromRequest);

            _membershipValidatorMock.Setup(v => v.ValidateAcceptRequestByPlayer(player, requestToFind, teamFromRequest));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.AcceptMembershipRequestAsyncPlayer(playerId, requestId));
        }

        [Test(Description = "Caminho feliz: AcceptMembershipRequest_Should_RemoveRequestFromLists")]
        public async Task AcceptMembershipRequest_Should_RemoveRequestFromLists()
        {
            // ARRANGE
            var playerId = "player-accepting-3";
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var team = new Team
            {
                Id = teamId,
                Name = "Team",
                Members = new List<Player>(),
                MembershipRequests = new List<MembershipRequest>()
            };

            var player = new Player
            {
                Id = playerId,
                Name = "Player",
                MembershipRequests = new List<MembershipRequest>()
            };

            var membershipRequest = new MembershipRequest
            {
                Id = requestId,
                IdPlayer = playerId,
                IdTeam = teamId,
                Team = team,
                Player = player
            };

            team.MembershipRequests.Add(membershipRequest);
            player.MembershipRequests.Add(membershipRequest);

            _playerRepoMock.Setup(r => r.GetPlayerByIdWithRequestsAsync(playerId))
                .ReturnsAsync(membershipRequest);

            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamByIdAsync(teamId)).ReturnsAsync(team);

            _membershipValidatorMock.Setup(v => v.ValidateAcceptRequestByPlayer(player, membershipRequest, team));
            _notificationServiceMock.Setup(n => n.SendUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()
            )).Returns(Task.CompletedTask);

            _membershipRequestRepoMock.Setup(r => r.RemoveAllMemberShipRequestsOfPlayer(playerId))
                .Returns(Task.CompletedTask)
                .Callback(() => player.MembershipRequests.Clear());

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.AcceptMembershipRequestAsyncPlayer(playerId, requestId);

            // ASSERT
            Assert.That(player.MembershipRequests, Is.Empty);
            Assert.That(team.MembershipRequests, Is.Empty);
        }

        [Test(Description = "BUG: Este teste deve falhar. O serviço não limpa os outros pedidos.")]
        public async Task AcceptMembershipRequest_Should_RemoveOtherRequests_When_JoiningTeam()
        {
            // ARRANGE
            var playerId = "player-accepting-4";
            var teamId1 = Guid.NewGuid();
            var teamId2 = Guid.NewGuid();
            var acceptedRequestId = Guid.NewGuid();

            var team1 = new Team { Id = teamId1, Name = "Team 1", Members = new List<Player>(), MembershipRequests = new List<MembershipRequest>() };
            var team2 = new Team { Id = teamId2, Name = "Team 2", Members = new List<Player>(), MembershipRequests = new List<MembershipRequest>() };

            var player = new Player
            {
                Id = playerId,
                Name = "Player",
                MembershipRequests = new List<MembershipRequest>()
            };

            var acceptedRequest = new MembershipRequest { Id = acceptedRequestId, IdPlayer = playerId, IdTeam = teamId1, Team = team1, Player = player };
            var otherRequest = new MembershipRequest { Id = Guid.NewGuid(), IdPlayer = playerId, IdTeam = teamId2, Team = team2, Player = player };

            team1.MembershipRequests.Add(acceptedRequest);
            team2.MembershipRequests.Add(otherRequest);
            player.MembershipRequests.Add(acceptedRequest);
            player.MembershipRequests.Add(otherRequest);

            _playerRepoMock.Setup(r => r.GetPlayerByIdWithRequestsAsync(playerId))
                .ReturnsAsync(acceptedRequest);

            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId1)).ReturnsAsync(team1);
            _teamRepoMock.Setup(r => r.GetTeamByIdAsync(teamId1)).ReturnsAsync(team1);
            _teamRepoMock.Setup(r => r.GetTeamByIdAsync(teamId2)).ReturnsAsync(team2);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));
            _membershipValidatorMock.Setup(v => v.ValidateAcceptRequestByPlayer(player, acceptedRequest, team1));
            _notificationServiceMock.Setup(n => n.SendUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()
            )).Returns(Task.CompletedTask);

            _membershipRequestRepoMock
                .Setup(r => r.RemoveAllMemberShipRequestsOfPlayer(playerId))
                .Returns(Task.CompletedTask)
                .Callback(() =>
                {
                    player.MembershipRequests.Clear();
                });

            // ACT
            await _sut.AcceptMembershipRequestAsyncPlayer(playerId, acceptedRequestId);

            // ASSERT
            Assert.That(player.MembershipRequests, Is.Empty, "All other membership requests should be cleared when joining a team.");
            Assert.That(player.IdTeam, Is.EqualTo(teamId1));
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _membershipRequestRepoMock.Verify(r => r.RemoveAllMemberShipRequestsOfPlayer(playerId), Times.Once);
        }
        #endregion

        #endregion

        #endregion
    }
}