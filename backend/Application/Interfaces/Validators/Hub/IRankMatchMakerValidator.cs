using Application.DTOs.RankMatchMaker;
using Domain.Entities;
using System.Collections.Concurrent;

namespace Application.Interfaces.Validators.Hub
{
    public interface IRankMatchMakerValidator
    {
        public void ValidateVariableJoinRankMatchMaker(Guid idPlayer, Guid idTeam, string connectionId);

        public void ValidateTeamJoinRankMatchMaker(Teams? team);

        public void ValidateJoinRankMatchMaker(Teams team, float averageAge, bool? findUser, ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub);
    
        public void ValidateLeaveRankMatchMaker(Guid idTeam);
    }
}
