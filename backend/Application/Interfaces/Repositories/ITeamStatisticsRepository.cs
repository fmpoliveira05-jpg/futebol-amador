using Domain.Entities;

namespace Application.Interfaces.Repositorys
{
    public interface ITeamStatisticsRepository
    {
        Task AddTeamStatistics(TeamStatistics teamStatistics);
    }
}
