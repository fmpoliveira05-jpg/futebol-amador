using Application.DTOs.Team;

namespace Application.Interfaces.Services
{
    public interface ILeaderboardService
    {
        Task<List<TeamLeaderboardDto>> GetLeaderboardAsync();
    }
}