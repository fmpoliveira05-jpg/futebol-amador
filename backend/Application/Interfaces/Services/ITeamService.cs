using Application.DTOs.Team;
using Application.DTOs.MemberShip;
using Application.DTOs.Filters;
using Application.DTOs.Match;

namespace Application.Interfaces.Services
{
    public interface ITeamService
    {
        Task<Guid> CreateTeamAsync(CreateTeamDto teamDto, Guid adminUserId);
        Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId);
        Task UpdateTeamInfoAsync(Guid teamId, UpdateTeamDto dto, Guid currentUserId);
        Task DeleteTeamAsync(Guid teamId, Guid currentUserId);
        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid teamId, Guid adminUserId);
        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncWithFilters(Guid teamId, Guid adminUserId, FilterMembershipRequestsTeam filters);
        Task AcceptMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId);
        Task RejectMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId);
        Task RemovePlayerFromTeamAsync(Guid teamId, Guid playerIdToRemove, Guid playerRemovingId);
        Task PromotePlayerToAdminAsync(Guid teamId, Guid playerIdToPromoteId, Guid playerPromotingId);
        Task DemoteAdminToPlayerAsync(Guid teamId, Guid adminIdToDemote, Guid currentAdminId);
        Task<List<MatchDto>> GetTeamScheduleAsync(Guid teamId);
        Task SendMembershipRequestAsync(Guid teamId, Guid playerIdToInvite, Guid adminUserId);
        Task<List<TeamLeaderboardDto>> GetLeaderboardAsync();
        Task<List<InfoTeamsDto>> SearchTeamsAsync(Guid idTeam);
        Task<List<InfoTeamsDto>> SearchTeamsWithFiltersAsync(Guid idTeam, FilterListTeamDto filters);
    }
}
