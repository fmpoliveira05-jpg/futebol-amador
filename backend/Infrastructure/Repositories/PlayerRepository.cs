using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
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

        public async Task<Player?> GetPlayerByPhoneAsync(string phone)
        {
            return await context.Player.FirstOrDefaultAsync(p => p.Phone == phone);
        }

        public async Task<Player?> GetPlayerByIdAsync(Guid id)
        {
            return await context.Player.FindAsync(id);
        }

        public void UpdatePlayer(Player updatedPlayer)
        {
            context.Player.Update(updatedPlayer);
        }

        public async Task<Player?> GetPlayerByIdWithRequestsAsync(Guid id)
        {
            return await context.Player
                .Include(p => p.MembershipRequests)
                    .ThenInclude(r => r.Team)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsDtoAsync(Guid playerId)
        {
            return await context.MembershipRequests
                .Where(mr => mr.IdPlayer == playerId)
                .Select(mr => new MemberShipRequestDto
                {
                    RequestId = mr.Id,
                    PlayerName = mr.Player.Name,
                    PlayerId = mr.IdPlayer,
                    TeamName = mr.Team.Name,
                    RequestDate = mr.InviteDate,
                    IsPlayerSender = mr.IsPlayerSender
                })
                .ToListAsync();
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsDtoAsyncWithFilters(Guid playerId, FilterMembershipRequestsPlayer filters)
        {
            var query = context.MembershipRequests
                .Where(mr => mr.IdPlayer == playerId);

            if (filters.IsPlayerSender.HasValue)
            {
                query = query.Where(mr => mr.IsPlayerSender == filters.IsPlayerSender.Value);
            }

            if (filters.MinDate.HasValue)
                query = query.Where(mr => DateOnly.FromDateTime(mr.InviteDate) >= filters.MinDate.Value);

            if (filters.MaxDate.HasValue)
                query = query.Where(mr => DateOnly.FromDateTime(mr.InviteDate) <= filters.MaxDate.Value);

            if (!string.IsNullOrWhiteSpace(filters.SenderName))
            {
                var upperName = filters.SenderName.ToUpper();
                query = query.Where(mr => mr.Team.Name.ToUpper().Contains(upperName));
            }

            return await query
                .Select(mr => new MemberShipRequestDto
                {
                    RequestId = mr.Id,
                    PlayerName = mr.Player.Name,
                    PlayerId = mr.IdPlayer,
                    TeamName = mr.Team.Name,
                    RequestDate = mr.InviteDate,
                    IsPlayerSender = mr.IsPlayerSender
                })
                .ToListAsync();
        }
    }
}