using Application.DTOs.Team;
using Application.DTOs.MemberShip;
using Application.DTOs.Match;
using Application.DTOs.PlayerDTOs;

namespace Application.Interfaces.Services
{
    public interface ITeamService
    {
        Task<Guid> CreateTeamAsync(CreateTeamDto teamDto, string adminUserId);

        Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId);

        Task<List<TeamSummaryDto>> SearchTeamsAsync(string playerId, TeamSearchFiltersDto filters);

        Task UpdateTeamInfoAsync(Guid teamId, UpdateTeamDto dto, string currentUserId);

        Task DeleteTeamAsync(Guid teamId, string currentUserId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid teamId, string adminUserId);

        Task AcceptMembershipRequestAsync(Guid teamId, Guid requestId, string adminUserId);

        Task RejectMembershipRequestAsync(Guid teamId, Guid requestId, string adminUserId);

        Task RemovePlayerFromTeamAsync(Guid teamId, string playerIdToRemove, string playerRemovingId);

        //Gestão de admins
        Task PromotePlayerToAdminAsync(Guid teamId, string playerIdToPromoteId, string playerPromotingId);

        Task DemoteAdminToPlayerAsync(Guid teamId, string adminIdToDemote, string currentAdminId);

        Task<List<PlayerDetailsDTO>> GetTeamPlayersAsync(Guid teamId);

        Task<List<MatchDto>> GetTeamScheduleAsync(Guid teamId);
    }
}
