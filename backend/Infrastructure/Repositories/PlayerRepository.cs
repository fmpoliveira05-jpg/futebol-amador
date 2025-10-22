using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly AmateurFootballContext context;

        public PlayerRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(Player player)
        {
            await context.Player.AddAsync(player);
        }

        public void DeletePlayer(Player playerToRemove)
        {
            context.Player.Remove(playerToRemove);
        }

        public async Task<List<Player>?> GetAllTPlayersAsync()
        {
            return await context.Player.ToListAsync();
        }

        public async Task<Player?> GetPlayerByEmailAsync(string email)
        {
            return await context.Player.FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<Player?> GetPlayerByIdAsync(Guid id)
        {
            return await context.Player.FindAsync(id);
        }

        public void UpdatePlayer(Player updatedPlayer)
        {
            context.Player.Update(updatedPlayer);
        }
    }
}
