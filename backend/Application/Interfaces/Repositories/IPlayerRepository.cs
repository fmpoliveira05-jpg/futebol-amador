using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IPlayerRepository
    {
        Task<List<Player>> GetPlayersListByIdListAsync(List<string> playerIdList);

        Task<List<Player>?> GetAllTPlayersAsync();

        Task<Player?> GetPlayerByIdAsync(string id);

        Task<Player?> GetPlayerByEmailAsync(string email);

        void DeletePlayer(Player playerToRemove);

        void UpdatePlayer(Player updatedPlayer);

        Task AddAsync(Player player);
    }
}
