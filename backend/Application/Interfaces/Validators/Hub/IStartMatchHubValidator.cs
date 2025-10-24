using Domain.Entities;
using System.Collections.Concurrent;

namespace Application.Interfaces.Validators.Hub
{
    public interface IStartMatchHubValidator
    {
        public void ValidateJoinMatch(Matches match, Teams teamMatchAdmin, Guid idTeam, ConcurrentDictionary<Guid, string> lobby);
        public void ValidateLeaveMatch(bool success);
    }
}
