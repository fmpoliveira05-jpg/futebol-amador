using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators;
using Application.Validators;
using Domain.Entities;
using Domain.Exceptions;
using FirebaseAdmin.Messaging;

namespace Application.Services
{
    public class MembershipService : IMembershipRequestService
    {
        private readonly ITeamRepository teamRepository;
        private readonly IPlayerRepository playerRepository;
        private readonly IMembershipRequestRepository membershipRequestRepository;
        private readonly IUnityOfWork unityOfWork;
        private readonly ITeamValidator teamValidator;
        private readonly IPlayerValidator playerValidator;
        private readonly IPlayerAuthorizationValidator authorizationValidator;
        private readonly IMembershipValidator membershipValidator;
        private ITeamRepository object1;
        private IPlayerRepository object2;
        private IMembershipRequestRepository object3;
        private IUnityOfWork object4;
        private PlayerAuthorizationValidator authorizationValidator1;

        private readonly INotificationService notificationService;

        public MembershipService(
            ITeamRepository teamRepository,
            IPlayerRepository playerRepository,
            IMembershipRequestRepository membershipRequestRepository,
            IUnityOfWork unityOfWork,
            ITeamValidator teamValidator,
            IPlayerValidator playerValidator,
            IPlayerAuthorizationValidator authorizationValidator,
            IMembershipValidator membershipValidator,
            INotificationService notificationService)
        {
            this.teamRepository = teamRepository;
            this.playerRepository = playerRepository;
            this.membershipRequestRepository = membershipRequestRepository;
            this.unityOfWork = unityOfWork;
            this.teamValidator = teamValidator;
            this.playerValidator = playerValidator;
            this.authorizationValidator = authorizationValidator;
            this.membershipValidator = membershipValidator;
            this.notificationService = notificationService;
        }

        public MembershipService(ITeamRepository object1, IPlayerRepository object2, IMembershipRequestRepository object3, IUnityOfWork object4, TeamValidator teamValidator, PlayerValidator playerValidator, PlayerAuthorizationValidator authorizationValidator1)
        {
            this.object1 = object1;
            this.object2 = object2;
            this.object3 = object3;
            this.object4 = object4;
            this.teamValidator = teamValidator;
            this.playerValidator = playerValidator;
            this.authorizationValidator1 = authorizationValidator1;
        }

        #region Pedidos de adesão da Team

        public async Task<MemberShipRequestDto> SendMembershipRequestTeam(Guid teamId, string playerIdToInvite)
        {
            var team = await teamRepository.GetTeamForMemberManagementAsync(teamId);
            var playerToInvite = await playerRepository.GetPlayerByIdAsync(playerIdToInvite);
            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(playerToInvite);

            var existing = await membershipRequestRepository.GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId);

            membershipValidator.ValidateSendRequestByTeam(team, playerToInvite, existing);

            var invite = new MembershipRequest
            {
                Id = Guid.NewGuid(),
                IdPlayer = playerIdToInvite,
                IdTeam = teamId,
                InviteDate = DateTime.UtcNow,
                IsPlayerSender = false
            };

            await notificationService.SendUserAsync(playerIdToInvite, "New Team Invitation!", $"You've been invited to join the team {team.Name}!");

            await membershipRequestRepository.AddMembershipRequest(invite);
            await unityOfWork.SaveChangesAsync();

            return new MemberShipRequestDto
            {
                RequestId = invite.Id,
                PlayerId = invite.IdPlayer,
                PlayerName = playerToInvite.Name,
                TeamId = invite.IdTeam,
                TeamName = team.Name,
                RequestDate = invite.InviteDate,
                IsPlayerSender = invite.IsPlayerSender
            };
        }

        public async Task AcceptMembershipRequestTeam(Guid teamId, Guid requestId)
        {
            var request = await membershipRequestRepository.GetMembershipRequestById(requestId);

            var team = await teamRepository.GetTeamForMembershipRequestAsync(teamId);

            var playerAccepted = await playerRepository.GetPlayerByIdAsync(request.IdPlayer);

            membershipValidator.ValidateAcceptRequestByTeam(team, request);

            membershipRequestRepository.RemoveMembershipRequest(request);

            playerAccepted.IdTeam = teamId;
            team.Members.Add(playerAccepted);

            await notificationService.SendUserAsync(request.IdPlayer, "Membership request Accepted!", $"Your request to join the team {team.Name} has been accepted!");

            await unityOfWork.SaveChangesAsync();
        }

        public Task RejectMembershipRequestTeam(Guid teamId, Guid requestId)
        {
            return membershipRequestRepository.GetMembershipRequestById(requestId).ContinueWith(async requestTask =>
            {
                var request = await requestTask;

                var team = await teamRepository.GetTeamForMembershipRequestAsync(teamId);

                membershipValidator.ValidateRejectRequestByTeam(team, request);

                membershipRequestRepository.RemoveMembershipRequest(request);

                await notificationService.SendUserAsync(request.IdPlayer, "Membership request rejected.", $"Your request to join the team {team.Name} has been rejected.");

                await unityOfWork.SaveChangesAsync();
            }).Unwrap();
        }

        public Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId)
        {
            return teamRepository.GetTeamForMemberManagementAsync(teamId).ContinueWith(async teamTask =>
            {
                var team = await teamTask;

                membershipValidator.ValidateGetRequestsByTeam(team);

                var list = await membershipRequestRepository.GetMembershipRequestsByTeam(teamId);
                return list ?? new List<MemberShipRequestDto>();
            }).Unwrap();
        }

        public Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeamWithFilters(Guid teamId, FilterMembershipRequestsTeam filters)
        {
            return teamRepository.GetTeamForMemberManagementAsync(teamId).ContinueWith(async teamTask =>
            {
                var team = await teamTask;

                membershipValidator.ValidateGetRequestsByTeam(team);

                var list = await membershipRequestRepository.GetMembershipRequestsByTeamWithFilters(teamId, filters);
                return list ?? new List<MemberShipRequestDto>();
            }).Unwrap();
        }

        #endregion

        #region Pedidos de adesão do Player

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncPlayer(string playerId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);

            return await membershipRequestRepository.GetMembershipRequestsByPlayer(playerId);
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncPlayerWithFilters(string playerId, FilterMembershipRequestsPlayer filters)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);

            return await membershipRequestRepository.GetMembershipRequestsByPlayerWithFilters(playerId, filters);
        }

        public async Task<MemberShipRequestDto> AcceptMembershipRequestAsyncPlayer(string playerId, Guid requestId)
        {
            var player = await playerRepository.GetPlayerByIdWithRequestsAsync(playerId);

            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);

            var request = player.MembershipRequests?.FirstOrDefault(r => r.Id == requestId);

            var team = await teamRepository.GetTeamForMembershipRequestAsync(request.IdTeam);

            membershipValidator.ValidateAcceptRequestByPlayer(player, request, team);

            player.IdTeam = team.Id;
            team.Members.Add(player);

            var teamAdmins = request.Team.Members
                            .Where(p => p.IsAdmin == true)
                            .ToList();

            foreach (var admin in teamAdmins)
            {
                await notificationService.SendUserAsync(admin.Id, "Membership Invite Accepted", $"{player.Name} accepted your membership invite and is now part of the team!.");
            }

            player.MembershipRequests.Remove(request);
            team.MembershipRequests?.Remove(request);

            await unityOfWork.SaveChangesAsync();

            var fullPlayer = await playerRepository.GetPlayerByIdAsync(request.IdPlayer);
            var fullTeam = await teamRepository.GetTeamByIdAsync(request.IdTeam);

            return new MemberShipRequestDto
            {
                RequestId = request.Id,
                PlayerId = fullPlayer.Id,
                PlayerName = fullPlayer.Name,
                TeamId = fullTeam.Id,
                TeamName = fullTeam.Name,
                RequestDate = request.InviteDate,
                IsPlayerSender = request.IsPlayerSender
            };
        }

        public async Task<MemberShipRequestDto> RejectMembershipRequestAsyncPlayer(string playerId, Guid requestId)
        {
            var player = await playerRepository.GetPlayerByIdWithRequestsAsync(playerId);

            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);

            var request = player.MembershipRequests?.FirstOrDefault(r => r.Id == requestId);

            membershipValidator.ValidateRejectRequestByPlayer(request, player);

            var fullPlayer = await playerRepository.GetPlayerByIdAsync(request.IdPlayer);
            var fullTeam = await teamRepository.GetTeamByIdAsync(request.IdTeam);

            player.MembershipRequests.Remove(request);

            var teamAdmins = request.Team.Members
                            .Where(p => p.IsAdmin == true)
                            .ToList();

            foreach (var admin in teamAdmins)
            {
                await notificationService.SendUserAsync(admin.Id, "Membership Invite Rejected", $"{player.Name} rejected your membership invite.");
            }

            await unityOfWork.SaveChangesAsync();

            return new MemberShipRequestDto
            {
                RequestId = request.Id,
                PlayerId = fullPlayer.Id,
                PlayerName = fullPlayer.Name,
                TeamId = fullTeam.Id,
                TeamName = fullTeam.Name,
                RequestDate = request.InviteDate,
                IsPlayerSender = request.IsPlayerSender
            };
        }

        public async Task<MemberShipRequestDto> SendMembershipRequestAsyncPlayer(string playerId, Guid teamId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);

            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);

            var team = await teamRepository.GetTeamForMembershipRequestAsync(teamId);

            var existingRequest = await membershipRequestRepository
                .GetMembershipRequestByPlayerAndTeam(playerId, teamId);

            playerValidator.SendMembershipRequestValidator(player, team, existingRequest);

            var newRequest = new MembershipRequest
            {
                Id = Guid.NewGuid(),
                IdPlayer = playerId,
                IdTeam = teamId,
                Player = player,
                Team = team,
                InviteDate = DateTime.UtcNow,
                IsPlayerSender = true
            };

            var teamAdmins = team.Members
                            .Where(p => p.IsAdmin == true)
                            .ToList();

            foreach (var admin in teamAdmins)
            {
                await notificationService.SendUserAsync(admin.Id, "New Membership Request", $"Your team received a new membership request from {player.Name}.");
            }

            await membershipRequestRepository.AddMembershipRequest(newRequest);
            await unityOfWork.SaveChangesAsync();

            return new MemberShipRequestDto
            {
                RequestId = newRequest.Id,
                PlayerId = newRequest.IdPlayer,
                PlayerName = newRequest.Player.Name,
                TeamId = newRequest.IdTeam,
                TeamName = newRequest.Team.Name,
                RequestDate = newRequest.InviteDate,
                IsPlayerSender = newRequest.IsPlayerSender
            };
        }

        #endregion
    }
} 
