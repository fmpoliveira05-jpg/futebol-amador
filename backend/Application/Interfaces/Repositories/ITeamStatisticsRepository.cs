using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ITeamStatisticsRepository
    {
        Task AddTeamStatistics(TeamStatistics teamStatistics);

        Task<TeamStatistics> GetTeamByIdAndMatch(Guid idMatch, Guid idTeam);
    }
}
