using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class TeamService : ITeamService
    {
        #region Initialize
        private readonly ITeamRepository TeamRepository;
        private readonly IPlayerRepository PlayerRepository;
        private readonly IUnityOfWork UnityOfWork;
        private readonly IRankRepository RankRepository;
        private readonly ITeamValidator TeamValidator;
        private readonly IMembershipRequestRepository MembershipRequestRepository;
        private readonly IPlayerValidator PlayerValidator;
        private readonly IPlayerAuthorizationValidator AuthorizationValidator;
        
        public TeamService(
            ITeamRepository teamRepository,
            IPlayerRepository playerRepository,
            IUnityOfWork unityOfWork,
            ITeamValidator teamValidator,
            IRankRepository rankRepository,
            IMembershipRequestRepository membershipRequestRepository,
            IPlayerValidator playerValidator,
            IPlayerAuthorizationValidator authorizationValidator)
        {
            this.TeamRepository = teamRepository;
            this.PlayerRepository = playerRepository;
            this.UnityOfWork = unityOfWork;
            this.TeamValidator = teamValidator;
            this.RankRepository = rankRepository;
            this.MembershipRequestRepository = membershipRequestRepository;
            this.PlayerValidator = playerValidator;
            this.AuthorizationValidator = authorizationValidator;
        }
        #endregion

        #region CRUD Team
        public async Task<Guid> CreateTeamAsync(CreateTeamDto teamDto, string playerId)
        {
            var playerCreating = await PlayerRepository.GetPlayerByIdAsync(playerId);
            AuthorizationValidator.ValidatePlayerAutorizationWithoutTeam(playerCreating);

            var existingTeam = await TeamRepository.GetTeamByNameAsync(teamDto.Name);
            var rank = await RankRepository.GetDefaultRankAsync();

            TeamValidator.CreateTeamValidation(teamDto, rank, existingTeam, playerCreating);

            var newTeam = new Team(
                teamDto.Name,
                teamDto.Description,
                teamDto.icon,
                new Pitch(teamDto.HomePitch.Name, teamDto.HomePitch.Address),
                rank
            );

            playerCreating.IdTeam = newTeam.Id;
            playerCreating.IsAdmin = true;
            playerCreating.IsAdminLastChangedAt = DateTime.UtcNow;

            await TeamRepository.AddAsync(newTeam);

            playerCreating.IsAdmin = true;
            playerCreating.IdTeam = newTeam.Id;
            playerCreating.Team = newTeam;
            newTeam.Members ??= new List<Player>();
            newTeam.Members.Add(playerCreating);

            PlayerRepository.UpdatePlayer(playerCreating);

            await UnityOfWork.SaveChangesAsync();
            return newTeam.Id;
        }

        public async Task DeleteTeamAsync(Guid teamId, string currentUserId)
        {
            var playerTryingToDelete = await PlayerRepository.GetPlayerByIdAsync(currentUserId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerTryingToDelete, teamId);

            var teamToDelete = await TeamRepository.GetTeamForDeletionAsync(teamId);

            TeamValidator.DeleteTeamValidation(teamToDelete);
            foreach (var member in teamToDelete.Members)
            {
                member.IdTeam = null;
                if (member.IsAdmin)
                {
                    member.IsAdmin = false;
                    member.IsAdminLastChangedAt = DateTime.UtcNow;
                }
            }
            TeamRepository.DeleteTeam(teamToDelete);

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task UpdateTeamInfoAsync(Guid teamId, UpdateTeamDto dto, string currentUserId)
        {
            var playerTryingToUpdate = await PlayerRepository.GetPlayerByIdAsync(currentUserId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerTryingToUpdate, teamId);

            var teamToUpdate = await TeamRepository.GetTeamForUpdateAsync(teamId);

            Team teamWithSameName = null;

            if (dto.Name != null)
            {
                teamWithSameName = await TeamRepository.GetTeamByNameAsync(dto.Name);
            }

            TeamValidator.UpdateTeamValidation(teamWithSameName, teamToUpdate);

            teamToUpdate.Name = dto.Name ?? teamToUpdate.Name;
            teamToUpdate.Description = dto.Description ?? teamToUpdate.Description;
            teamToUpdate.Icon = dto.icon ?? teamToUpdate.Icon;
            teamToUpdate.Pitch.Name = dto.PitchName ?? teamToUpdate.Pitch.Name;
            teamToUpdate.Pitch.Address = dto.PitchLocation ?? teamToUpdate.Pitch.Address;

            TeamRepository.UpdateTeam(teamToUpdate);

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId)
        {
            var team = await TeamRepository.GetTeamDetailsDtoAsync(teamId);

            TeamValidator.GetTeamByIdValidation(team);

            return team;
        }

        #endregion

        #region Admins Manager
        public async Task PromotePlayerToAdminAsync(Guid teamId, string playerIdToPromoteId, string playerIdToPromotingId)
        {
            var playerPromoting = await PlayerRepository.GetPlayerByIdAsync(playerIdToPromotingId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerPromoting, teamId);

            var playerToPromote = await PlayerRepository.GetPlayerByIdAsync(playerIdToPromoteId);
            AuthorizationValidator.ValidatePlayerAutorizationIsMember(playerPromoting, teamId);

            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            
            TeamValidator.PromoteMemberToAdminValidation(existingTeam, playerToPromote, playerPromoting);

            playerToPromote.IsAdmin = true;
            playerToPromote.IsAdminLastChangedAt = DateTime.UtcNow;

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task DemoteAdminToPlayerAsync(Guid teamId, string adminIdToDemote, string adminDemotingId)
        {

            var playerToDemote = await PlayerRepository.GetPlayerByIdAsync(adminIdToDemote);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerToDemote, teamId);

            var playerDemoting = await PlayerRepository.GetPlayerByIdAsync(adminDemotingId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerDemoting, teamId);

            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            TeamValidator.DemoteAdminToMemberValidation(existingTeam, playerToDemote, playerDemoting);

            playerToDemote.IsAdmin = false;
            playerToDemote.IsAdminLastChangedAt = DateTime.UtcNow;
            await UnityOfWork.SaveChangesAsync();
        }

        #endregion

        #region MemberShipRequest
        public async Task<MemberShipRequestDto> SendMembershipRequestAsync(Guid teamId, string playerIdToInvite, string adminUserId)
        {
            var admin = await PlayerRepository.GetPlayerByIdAsync(adminUserId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(admin, teamId);

            var playerToInvite = await PlayerRepository.GetPlayerByIdAsync(playerIdToInvite);
            AuthorizationValidator.ValidatePlayerAutorizationWithoutTeam(playerToInvite);

            var team = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var existing = await MembershipRequestRepository.GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId);

            TeamValidator.SendMembershipRequestValidation(existing, team);
            PlayerValidator.PlayerExists(playerToInvite);

            var invite = new MembershipRequest
            {
                Id = Guid.NewGuid(),
                IdPlayer = playerIdToInvite,
                IdTeam = teamId,
                InviteDate = DateTime.UtcNow,
                IsPlayerSender = false
            };

            await MembershipRequestRepository.AddMembershipRequest(invite);
            await UnityOfWork.SaveChangesAsync();

            return new MemberShipRequestDto
            {
                RequestId = invite.Id,
                PlayerId = invite.IdPlayer,
                PlayerName = invite.Player.Name,
                TeamId = invite.IdTeam,
                TeamName = invite.Team.Name,
                RequestDate = invite.InviteDate,
                IsPlayerSender = invite.IsPlayerSender
            };
        }

        public async Task AcceptMembershipRequestAsync(Guid teamId, Guid requestId, string adminUserId)
        {
            var playerAccepting = await PlayerRepository.GetPlayerByIdAsync(adminUserId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerAccepting, teamId);

            var existingTeam = await TeamRepository.GetTeamForMembershipRequestAsync(teamId);

            if (existingTeam.MembershipRequests == null) { 
                existingTeam.MembershipRequests = new List<MembershipRequest>();
            }

            var requestToRemove = existingTeam?.MembershipRequests.FirstOrDefault(r => r.Id == requestId);
            if (requestToRemove == null)
            {
                throw new ValidationException($"A equipa com Id '{teamId}' não possui um pedido de adesão com Id '{requestId}'.");
            }

            TeamValidator.ApproveMembershipRequestValidation(existingTeam, requestId);

            var playerAccepted = await PlayerRepository.GetPlayerByIdAsync(requestToRemove.IdPlayer);

            existingTeam.MembershipRequests.Remove(requestToRemove);
            //Apaga os pedidos de adesão que o jogador enviou e deixa os que o mesmo recebeu
            if (playerAccepted?.MembershipRequests != null)
            {
                foreach (MembershipRequest request in playerAccepted.MembershipRequests)
                {
                    if (request.IsPlayerSender)
                    {
                        playerAccepted.MembershipRequests.Remove(request);
                        break;
                    }
                }
            }

            playerAccepted.IdTeam = teamId;
            existingTeam.Members.Add(playerAccepted);

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task RejectMembershipRequestAsync(Guid teamId, Guid requestId, string adminUserId)
        {
            var playerRejecting = await  PlayerRepository.GetPlayerByIdAsync(adminUserId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerRejecting, teamId);

            var existingTeam = await TeamRepository.GetTeamForMembershipRequestAsync(teamId);

            if (existingTeam.MembershipRequests == null)
            {
                existingTeam.MembershipRequests = new List<MembershipRequest>();
            }

            var requestToRemove = existingTeam?.MembershipRequests.FirstOrDefault(r => r.Id == requestId);
            if (requestToRemove == null) 
            {
                throw new ValidationException($"A equipa com Id '{teamId}' não possui um pedido de adesão com Id '{requestId}'.");
            }

            TeamValidator.RejectMembershipRequestValidation(existingTeam, requestId);

            var playerRejected = await PlayerRepository.GetPlayerByIdAsync(requestToRemove.IdPlayer);

            existingTeam.MembershipRequests.Remove(requestToRemove);

            if (playerRejected?.MembershipRequests != null)
            {
                var playerRequest = playerRejected.MembershipRequests.FirstOrDefault(r => r.Id == requestId);
                if (playerRequest != null)
                {
                    playerRejected.MembershipRequests.Remove(playerRequest);
                }
            }

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid teamId, string adminUserId)
        {
            var adminConsulting = await PlayerRepository.GetPlayerByIdAsync(adminUserId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(adminConsulting, teamId);

            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);

            if (!existingTeam.MembershipRequests.Any())
            {
                var fakeRequest = (MembershipRequest)Activator.CreateInstance(typeof(MembershipRequest), nonPublic: true)!;
                existingTeam.MembershipRequests.Add(fakeRequest);
            }

            TeamValidator.GetMembershipRequestsValidation(existingTeam, adminConsulting);

            return await TeamRepository.GetMembershipRequestsDtoAsync(teamId);
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncWithFilters(Guid teamId, string adminUserId, FilterMembershipRequestsTeam filters)
        {
            var admin = await PlayerRepository.GetPlayerByIdAsync(adminUserId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(admin, teamId);

            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);

            if (existingTeam.MembershipRequests == null)
            {
                existingTeam.MembershipRequests = new List<MembershipRequest>();
            }

            if (!existingTeam.MembershipRequests.Any())
            {
                var fakeRequest = (MembershipRequest)Activator.CreateInstance(typeof(MembershipRequest), nonPublic: true)!;
                existingTeam.MembershipRequests.Add(fakeRequest);
            }

            TeamValidator.GetMembershipRequestsValidation(existingTeam, admin);

            return await TeamRepository.GetMembershipRequestsDtoAsyncWithFilters(teamId, filters);
        }

        #endregion

        #region Members Team
        public async Task<List<PlayerDetailsDto>> GetTeamPlayersAsync(Guid teamId)
        {
            var team = await TeamRepository.GetTeamForMemberManagementAsync(teamId);

            TeamValidator.GetTeamMembersValidation(team);

            var playerDtos = team.Members.Select(player => new PlayerDetailsDto
            {
                Name = player.Name,
                Height = player.Height,
                IdTeam = player.IdTeam,
                Position = player.Position,
                IsAdmin = player.IsAdmin
            }).ToList();

            return playerDtos;
        }

        public async Task<List<PlayerDetailsDto>> GetTeamPlayersAsyncWithFilters(Guid teamId, FilterTeamPlayers filters)
        {
            var team = await TeamRepository.GetTeamForMemberManagementAsync(teamId);

            TeamValidator.GetTeamMembersValidation(team);

            return await TeamRepository.GetTeamPlayersDtoAsyncWithFilters(teamId, filters);
        }

        public async Task RemovePlayerFromTeamAsync(Guid teamId, string playerIdToRemove, string playerRemovingId)
        {
            var playerRemoving = await PlayerRepository.GetPlayerByIdAsync(playerRemovingId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerRemoving, teamId);

            var playerToRemove = await PlayerRepository.GetPlayerByIdAsync(playerIdToRemove);
            AuthorizationValidator.ValidatePlayerAutorizationIsMember(playerToRemove, teamId);

            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);


            TeamValidator.RemovePlayerFromTeamValidation(existingTeam, playerRemoving, playerToRemove);

            existingTeam.Members.Remove(playerToRemove);
            playerToRemove.IdTeam = null;
            if (playerToRemove.IsAdmin)
            {
                playerToRemove.IsAdmin = false;
                playerToRemove.IsAdminLastChangedAt = DateTime.UtcNow;
            }

            await UnityOfWork.SaveChangesAsync();
        }

        #endregion

        #region List Team To MatchInvite
        public async Task<List<InfoTeamsDto>> SearchTeamsAsync(Guid idTeam)
        {
            TeamValidator.ValidateVariableSearchTeam(idTeam);
            var team = await TeamRepository.GetTeamByIdAsync(idTeam);

            TeamValidator.ValidateTeamSearch(team);


            return await TeamRepository.GetListTeamsForTeams(idTeam);
        }

        public async Task<List<InfoTeamsDto>> SearchTeamsWithFiltersAsync(Guid idTeam, FilterListTeamDto filters)
        {
            TeamValidator.ValidateVaribleSearchTeamWithFilters(idTeam, filters);
            var team = await TeamRepository.GetTeamByIdAsync(idTeam);

            TeamValidator.ValidateTeamSearch(team);

            return await TeamRepository.GetListTeamsByTeamsWithFilters(idTeam, filters);
        }
        #endregion

        #region List Player To MemberShipRequest
        public async Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeam()
        {
            var playersWithoutTeam = await PlayerRepository.GetPlayersWithoutTeamAsync();

            if (playersWithoutTeam == null || !playersWithoutTeam.Any())
                throw new NotFoundException("Não existem jogadores sem equipa no momento.");

            return playersWithoutTeam;
        }

        public async Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeamWithFilters(FilterPlayersWithoutTeamDto filter)
        {
            TeamValidator.ValidateFiltersGetPlayersWithout(filter);

            return await TeamRepository.GetListPlayersWithoutTeamtWithFilters(filter);
        }

        #endregion
    }
}