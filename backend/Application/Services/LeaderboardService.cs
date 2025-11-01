using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ITeamRepository _teamRepository;

        public LeaderboardService(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<List<TeamLeaderboardDto>> GetLeaderboardAsync()
        {
            const int top = 100;
            var teams = await _teamRepository.GetTopTeamsAsync(top);

            return teams
                .Select((t, index) => new TeamLeaderboardDto
                {
                    Position = index + 1,
                    TeamName = t.TeamName,
                    CurrentPoints = t.CurrentPoints,
                    RankName = t.RankName
                })
                .ToList();
        }
    }
}