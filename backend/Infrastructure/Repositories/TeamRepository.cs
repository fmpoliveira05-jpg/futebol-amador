using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    internal class TeamRepository : ITeamRepository
    {
        private readonly AmateurFootballContext DbContext;
        public TeamRepository(AmateurFootballContext dbContext)
        {
            DbContext = dbContext;
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

        public async Task<Teams?> GetTeamById(Guid id)
        {
            return await DbContext.Team.FindAsync(id); // Retorna a equipa ou null
        }

        public async Task<Teams?> GetTeamByName(String name)
        {
            return await DbContext.Team.FindAsync(name); // Retorna a equipa ou null
        }

        public async Task AddAsync(Teams team)
        {
            await DbContext.Team.AddAsync(team);
        }
    }
}
