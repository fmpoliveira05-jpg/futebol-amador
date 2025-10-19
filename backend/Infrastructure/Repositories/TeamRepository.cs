using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly AmateurFootballContext _context;

        public TeamRepository(AmateurFootballContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task DeleteTeam(Teams teamToRemove)
        {
            if (teamToRemove == null)
            {
                throw new ArgumentNullException(nameof(teamToRemove));
            }

            _context.Team.Remove(teamToRemove);
            return Task.CompletedTask;
        }

        public Task UpdateTeam(Teams updatedTeam)
        {
            if (updatedTeam == null)
            {
                throw new ArgumentNullException(nameof(updatedTeam));
            }

            _context.Team.Update(updatedTeam);
            return Task.CompletedTask;
        }

        public async Task<List<Teams>?> GetAllTeamsAsync()
        {
            return await _context.Team.ToListAsync();
        }

        public async Task<Teams?> GetTeamByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            return await _context.Team.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetTeamByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return await _context.Team.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task AddAsync(Teams team)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }

            await _context.Team.AddAsync(team);
        }

        public async Task<Teams?> GetTeamByIdWithPitchAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            return await _context.Team
                .Include(t => t.Pitch)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetByIdWithReceivedInvitesAndCalendar(Guid id)
        {
            if (id == Guid.Empty) {
                return null;
            }

            return await _context.Team
                .Include(t => t.ReceivedInvites)
                .Include(t => t.Calendar)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        public async Task<Teams?> GetByIdWithReceivedInvites(Guid id)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            return await _context.Team
               .Include(t => t.ReceivedInvites)
               .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}