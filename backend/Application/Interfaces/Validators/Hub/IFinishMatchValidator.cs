using Application.DTOs;
using Application.Hubs;
using Domain.Entities;
using System.Collections.Concurrent;

namespace Application.Interfaces.Validators.Hub
{
    public interface IFinishMatchValidator
    {
        public void ValidateVariableJoinMatch(Guid matchId, FinishMatchDTO finishMatch,Guid userId, string connectionId);
        public void ValidateMatchJoinMatch(Matches match);
        public void ValidateJoinMatch(TeamStatistics teamMatchAdmin, Guid teamId, ConcurrentDictionary<Guid, EntryHubFinishMatch> hub);
        public void ValidateMatchResultTwoTeams(FinishMatchDTO firstResult, FinishMatchDTO secondResult);
        public void ValidateOpponentTeam(TeamStatistics opponent);
    }
}
