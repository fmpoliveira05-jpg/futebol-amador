using Application.DTOs.Player;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IPlayerRepository
    {
        Task<List<Player>> GetPlayersListByIdListAsync(List<string> playerIdList);

        Task<Player?> GetPlayerByPhoneNumberAsync(string phoneNumber);

        Task<List<Player>?> GetAllTPlayersAsync();

        Task<Player?> GetPlayerByIdAsync(string id);

        Task<Player?> GetPlayerByEmailAsync(string email);

        void DeletePlayer(Player playerToRemove);

        void UpdatePlayer(Player updatedPlayer);

        Task<Player?> GetPlayerByIdWithRequestsAsync(string userId);

        Task AddAsync(Player player);

        Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeamAsync();
    }
}
