using Application.DTOs.PlayerDTOs;

namespace Application.Interfaces.Services
{
    public interface IPlayerService
    {
        Task<Guid> CreatePlayerAsync(CreatePlayerDTO playerDto);

        Task<PlayerDetailsDTO> GetPlayerByIdAsync(Guid playerId);

        Task UpdatePlayerAsync(Guid playerId, UpdatePlayerDTO dto);

        Task DeletePlayerAsync(Guid playerId);

        Task<String> LeaveTeam(Guid playerId);
    }
}
