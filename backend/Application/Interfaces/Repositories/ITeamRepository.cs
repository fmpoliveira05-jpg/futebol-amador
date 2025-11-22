using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Domain.Entities;
namespace Application.Interfaces.Repositories
{
    public interface ITeamRepository
    {
        Task<List<string>> GetAdminsIdsByTeamIdAsync(Guid teamId);

        Task<List<string>> GetMemberIdsByTeamIdAsync(Guid teamId);

        Task<List<Team>?> GetAllTeamsAsync();

        Task<Team?> GetTeamByIdAsync(Guid id);

        Task<Team?> GetTeamByNameAsync(string name);

        void DeleteTeam(Team teamToRemove);

        void UpdateTeam(Team updatedTeam);

        Task AddAsync(Team team);

        Task<TeamDetailsDto?> GetTeamDetailsDtoAsync(Guid teamId);

        Task<Team?> GetTeamForMembershipRequestAsync(Guid id);

        Task<Team?> GetTeamForDeletionAsync(Guid id);

        Task<Team?> GetTeamForUpdateAsync(Guid id);

        Task<Team?> GetTeamByNameWithMembersAsync(string name);

        Task<Team?> GetTeamForMemberManagementAsync(Guid id);

        Task<Team?> GetTeamWitchMemberRankAndPitchAsync(Guid id);

        Task<Team?> GetTeamByIdWithPitchAsync(Guid id);

        Task<Team?> GetByIdWithReceivedInvitesAndCalendar(Guid id);

        Task<Team?> GetByIdWithReceivedInvites(Guid id);

        Task<List<PlayerDetailsDto>> GetTeamPlayersDtoAsyncWithFilters(Guid teamId, FilterTeamPlayers filter);

        Task<List<TeamLeaderboardDto>> GetTopTeamsAsync(int top);

        Task<List<InfoTeamsDto>> GetListTeamsPlayer();

        Task<List<InfoTeamsDto>> GetListTeamsPlayersWithFilters(FilterListTeamDto filters);

        Task<List<InfoTeamsDto>> GetListTeamsForTeams(Guid idTeam);

        Task<List<InfoTeamsDto>> GetListTeamsByTeamsWithFilters(Guid idTeam, FilterListTeamDto filters);

        Task<List<PlayerWithoutTeamInfoDto>> GetListPlayersWithoutTeam();

        Task<List<PlayerWithoutTeamInfoDto>> GetListPlayersWithoutTeamtWithFilters(FilterTeamDto filters);
    }
}
