using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using System.Globalization;
using System.Security.Claims;

namespace Application.Services
{
    public class MembershipService : IMembershipRequestService
    {
        private readonly ITeamRepository teamRepository;
        private readonly IPlayerRepository playerRepository;
        private readonly IMembershipRequestRepository membershipRequestRepository;
        private readonly IUnityOfWork unityOfWork;
        private readonly IPlayerValidator playerValidator;
        private readonly IPlayerAuthorizationValidator authorizationValidator;
        private readonly IMembershipValidator membershipValidator;

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
            this.playerValidator = playerValidator;
            this.authorizationValidator = authorizationValidator;
            this.membershipValidator = membershipValidator;
            this.notificationService = notificationService;
        }

        #region Pedidos de adesão da Team

        public async Task<MemberShipRequestDto> SendMembershipRequestTeam(Guid teamId, string playerIdToInvite, string sender)
        {
            var team = await teamRepository.GetTeamForMemberManagementAsync(teamId);
            var playerToInvite = await playerRepository.GetPlayerByIdAsync(playerIdToInvite);
            var player = await playerRepository.GetPlayerByIdAsync(sender);
            var existing = await membershipRequestRepository.GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId);

            membershipValidator.ValidateSendRequestByTeam(team, playerToInvite, existing, player);

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
            await notificationService.SendUserAsync(playerIdToInvite, "New Team Invitation!", $"You've been invited to join the team {team.Name}!");

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

        public async Task AcceptMembershipRequestTeam(Guid teamId, Guid requestId, string adminId)
        {
            var request = await membershipRequestRepository.GetMembershipRequestById(requestId);
            var team = await teamRepository.GetTeamForMembershipRequestAsync(teamId);
            var playerAccepting = await playerRepository.GetPlayerByIdAsync(adminId);

            membershipValidator.ValidateAcceptRequestByTeam(team, request, playerAccepting);

            var playerAccepted = await playerRepository.GetPlayerByIdAsync(request.IdPlayer);

            membershipRequestRepository.RemoveMembershipRequest(request);

            playerAccepted.IdTeam = teamId;
            team.Members.Add(playerAccepted);

            await membershipRequestRepository.RemoveAllMemberShipRequestsOfPlayer(playerAccepted.Id);
            await RemoveAllMatchInviteTeam(team);
            await unityOfWork.SaveChangesAsync();
            await notificationService.SendUserAsync(request.IdPlayer, "Membership request Accepted!", $"Your request to join the team {team.Name} has been accepted!");
        }

        public Task RejectMembershipRequestTeam(Guid teamId, Guid requestId, string adminId)
        {
            return membershipRequestRepository.GetMembershipRequestById(requestId).ContinueWith(async requestTask =>
            {
                var request = await requestTask;
                var team = await teamRepository.GetTeamForMembershipRequestAsync(teamId);
                var playerAccepting = await playerRepository.GetPlayerByIdAsync(adminId);

                membershipValidator.ValidateRejectRequestByTeam(team, request, playerAccepting);

                var playerAccepted = await playerRepository.GetPlayerByIdAsync(request.IdPlayer);

                membershipRequestRepository.RemoveMembershipRequest(request);

                await unityOfWork.SaveChangesAsync();
                await notificationService.SendUserAsync(request.IdPlayer, "Membership request rejected.", $"Your request to join the team {team.Name} has been rejected.");
            }).Unwrap();
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId, string playerId)
        {
            var team = await teamRepository.GetTeamForMemberManagementAsync(teamId);

            var player = await playerRepository.GetPlayerByIdAsync(playerId);

            membershipValidator.ValidateGetRequestsByTeam(team, player);

            return await membershipRequestRepository.GetMembershipRequestsByTeam(teamId);
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeamWithFilters(Guid teamId, FilterMembershipRequestsTeam filters, string playerId)
        {
            var team = await teamRepository.GetTeamForMemberManagementAsync(teamId);

            var player = await playerRepository.GetPlayerByIdAsync(playerId);

            membershipValidator.ValidateGetRequestsByTeam(team, player);

            return await membershipRequestRepository.GetMembershipRequestsByTeamWithFilters(teamId, filters);
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
            var membershipRequest = await membershipRequestRepository.GetMembershipRequestById(requestId);

            if (membershipRequest == null)
            {
                throw new ValidationException("O pedido de adesão não foi encontrado.");
            }

            var player = membershipRequest.Player;

            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);

            var team = await teamRepository.GetTeamForMembershipRequestAsync(membershipRequest.IdTeam);

            membershipValidator.ValidateAcceptRequestByPlayer(player, membershipRequest, team);

            player.IdTeam = team.Id;
            team.Members.Add(player);

            player.MembershipRequests?.Remove(membershipRequest);
            team.MembershipRequests?.Remove(membershipRequest);

            await membershipRequestRepository.RemoveAllMemberShipRequestsOfPlayer(player.Id);

            await RemoveAllMatchInviteTeam(team);

            await unityOfWork.SaveChangesAsync();

            var fullPlayer = await playerRepository.GetPlayerByIdAsync(membershipRequest.IdPlayer);
            var fullTeam = await teamRepository.GetTeamByIdAsync(membershipRequest.IdTeam);

            var teamAdmins = team.Members.Where(p => p.IsAdmin).ToList();

            foreach (var admin in teamAdmins)
            {
                await notificationService.SendUserAsync(admin.Id, "Membership Invite Accepted", $"{player.Name} accepted your membership invite and is now part of the team!");
            }

            return new MemberShipRequestDto
            {
                RequestId = membershipRequest.Id,
                PlayerId = fullPlayer.Id,
                PlayerName = fullPlayer.Name,
                TeamId = fullTeam.Id,
                TeamName = fullTeam.Name,
                RequestDate = membershipRequest.InviteDate,
                IsPlayerSender = membershipRequest.IsPlayerSender
            };
        }

        public Task<MemberShipRequestDto> RejectMembershipRequestAsyncPlayer(string playerId, Guid requestId)
        {
            return membershipRequestRepository.GetMembershipRequestById(requestId).ContinueWith(async requestTask =>
            {
                var request = await requestTask;
                var player = await playerRepository.GetPlayerByIdAsync(playerId);

                authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);
                membershipValidator.ValidateRejectRequestByPlayer(request, player);

                player.MembershipRequests?.Remove(request);
                await unityOfWork.SaveChangesAsync();

                if (request?.Team != null)
                {
                    var teamAdmins = request.Team.Members.Where(p => p.IsAdmin).ToList();

                    foreach (var admin in teamAdmins)
                    {
                        await notificationService.SendUserAsync(admin.Id, "Membership Invite Rejected", $"{player.Name} rejected your membership invite.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("A equipa associada ao pedido de adesão não foi encontrada.");
                }

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
            }).Unwrap();
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
            
            var teamAdmins = team.Members
                .Where(p => p.IsAdmin == true)
                .ToList();

            foreach (var admin in teamAdmins)
            {
                await notificationService.SendUserAsync(admin.Id, "New Membership Request", $"Your team received a new membership request from {player.Name}.");
            }

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

        #region Private Methods
        private async Task RemoveAllMatchInviteTeam(Team team)
        {
            if (team.Members.Count >= ModelConstants.TeamConst.MaxMembers)
            {
                await membershipRequestRepository.RemoveAllMemberShipRequestsOfTeam(team.Id);
            }
        }
        #endregion
    }

} 
