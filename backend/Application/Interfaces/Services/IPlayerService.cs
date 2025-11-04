using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;

namespace Application.Interfaces.Services
{
    public interface IPlayerService
    {
        Task<string> CreatePlayerAsync(string userId,string email,CreatePlayerDto playerDTO);

        Task<PlayerDetailsDto> GetPlayerByIdAsync(string playerId);

        Task UpdatePlayerAsync(string playerId, UpdatePlayerDto dto);

        Task DeletePlayerAsync(string playerId);


        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(string playerId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncWithFilters(string playerId, FilterMembershipRequestsPlayer filters);

        Task<MemberShipRequestDto> AcceptMembershipRequestAsync(string playerId, Guid requestId);

        Task<MemberShipRequestDto> RejectMembershipRequestAsync(string playerId, Guid requestId);

        Task<MemberShipRequestDto> SendMembershipRequestAsync(string playerId, Guid teamId);

        Task<List<InfoTeamsDto>> GetTeamListWithFilters(FilterListTeamDto filter);

        Task<List<InfoTeamsDto>> GetListTeams();

        Task<string> LeaveTeam(string playerId);
    }
}
