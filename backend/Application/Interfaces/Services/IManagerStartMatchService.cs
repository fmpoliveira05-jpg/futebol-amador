using Application.Hubs;

namespace Application.Interfaces.Services
{
    public interface IManagerStartMatchService
    {
        Task<JoinStartMatchResult> JoinHubAsync(Guid matchId, Guid userId, string connectionId);
        Task<bool> LeaveHubAsync(Guid matchId, Guid idTeam, string connectionId);
        Task<bool> HandleDisconnectAsync(Guid? maybeMatchId, Guid? maybeTeamId, string connectionId);
    }
}
