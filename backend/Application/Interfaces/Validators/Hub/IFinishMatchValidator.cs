using Application.DTOs;
using Application.Hubs;
using Domain.Entities;
using System.Collections.Concurrent;

namespace Application.Interfaces.Validators.Hub
{
    public interface IFinishMatchValidator
    {
        public void ValidateVariableJoinMatch(Guid matchId, ResultMatchDto finishMatch,Guid userId, string connectionId);
        public void ValidateMatchJoinMatch(Matches match);
        public void ValidateJoinMatch(TeamStatistics teamMatchAdmin, Guid teamId, ConcurrentDictionary<Guid, EntryHubFinishMatch> hub);
        public void ValidateUpdateResult(TeamStatistics teamMatchAdmin, Guid teamId, ConcurrentDictionary<Guid, EntryHubFinishMatch> hub);
        public void ValidateMatchResultTwoTeams(ResultMatchDto firstResult, ResultMatchDto secondResult);
        public void ValidateOpponentTeam(TeamStatistics opponent);
    }
}
