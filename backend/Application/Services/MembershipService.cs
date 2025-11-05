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
        public MembershipService(
            ITeamRepository teamRepository,
            IPlayerRepository playerRepository,
            IMembershipRequestRepository membershipRequestRepository,
            IUnityOfWork unityOfWork,
            ITeamValidator teamValidator,
            IPlayerValidator playerValidator,
            IPlayerAuthorizationValidator authorizationValidator)
        {
            teamRepository = teamRepository;
            playerRepository = playerRepository;
            membershipRequestRepository = membershipRequestRepository;
            unityOfWork = unityOfWork;
            teamValidator = teamValidator;
            playerValidator = playerValidator;
            this.authorizationValidator = authorizationValidator;

        }

        #region Pedidos de adesão da Team

        public async Task<MemberShipRequestDto> SendMembershipRequest(Guid teamId, string playerIdToInvite, string adminUserId)
        {
            var team = await teamRepository.GetTeamForMemberManagementAsync(teamId);
            var admin = await playerRepository.GetPlayerByIdAsync(adminUserId);
            var playerToInvite = await playerRepository.GetPlayerByIdAsync(playerIdToInvite);
            var existing = await membershipRequestRepository.GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId);

            teamValidator.SendMembershipRequestValidation(existing, team, admin, playerToInvite);
            playerValidator.PlayerExists(playerToInvite);

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

        public async Task AcceptMembershipRequest(Guid teamId, Guid requestId, string adminUserId)
        {
            var request = await membershipRequestRepository.GetMembershipRequestById(requestId);
            if (request == null)
                throw new ValidationException($"O pedido de adesão com Id '{requestId}' não existe.");

            var team = await teamRepository.GetTeamForMembershipRequestAsync(teamId);
            if (team == null)
                throw new ValidationException($"A equipa com Id '{teamId}' não existe.");

            var admin = await playerRepository.GetPlayerByIdAsync(adminUserId);

            teamValidator.ApproveMembershipRequestValidation(team, admin, requestId);

            var playerAccepted = await playerRepository.GetPlayerByIdAsync(request.IdPlayer);
            if (playerAccepted == null)
                throw new ValidationException($"O jogador com Id '{request.IdPlayer}' não existe.");

            membershipRequestRepository.RemoveMembershipRequest(request);

            playerAccepted.IdTeam = teamId;
            team.Members.Add(playerAccepted);

            await unityOfWork.SaveChangesAsync();
        }

        public Task RejectMembershipRequest(Guid teamId, Guid requestId, string adminUserId)
        {
            return membershipRequestRepository.GetMembershipRequestById(requestId).ContinueWith(async requestTask =>
            {
                var request = await requestTask;
                if (request == null)
                    throw new ValidationException($"O pedido de adesão com Id '{requestId}' não existe.");

                var team = await teamRepository.GetTeamForMembershipRequestAsync(teamId);
                var admin = await playerRepository.GetPlayerByIdAsync(adminUserId);

                if (team == null)
                    throw new ValidationException($"A equipa com Id '{teamId}' não existe.");

                teamValidator.RejectMembershipRequestValidation(team, admin, requestId);

                membershipRequestRepository.RemoveMembershipRequest(request);
                await unityOfWork.SaveChangesAsync();
            }).Unwrap();
        }

        public Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId, string adminUserId)
        {
            return teamRepository.GetTeamForMemberManagementAsync(teamId).ContinueWith(async teamTask =>
            {
                var team = await teamTask;
                var admin = await playerRepository.GetPlayerByIdAsync(adminUserId);

                if (team == null)
                    throw new ValidationException($"A equipa com Id '{teamId}' não existe.");

                teamValidator.GetMembershipRequestsValidation(team, admin);

                var list = await membershipRequestRepository.GetMembershipRequestsByTeam(teamId);
                return list ?? new List<MemberShipRequestDto>();
            }).Unwrap();
        }

        public Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId, string adminUserId, FilterMembershipRequestsTeam filters)
        {
            return teamRepository.GetTeamForMemberManagementAsync(teamId).ContinueWith(async teamTask =>
            {
                var team = await teamTask;
                var admin = await playerRepository.GetPlayerByIdAsync(adminUserId);

                if (team == null)
                    throw new ValidationException($"A equipa com Id '{teamId}' não existe.");

                teamValidator.GetMembershipRequestsValidation(team, admin);

                var list = await membershipRequestRepository.GetMembershipRequestsByTeamWithFilters(teamId, filters);
                return list ?? new List<MemberShipRequestDto>();
            }).Unwrap();
        }

        #endregion

        #region Pedidos de adesão do Player

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(string playerId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);

            return await membershipRequestRepository.GetMembershipRequestsByPlayer(playerId);
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncWithFilters(string playerId, FilterMembershipRequestsPlayer filters)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);

            return await membershipRequestRepository.GetMembershipRequestsByPlayerWithFilters(playerId, filters);
        }

        public async Task<MemberShipRequestDto> AcceptMembershipRequestAsync(string playerId, Guid requestId)
        {
            var player = await playerRepository.GetPlayerByIdWithRequestsAsync(playerId)
                ?? throw new ValidationException($"O jogador com Id '{playerId}' não existe.");

            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);

            var request = player.MembershipRequests?.FirstOrDefault(r => r.Id == requestId)
                ?? throw new ValidationException($"O jogador não possui um pedido de adesão com Id '{requestId}'.");

            var team = await teamRepository.GetTeamForMembershipRequestAsync(request.IdTeam)
                ?? throw new ValidationException($"A equipa com Id '{request.IdTeam}' não existe.");

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

        public async Task<MemberShipRequestDto> RejectMembershipRequestAsync(string playerId, Guid requestId)
        {
            var player = await playerRepository.GetPlayerByIdWithRequestsAsync(playerId)
                ?? throw new ValidationException($"O jogador com Id '{playerId}' não existe.");

            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);

            var request = player.MembershipRequests?.FirstOrDefault(r => r.Id == requestId)
                ?? throw new ValidationException($"O jogador não possui um pedido de adesão com Id '{requestId}'.");

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

        //faz validation de se o player ja tem equipa
        public async Task<MemberShipRequestDto> SendMembershipRequestAsync(string playerId, Guid teamId)
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