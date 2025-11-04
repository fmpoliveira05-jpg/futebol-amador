using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
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

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncWithFilters(Guid teamId, string idAdmin, FilterMembershipRequestsTeam filters);

        Task AcceptMembershipRequestAsync(Guid teamId, Guid requestId, string adminUserId);

        Task RejectMembershipRequestAsync(Guid teamId, Guid requestId, string adminUserId);

        Task<List<PlayerDetailsDto>> GetTeamPlayersAsync(Guid teamId);

        Task<List<PlayerDetailsDto>> GetTeamPlayersAsyncWithFilters(Guid teamId, FilterTeamPlayers filters);
        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid teamId, string adminUserId);

        Task RemovePlayerFromTeamAsync(Guid teamId, string playerIdToRemove, string playerRemovingId);

        //Gestão de admins
        Task PromotePlayerToAdminAsync(Guid teamId, string playerIdToPromoteId, string playerPromotingId);

        Task DemoteAdminToPlayerAsync(Guid teamId, string adminIdToDemote, string currentAdminId);

        Task<MemberShipRequestDto> SendMembershipRequestAsync(Guid teamId, string playerIdToInvite, string adminUserId);

        Task<List<InfoTeamsDto>> SearchTeamsAsync(Guid idTeam);

        Task<List<InfoTeamsDto>> SearchTeamsWithFiltersAsync(Guid idTeam, FilterListTeamDto filters);

        Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeam();

        Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeamWithFilters(FilterPlayersWithoutTeamDto filter);
    }
}
