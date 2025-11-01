using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Domain.Entities;
namespace Application.Interfaces.Repositories
{
    public interface ITeamRepository
    {
        Task<List<Teams>?> GetAllTeamsAsync();
        Task<Teams?> GetTeamByIdAsync(Guid id);
        Task<Teams?> GetTeamByNameAsync(string name);
        Task DeleteTeam(Teams teamToRemove);
        Task UpdateTeam(Teams updatedTeam);
        Task AddAsync(Teams team);
        Task<TeamDetailsDto?> GetTeamDetailsDtoAsync(Guid teamId);
        Task<List<MemberShipRequestDto>?> GetMembershipRequestsDtoAsync(Guid teamId);
        Task<List<MemberShipRequestDto>?> GetMembershipRequestsDtoAsyncWithFilters(Guid teamId, FilterMembershipRequestsTeam filters);
        Task<Teams?> GetTeamForMembershipRequestAsync(Guid id);
        Task<Teams?> GetTeamForDeletionAsync(Guid id);
        Task<Teams?> GetTeamForUpdateAsync(Guid id);
        Task<Teams?> GetTeamByNameWithMembersAsync(string name);
        Task<Teams?> GetTeamForMemberManagementAsync(Guid id);
        Task<Teams?> GetTeamWitchMemberRankAndPitchAsync(Guid id);
        Task<Teams?> GetTeamByIdWithPitchAsync(Guid id);
        Task<Teams?> GetByIdWithReceivedInvitesAndCalendar(Guid id);
        Task<Teams?> GetByIdWithReceivedInvites(Guid id);
        Task<List<TeamLeaderboardDto>> GetTopTeamsAsync(int top);
        Task<List<InfoTeamsDto>> GetListTeamsPlayer();
        Task<List<InfoTeamsDto>> GetListTeamsPlayersWithFilters(FilterListTeamDto filters);
        Task<List<InfoTeamsDto>> GetListTeamsForTeams(Guid idTeam);
        Task<List<InfoTeamsDto>> GetListTeamsByTeamsWithFilters(Guid idTeam, FilterListTeamDto filters);
    }
}
