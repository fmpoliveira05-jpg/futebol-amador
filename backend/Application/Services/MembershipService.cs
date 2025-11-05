using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Application.Validators;
using Domain.Entities;
using Domain.Exceptions;

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

        public MembershipService(
            ITeamRepository teamRepository,
            IPlayerRepository playerRepository,
            IMembershipRequestRepository membershipRequestRepository,
            IUnityOfWork unityOfWork,
            ITeamValidator teamValidator,
            IPlayerValidator playerValidator,
            IPlayerAuthorizationValidator authorizationValidator,
            IMembershipValidator membershipValidator)
        {
            this.teamRepository = teamRepository;
            this.playerRepository = playerRepository;
            this.membershipRequestRepository = membershipRequestRepository;
            this.unityOfWork = unityOfWork;
            this.teamValidator = teamValidator;
            this.playerValidator = playerValidator;
            this.authorizationValidator = authorizationValidator;
            this.membershipValidator = membershipValidator;

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
