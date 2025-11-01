using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IPlayerService
    {
        Task<Guid> CreatePlayerAsync(CreatePlayerDTO playerDTO);

        Task<PlayerDetailsDTO> GetPlayerByIdAsync(Guid teamId);

        Task UpdatePlayerAsync(Guid playerId, UpdatePlayerDTO dto);

        Task DeletePlayerAsync(Guid playerId);

        Task<String> LeaveTeam(Guid playerId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid playerId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncWithFilters(Guid playerId, FilterMembershipRequestsPlayer filters);

        Task<MemberShipRequestDto> AcceptMembershipRequestAsync(Guid playerId, Guid requestId);

        Task<MemberShipRequestDto> RejectMembershipRequestAsync(Guid playerId, Guid requestId);

        Task<MemberShipRequestDto> SendMembershipRequestAsync(Guid playerId, Guid teamId);

    }
}
