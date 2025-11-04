using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class MembershipService : IMembershipRequestService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IMembershipRequestRepository _membershipRequestRepository;
        private readonly IUnityOfWork _unityOfWork;
        private readonly ITeamValidator _teamValidator;
        private readonly IPlayerValidator _playerValidator;

        public MembershipService(
            ITeamRepository teamRepository,
            IPlayerRepository playerRepository,
            IMembershipRequestRepository membershipRequestRepository,
            IUnityOfWork unityOfWork,
            ITeamValidator teamValidator,
            IPlayerValidator playerValidator)
        {
            _teamRepository = teamRepository;
            _playerRepository = playerRepository;
            _membershipRequestRepository = membershipRequestRepository;
            _unityOfWork = unityOfWork;
            _teamValidator = teamValidator;
            _playerValidator = playerValidator;
        }

        #region Pedidos de adesão da Team

        public Task<MemberShipRequestDto> SendMembershipRequest(Guid teamId, string playerIdToInvite, string adminUserId)
        {
            return _teamRepository.GetTeamForMemberManagementAsync(teamId).ContinueWith(async teamTask =>
            {
                var team = await teamTask;
                var admin = await _playerRepository.GetPlayerByIdAsync(adminUserId);
                var playerToInvite = await _playerRepository.GetPlayerByIdAsync(playerIdToInvite);
                var existing = await _membershipRequestRepository.GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId);

                _teamValidator.SendMembershipRequestValidation(existing, team, admin, playerToInvite);
                _playerValidator.PlayerExists(playerToInvite);

                var invite = new MembershipRequest
                {
                    Id = Guid.NewGuid(),
                    IdPlayer = playerIdToInvite,
                    IdTeam = teamId,
                    InviteDate = DateTime.UtcNow,
                    IsPlayerSender = false
                };

                await _membershipRequestRepository.AddMembershipRequest(invite);
                await _unityOfWork.SaveChangesAsync();

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
            }).Unwrap();
        }

        public async Task AcceptMembershipRequest(Guid teamId, Guid requestId, string adminUserId)
        {
            var request = await _membershipRequestRepository.GetMembershipRequestById(requestId);
            if (request == null)
                throw new ValidationException($"O pedido de adesão com Id '{requestId}' não existe.");

            var team = await _teamRepository.GetTeamForMembershipRequestAsync(teamId);
            if (team == null)
                throw new ValidationException($"A equipa com Id '{teamId}' não existe.");

            var admin = await _playerRepository.GetPlayerByIdAsync(adminUserId);

            _teamValidator.ApproveMembershipRequestValidation(team, admin, requestId);

            var playerAccepted = await _playerRepository.GetPlayerByIdAsync(request.IdPlayer);
            if (playerAccepted == null)
                throw new ValidationException($"O jogador com Id '{request.IdPlayer}' não existe.");

            _membershipRequestRepository.RemoveMembershipRequest(request);

            playerAccepted.IdTeam = teamId;
            team.Members.Add(playerAccepted);

            await _unityOfWork.SaveChangesAsync();
        }

        public Task RejectMembershipRequest(Guid teamId, Guid requestId, string adminUserId)
        {
            return _membershipRequestRepository.GetMembershipRequestById(requestId).ContinueWith(async requestTask =>
            {
                var request = await requestTask;
                if (request == null)
                    throw new ValidationException($"O pedido de adesão com Id '{requestId}' não existe.");

                var team = await _teamRepository.GetTeamForMembershipRequestAsync(teamId);
                var admin = await _playerRepository.GetPlayerByIdAsync(adminUserId);

                if (team == null)
                    throw new ValidationException($"A equipa com Id '{teamId}' não existe.");

                _teamValidator.RejectMembershipRequestValidation(team, admin, requestId);

                _membershipRequestRepository.RemoveMembershipRequest(request);
                await _unityOfWork.SaveChangesAsync();
            }).Unwrap();
        }

        public Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId, string adminUserId)
        {
            return _teamRepository.GetTeamForMemberManagementAsync(teamId).ContinueWith(async teamTask =>
            {
                var team = await teamTask;
                var admin = await _playerRepository.GetPlayerByIdAsync(adminUserId);

                if (team == null)
                    throw new ValidationException($"A equipa com Id '{teamId}' não existe.");

                _teamValidator.GetMembershipRequestsValidation(team, admin);

                var list = await _membershipRequestRepository.GetMembershipRequestsByTeam(teamId);
                return list ?? new List<MemberShipRequestDto>();
            }).Unwrap();
        }

        public Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId, string adminUserId, FilterMembershipRequestsTeam filters)
        {
            return _teamRepository.GetTeamForMemberManagementAsync(teamId).ContinueWith(async teamTask =>
            {
                var team = await teamTask;
                var admin = await _playerRepository.GetPlayerByIdAsync(adminUserId);

                if (team == null)
                    throw new ValidationException($"A equipa com Id '{teamId}' não existe.");

                _teamValidator.GetMembershipRequestsValidation(team, admin);

                var list = await _membershipRequestRepository.GetMembershipRequestsByTeamWithFilters(teamId, filters);
                return list ?? new List<MemberShipRequestDto>();
            }).Unwrap();
        }

        #endregion

        #region Pedidos de adesão do Player

        public Task<List<MemberShipRequestDto>> GetMembershipRequestsByPlayer(string playerId)
        {
            return _membershipRequestRepository.GetMembershipRequestsByPlayer(playerId);
        }

        public Task<List<MemberShipRequestDto>> GetMembershipRequestsByPlayer(string playerId, FilterMembershipRequestsPlayer filters)
        {
            return _membershipRequestRepository.GetMembershipRequestsByPlayerWithFilters(playerId, filters);
        }

        public Task<MemberShipRequestDto> PlayerSendMembershipRequest(string playerId, Guid teamId)
        {
            return _playerRepository.GetPlayerByIdAsync(playerId).ContinueWith(async playerTask =>
            {
                var player = await playerTask;
                var team = await _teamRepository.GetTeamForMembershipRequestAsync(teamId);
                var existing = await _membershipRequestRepository.GetMembershipRequestByPlayerAndTeam(playerId, teamId);

                _playerValidator.SendMembershipRequestValidator(player, team, existing);

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

                await _membershipRequestRepository.AddMembershipRequest(newRequest);
                await _unityOfWork.SaveChangesAsync();

                return new MemberShipRequestDto
                {
                    RequestId = newRequest.Id,
                    PlayerId = newRequest.IdPlayer,
                    PlayerName = player.Name,
                    TeamId = newRequest.IdTeam,
                    TeamName = team.Name,
                    RequestDate = newRequest.InviteDate,
                    IsPlayerSender = newRequest.IsPlayerSender
                };
            }).Unwrap();
        }

        #endregion
    }
}