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