using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly AmateurFootballContext DbContext;

        public TeamRepository(AmateurFootballContext DbContext)
        {
            this.DbContext = DbContext;
        }

        public void DeleteTeam(Teams teamToRemove)
        {
            DbContext.Team.Remove(teamToRemove);
        }

        public void UpdateTeam(Teams updatedTeam)
        {
            DbContext.Team.Update(updatedTeam);
        }

        public async Task<List<Teams>?> GetAllTeamsAsync()
        {
            return await DbContext.Team.ToListAsync();
        }

        public async Task<Teams?> GetTeamByIdAsync(Guid id)
        {
            return await DbContext.Team.FindAsync(id); // Retorna a equipa ou null
        }
        //verificar se o nome é unico, caso não seja, alterar pra retornar uma lista
        public async Task<Teams?> GetTeamByNameAsync(String name)
        {
            return await DbContext.Team.FirstOrDefaultAsync(t => t.Name == name); // Retorna a equipa ou null
        }

        public async Task AddAsync(Teams team)
        {
            await DbContext.Team.AddAsync(team);
        }

        public async Task<Teams?> GetTeamByIdWithPitchAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.Pitch)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetByIdWithReceivedInvitesAndCalendar(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.ReceivedInvites)
                .Include(t => t.Calendar)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetByIdWithReceivedInvites(Guid id)
        {
            return await DbContext.Team
               .Include(t => t.ReceivedInvites)
               .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
