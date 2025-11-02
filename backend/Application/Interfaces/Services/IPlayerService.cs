using Application.DTOs.PlayerDTOs;

namespace Application.Interfaces.Services
{
    public interface IPlayerService
    {
        Task<string> CreatePlayerAsync(string userId,string email,CreatePlayerDTO playerDTO);

        Task<PlayerDetailsDTO> GetPlayerByIdAsync(string teamId);

        Task UpdatePlayerAsync(string playerId, UpdatePlayerDTO dto);

        Task DeletePlayerAsync(string playerId);

        Task<String> LeaveTeam(string playerId);
    }
}
