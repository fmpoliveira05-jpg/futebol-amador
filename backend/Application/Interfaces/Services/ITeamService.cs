using Application.DTOs.Team;
using Application.DTOs.MemberShip;
using Application.DTOs.Match;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Filters;

namespace Application.Interfaces.Services
{
    public interface ITeamService
    {
        Task<Guid> CreateTeamAsync(CreateTeamDto teamDto, Guid adminUserId);

        Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId);

        Task<List<TeamSummaryDto>> SearchTeamsAsync(Guid playerId, TeamSearchFiltersDto filters);

        Task UpdateTeamInfoAsync(Guid teamId, UpdateTeamDto dto, Guid currentUserId);

        Task DeleteTeamAsync(Guid teamId, Guid currentUserId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid teamId, Guid adminUserId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncWithFilters(Guid teamId, Guid adminUserId, FilterMembershipRequestsTeam filters);

        Task AcceptMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId);

        Task RejectMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId);

        Task RemovePlayerFromTeamAsync(Guid teamId, Guid playerIdToRemove, Guid playerRemovingId);

        //Gestão de admins
        Task PromotePlayerToAdminAsync(Guid teamId, Guid playerIdToPromoteId, Guid playerPromotingId);

        Task DemoteAdminToPlayerAsync(Guid teamId, Guid adminIdToDemote, Guid currentAdminId);

        Task<List<PlayerDetailsDTO>> GetTeamPlayersAsync(Guid teamId);

        Task<List<MatchDto>> GetTeamScheduleAsync(Guid teamId);
    }
}
