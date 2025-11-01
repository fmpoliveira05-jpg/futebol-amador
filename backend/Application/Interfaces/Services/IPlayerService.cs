using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;

namespace Application.Interfaces.Services
{
    public interface IPlayerService
    {
        Task<Guid> CreatePlayerAsync(CreatePlayerDto playerDto);
        Task<PlayerDetailsDTO> GetPlayerByIdAsync(Guid teamId);
        Task UpdatePlayerAsync(Guid playerId, UpdatePlayerDTO dto);
        Task DeletePlayerAsync(Guid playerId);
        Task<String> LeaveTeam(Guid playerId);
        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid playerId);
        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncWithFilters(Guid playerId, FilterMembershipRequestsPlayer filters);
        Task AcceptMembershipRequestAsync(Guid playerId, Guid requestId);
        Task RejectMembershipRequestAsync(Guid playerId, Guid requestId);
        Task SendMembershipRequestAsync(Guid playerId, Guid teamId);
        Task<List<InfoTeamsDto>> GetListTeams();
        Task<List<InfoTeamsDto>> GetTeamListWithFilters(FilterListTeamDto filter);
    }
}
