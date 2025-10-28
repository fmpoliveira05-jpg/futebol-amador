using Application.Interfaces.Repositories;
using Domain.Entities;

namespace Infrastructure.Repositories
{
    public class UserRepository
    {
        public Task AddAsync(Player player)
        {
            throw new NotImplementedException();
        }

        public void DeletePlayer(Player playerToRemove)
        {
            throw new NotImplementedException();
        }

        public Task<List<Player>?> GetAllTPlayersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Player?> GetPlayerByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<Player?> GetPlayerByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Player>> GetPlayersListByIdListAsync(List<Guid> playerIdList)
        {
            throw new NotImplementedException();
        }

        public Task<Teams?> GetUserByIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public void UpdatePlayer(Player updatedPlayer)
        {
            throw new NotImplementedException();
        }
    }
}
