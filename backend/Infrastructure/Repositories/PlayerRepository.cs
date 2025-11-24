using Application.DTOs.Filters;
using Application.DTOs.Player;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

        public async Task<Player?> GetPlayerByEmailAsync(string email)
        {
            return await context.Player.FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<Player?> GetPlayerByIdAsync(string id)
        {
            return await context.Player
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == id);
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

        public async Task<MembershipRequest?> GetPlayerByIdWithRequestsAsync(string playerId)
        {
            return await context.MembershipRequests
                .Include(m => m.Player)
                .ThenInclude(m => m.Team)
                .FirstOrDefaultAsync(p => p.IdPlayer == playerId);
        }

        public async Task<List<InfoPlayerDto?>> GetPlayersList(FilterTeamDto? filters)
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
            var query = context.Player.AsQueryable();

            if (filters != null)
            {
                if (!string.IsNullOrEmpty(filters.PlayerName))
                {
                    var upperCase = filters.PlayerName.ToLower();
                    query = query.Where(p => p.Name.ToLower().Contains(upperCase));
                }

                if (!string.IsNullOrEmpty(filters.City))
                {
                    var fragment = filters.City.ToLower();
                    query = query.Where(p =>
                        EF.Functions.Like(p.Address.ToLower(), "%, %" + fragment + "%")
                        &&
                        !EF.Functions.Like(p.Address.ToLower(), "%, %" + fragment + "%,%")
                    );
                }

                if (filters.MinAge.HasValue)
                {
                    query = query.Where(p => EF.Functions.DateDiffDay(p.DateOfBirth, dateNow) >= filters.MinAge);
                }

                if (filters.MaxAge.HasValue)
                {
                    query = query.Where(p => EF.Functions.DateDiffDay(p.DateOfBirth, dateNow) <= filters.MaxAge);
                }

                if (filters.MinHeight.HasValue)
                {
                    query = query.Where(p => p.Height >= filters.MinHeight);
                }

                if (filters.MaxHeight.HasValue)
                {
                    query = query.Where(p => p.Height <= filters.MaxHeight);
                }

                if (filters.Position.HasValue)
                {
                    query = query.Where(p => p.Position == filters.Position);
                }
            }

            var list = await query
                .Select(p => new InfoPlayerDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    Age = EF.Functions.DateDiffDay(p.DateOfBirth, dateNow) / 365,
                    Heigth = p.Height,
                    Position = p.Position,
                    HaveTeam = p.IdTeam != null
                })
                .ToListAsync();

            return list!; 
        }
    }
}
