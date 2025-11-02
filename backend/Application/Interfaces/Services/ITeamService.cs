using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;

namespace Application.Interfaces.Services
{
    public interface ITeamService
    {
        Task<Guid> CreateTeamAsync(CreateTeamDto teamDto, Guid adminUserId);

        Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId);

        Task UpdateTeamInfoAsync(Guid teamId, UpdateTeamDto dto, Guid currentUserId);

        Task DeleteTeamAsync(Guid teamId, Guid currentUserId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid teamId, Guid idAdmin);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncWithFilters(Guid teamId, Guid idAdmin, FilterMembershipRequestsTeam filters);

        Task<MemberShipRequestDto> AcceptMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId);

        Task<MemberShipRequestDto> RejectMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId);

        Task<List<PlayerDetailsDto>> GetTeamPlayersAsync(Guid teamId);

        Task<List<PlayerDetailsDto>> GetTeamPlayersAsyncWithFilters(Guid teamId, FilterTeamPlayers filters);

        Task RemovePlayerFromTeamAsync(Guid teamId, Guid playerIdToRemove, Guid playerRemovingId);

        Task PromotePlayerToAdminAsync(Guid teamId, Guid playerIdToPromoteId, Guid playerPromotingId);

        Task DemoteAdminToPlayerAsync(Guid teamId, Guid adminIdToDemote, Guid currentAdminId);

        Task<List<MatchDto>> GetTeamScheduleAsync(Guid teamId);

        Task<MemberShipRequestDto> SendMembershipRequestAsync(Guid teamId, Guid playerIdToInvite, Guid adminUserId);

        Task<List<InfoTeamsDto>> SearchTeamsAsync(Guid idTeam);

        Task<List<InfoTeamsDto>> SearchTeamsWithFiltersAsync(Guid idTeam, FilterListTeamDto filters);

        Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeam();

        Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeamWithFilters(FilterPlayersWithoutTeamDto filter);
    }
}
