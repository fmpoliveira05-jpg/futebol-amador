using Domain.Entities;
using System.Collections.Concurrent;

namespace Application.Interfaces.Validators.Hub
{
    public interface IStartMatchHubValidator
    {
        public void ValidateVariableJoinMatch(Guid matchId, Guid userId, string connectionId);
        public void ValidateMatchJoinMatch(Matches match);
        public void ValidateJoinMatch(TeamStatistics teamMatchAdmin, Guid idTeam, ConcurrentDictionary<Guid, string> hub);
    }
}
