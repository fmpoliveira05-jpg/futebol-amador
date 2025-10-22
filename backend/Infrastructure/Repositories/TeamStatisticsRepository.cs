using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TeamStatisticsRepository : ITeamStatisticsRepository
    {
        private readonly AmateurFootballContext context;

        public TeamStatisticsRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        public async Task AddTeamStatistics(TeamStatistics teamStatistics)
        {
            await context.TeamStatistics.AddAsync(teamStatistics);
        }

        public async Task<TeamStatistics> GetTeamByIdAndMatch(Guid idMatch, Guid idTeam)
        {
            var query = await context.TeamStatistics
                .Include(T => T.Team)
                .FirstOrDefaultAsync(ts => ts.MatchesId == idMatch
                                    && ts.IdTeam == idTeam);
            return query;
        }
    }
}
