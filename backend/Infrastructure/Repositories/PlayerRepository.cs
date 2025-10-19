using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly AmateurFootballContext _context;

        public PlayerRepository(AmateurFootballContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Player?> GetPlayerByIdAsync(Guid id)
        {
            if (id == Guid.Empty) return null;

            return await _context.Player
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Player?> GetPlayerByIdWithMembershipRequests(Guid id)
        {
            if (id == Guid.Empty) return null;

            return await _context.Player
                .Include(p => p.MembershipRequests)
                    .ThenInclude(mr => mr.Team)
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task UpdatePlayer(Player player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            _context.Player.Update(player);
            return Task.CompletedTask;
        }
    }
}