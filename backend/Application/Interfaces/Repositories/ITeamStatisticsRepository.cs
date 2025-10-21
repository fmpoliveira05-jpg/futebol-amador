using Domain.Entities;

namespace Application.Interfaces.Repositorys
{
    public interface ITeamStatisticsRepository
    {
        Task AddTeamStatistics(TeamStatistics teamStatistics);

        Task<TeamStatistics> GetTeamByIdAndMatch(Guid idMatch, Guid idTeam);
    }
}
