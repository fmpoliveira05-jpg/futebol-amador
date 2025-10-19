using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ITeamStatisticsRepository
    {
        Task AddTeamStatistics(TeamStatistics teamStatistics);
    }
}
