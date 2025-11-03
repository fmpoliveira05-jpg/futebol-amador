using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository
    {
        private readonly AmateurFootballContext context;

        public UserRepository(AmateurFootballContext context)
        {
            this.context = context;
        }
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
            this.context = context;
        }

        public async Task<List<Users?>> GetAllUsersAsync()
        {
            return await context.User.ToListAsync();
        }

        public async Task<Users?> GetUserByIdAsync(Guid id)
        {
            return await context.User.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Users?> GetUserByEmailAsync(string email)
        {
            return await context.User.FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<Users?> GetUserByPhoneAsync(string phone)
        {
            return await context.User.FirstOrDefaultAsync(p => p.Phone == phone);
        }

        public void UpdatePlayer(Player updatedPlayer)
        {
            throw new NotImplementedException();
        }
    }
}