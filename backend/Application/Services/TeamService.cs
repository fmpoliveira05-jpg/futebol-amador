using Application.DTOs.Filters;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators;
using Domain.Entities;

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
        private readonly INotificationService notificationService;

        public TeamService(
            ITeamRepository teamRepository,
            IPlayerRepository playerRepository,
            IUnityOfWork unityOfWork,
            ITeamValidator teamValidator,
            IRankRepository rankRepository,
            IMembershipRequestRepository membershipRequestRepository,
            IPlayerValidator playerValidator,
            IPlayerAuthorizationValidator authorizationValidator,
            INotificationService notificationService)
        {
            TeamRepository = teamRepository;
            PlayerRepository = playerRepository;
            UnityOfWork = unityOfWork;
            TeamValidator = teamValidator;
            RankRepository = rankRepository;
            AuthorizationValidator = authorizationValidator;
            this.notificationService = notificationService;
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

            playerCreating.IsAdmin = true;
            playerCreating.IdTeam = newTeam.Id;
            playerCreating.Team = newTeam;
            playerCreating.IsAdminLastChangedAt = DateTime.UtcNow;
            newTeam.Members ??= new List<Player>();
            newTeam.Members.Add(playerCreating);

            await TeamRepository.AddAsync(newTeam);

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

            /*TODO: Meter para todas as partidas dessa team serem cancelados pelo 
            motivo que a equipa foi eliminada
            */
            TeamRepository.DeleteTeam(teamToDelete);

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task UpdateTeamInfoAsync(Guid teamId, CreateTeamDto dto, string currentUserId)
        {
            var playerTryingToUpdate = await PlayerRepository.GetPlayerByIdAsync(currentUserId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerTryingToUpdate, teamId);

            var teamToUpdate = await TeamRepository.GetTeamForUpdateAsync(teamId);
            Team? teamWithSameName = null;

            if (dto.Name != null)
            {
                teamWithSameName = await TeamRepository.GetTeamByNameAsync(dto.Name);
            }

            TeamValidator.UpdateTeamValidation(teamWithSameName, teamToUpdate);

            teamToUpdate.Name = dto.Name ?? teamToUpdate.Name;
            teamToUpdate.Description = dto.Description ?? teamToUpdate.Description;
            teamToUpdate.Icon = dto.icon ?? teamToUpdate.Icon;
            teamToUpdate.Pitch.Name = dto.HomePitch.Name ?? teamToUpdate.Pitch.Name;
            teamToUpdate.Pitch.Address = dto.HomePitch.Address ?? teamToUpdate.Pitch.Address;

            TeamRepository.UpdateTeam(teamToUpdate);

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId)
        {
            var team = await TeamRepository.GetTeamDetailsDtoAsync(teamId);

            TeamValidator.GetTeamByIdValidation(team);

            return team;
        }

        public async Task<List<InfoTeamsDto>> GetListTeams(FilterListTeamDto? filters)
        {
            if (filters != null)
            {
                TeamValidator.ValidateFilterTeams(filters);
            }

            var listTeam = await TeamRepository.GetListTeams(filters);

            return listTeam;
        }

        #endregion

        #region Admins Manager
        public async Task PromotePlayerToAdminAsync(Guid teamId, string playerIdToPromoteId, string playerIdToPromotingId)
        {
            var playerPromoting = await PlayerRepository.GetPlayerByIdAsync(playerIdToPromotingId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerPromoting, teamId);

            var playerToPromote = await PlayerRepository.GetPlayerByIdAsync(playerIdToPromoteId);

            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            
            TeamValidator.PromoteMemberToAdminValidation(existingTeam, playerToPromote, playerPromoting);

            await notificationService.SendUserAsync(playerIdToPromoteId, "Team Promotion", $"You have been promoted to admin of the team {existingTeam.Name}.");

            playerToPromote.IsAdmin = true;
            playerToPromote.IsAdminLastChangedAt = DateTime.UtcNow;

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task DemoteAdminToPlayerAsync(Guid teamId, string adminIdToDemote, string adminDemotingId)
        {
            var playerDemoting = await PlayerRepository.GetPlayerByIdAsync(adminDemotingId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerDemoting, teamId);

            var playerToDemote = await PlayerRepository.GetPlayerByIdAsync(adminIdToDemote);
            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            TeamValidator.DemoteAdminToMemberValidation(existingTeam, playerToDemote, playerDemoting);

            playerToDemote.IsAdmin = false;
            playerToDemote.IsAdminLastChangedAt = DateTime.UtcNow;
            await UnityOfWork.SaveChangesAsync();
            await notificationService.SendUserAsync(adminIdToDemote, "Team Demotion", $"You have been demoted to player of the team {existingTeam.Name}.");
        }

        #endregion

        #region Members Team
        public async Task<List<PlayerDetailsDto>> GetTeamPlayersAsync(Guid teamId)
        {
            var team = await TeamRepository.GetTeamForMemberManagementAsync(teamId);

            TeamValidator.GetTeamMembersValidation(team);

            var today = DateTime.Today;
            var playerDtos = team.Members.Select(player => new PlayerDetailsDto
            {
                PlayerId = player.Id,
                Name = player.Name,
                IsAdmin = player.IsAdmin,
                Height = player.Height,
                Position = player.Position,
                DateOfBirth = player.DateOfBirth,
                Age = today.Year - player.DateOfBirth.Year,
                IdTeam = player.IdTeam,
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
            await notificationService.SendUserAsync(playerIdToRemove, "Team Ban", $"You have been removed from the team {existingTeam.Name}.");
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

        public async Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeamWithFilters(FilterTeamDto filter)
        {
            TeamValidator.ValidateFiltersGetPlayersWithout(filter);

            return await TeamRepository.GetListPlayersWithoutTeamtWithFilters(filter);
        }

        public async Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeam()
        {
            return await TeamRepository.GetListPlayersWithoutTeam();
        }

        #endregion
    }
}