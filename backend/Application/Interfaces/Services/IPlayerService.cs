using Application.DTOs.PlayerDTOs;

namespace Application.Interfaces.Services
{
    public interface IPlayerService
    {
        Task<Guid> CreatePlayerAsync(CreatePlayerDTO playerDTO);

        Task<PlayerDetailsDTO> GetPlayerByIdAsync(Guid teamId);

        Task UpdatePlayerAsync(Guid playerId, UpdatePlayerDTO dto);

        Task DeletePlayerAsync(Guid playerId);

        Task<String> LeaveTeam(Guid playerId);
    }
}
