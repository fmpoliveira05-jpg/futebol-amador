using Application.Hubs;

namespace Application.Interfaces.Services.Hub
{
    public interface IManagerStartMatchService
    {
        public Task<JoinStartMatchResult> JoinHubAsync(Guid matchId, string userId, Guid idTeam, string connectionId);
        public Task<bool> LeaveHubAsync(Guid matchId, Guid idTeam, string connectionId);
        public Task<bool> HandleDisconnectAsync(Guid? maybeMatchId, Guid? maybeTeamId, string connectionId);
    }
}
