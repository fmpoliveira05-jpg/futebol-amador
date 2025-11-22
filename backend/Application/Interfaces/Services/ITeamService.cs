using Application.DTOs.Filters;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;

namespace Application.Interfaces.Services
{
    public interface ITeamService
    {
        Task<Guid> CreateTeamAsync(CreateTeamDto teamDto, string adminUserId);

        Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId);

        Task UpdateTeamInfoAsync(Guid teamId, UpdateTeamDto dto, string currentUserId);

        Task DeleteTeamAsync(Guid teamId, string currentUserId);

        Task<List<PlayerDetailsDto>> GetTeamPlayersAsync(Guid teamId);

        Task<List<PlayerDetailsDto>> GetTeamPlayersAsyncWithFilters(Guid teamId, FilterTeamPlayers filters);

        Task RemovePlayerFromTeamAsync(Guid teamId, string playerIdToRemove, string playerRemovingId);

        //Gestão de admins
        Task PromotePlayerToAdminAsync(Guid teamId, string playerIdToPromoteId, string playerPromotingId);

        Task DemoteAdminToPlayerAsync(Guid teamId, string adminIdToDemote, string currentAdminId);

        Task<List<InfoTeamsDto>> SearchTeamsAsync(Guid idTeam);

        Task<List<InfoTeamsDto>> SearchTeamsWithFiltersAsync(Guid idTeam, FilterListTeamDto filters);

        Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeam();

        Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeamWithFilters(FilterTeamDto filter);
    }
}
