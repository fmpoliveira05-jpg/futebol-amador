using Application.DTOs;
using Application.Hubs;

namespace Application.Interfaces.Services.Hub
{
    public interface IManagerFinishMatchService
    {
        public Task<JoinFinishMatch> JoinHubAsync(Guid matchId, ResultMatchDto finishMatch, Guid userId, string connectionId);
        public Task<JoinFinishMatch> UpdateResult(Guid matchId, ResultMatchDto finishMatch, Guid userId, string connectionId);
        public Task<bool> LeaveHubAsync(Guid matchId, Guid teamId, string connectionId);
        public Task<bool> HandleDisconnectAsync(Guid? maybeMatchId, Guid? maybeTeamId, string connectionId);
    }
}
