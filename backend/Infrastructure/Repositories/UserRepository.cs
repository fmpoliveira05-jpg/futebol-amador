using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
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

        public async Task<List<User?>> GetAllUsersAsync()
        {
            return await context.User.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await context.User.FirstOrDefaultAsync(p => p.Id == userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await context.User.FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<User?> GetUserByPhoneAsync(string phone)
        {
            return await context.User.FirstOrDefaultAsync(p => p.Phone == phone);
        }

        public void UpdatePlayer(Player updatedPlayer)
        {
            throw new NotImplementedException();
        }
    }
}