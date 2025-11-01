using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;

namespace Application.Interfaces.Services
{
    public interface IPlayerService
    {
        Task<Guid> CreatePlayerAsync(CreatePlayerDTO playerDto);

        Task<PlayerDetailsDTO> GetPlayerByIdAsync(Guid playerId);

        Task UpdatePlayerAsync(Guid playerId, UpdatePlayerDto dto);

        Task DeletePlayerAsync(Guid playerId);

        Task<String> LeaveTeam(Guid playerId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid playerId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncWithFilters(Guid playerId, FilterMembershipRequestsPlayer filters);

        Task<MemberShipRequestDto> AcceptMembershipRequestAsync(Guid playerId, Guid requestId);

        Task<MemberShipRequestDto> RejectMembershipRequestAsync(Guid playerId, Guid requestId);

        Task<MemberShipRequestDto> SendMembershipRequestAsync(Guid playerId, Guid teamId);

        Task<List<InfoTeamsDto>> GetTeamListWithFilters(FilterListTeamDto filter);

        Task<List<InfoTeamsDto>> GetListTeams();

    }
}
