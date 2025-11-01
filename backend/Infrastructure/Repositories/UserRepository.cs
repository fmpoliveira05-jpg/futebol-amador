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

        public async Task<Player?> GetUserByEmailAsync(string email)
        {
            return await context.Player.FirstOrDefaultAsync(p => p.Email.ToUpper() == email.ToUpper());
        }

        public async Task<Player?> GetUserByIdAsync(Guid id)
        {
            return await context.Player.FirstOrDefaultAsync(p => p.Id == id);
        }

        public void UpdateUser(Player player)
        {
            context.Player.Update(player);
        }
    }
}