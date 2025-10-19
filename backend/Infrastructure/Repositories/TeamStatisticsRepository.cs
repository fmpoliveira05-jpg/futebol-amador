using Application.Interfaces.Repositorys;
using Domain.Entities;
using Infrastructure.Data;

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
            if(teamStatistics == null)
            {
                throw new ArgumentNullException("A teamStatistics não pode ser nulo: ", nameof(teamStatistics));
            }

            await context.TeamStatistics.AddAsync(teamStatistics);
        }
    }
}
