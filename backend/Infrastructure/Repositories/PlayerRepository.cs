using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
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

        public async Task<Player?> GetPlayerByPhoneNumberAsync(string phoneNumber) {
            return await context.Player.FirstOrDefaultAsync(p => p.Phone == phoneNumber);
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

        public async Task<Player?> GetPlayerByIdAsync(string id)
        {
            return await context.Player.FindAsync(id);
        }

        public async Task<List<Player>> GetPlayersListByIdListAsync(List<string> playerIdList)
        {
            return await context.Player
                .Where(p => playerIdList.Contains(p.Id))
                .ToListAsync();
        }

        public void UpdatePlayer(Player updatedPlayer)
        {
            context.Player.Update(updatedPlayer);
        }

        public async Task<Player?> GetPlayerByIdWithRequestsAsync(string playerId)
        {
            return await context.Player
                .Include(p => p.MembershipRequests)
                    .ThenInclude(r => r.Team)
                .FirstOrDefaultAsync(p => p.Id == playerId);
        }

        public async Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeamAsync()
        {
            return await context.Player
                .Where(p => p.IdTeam == null || p.IdTeam == Guid.Empty)
                .Select(p => new PlayerWithoutTeamInfoDto
                {
                    PlayerId = p.Id,
                    Name = p.Name,
                    Age = DateTime.Now.Year - p.DateOfBirth.Year -
                          ((DateTime.Now.DayOfYear < p.DateOfBirth.DayOfYear) ? 1 : 0),
                    Position = p.Position
                })
                .ToListAsync();
        }
    }
}
