using Application.DTOs.RankMatchMaker;
using Domain.Entities;
using System.Collections.Concurrent;

namespace Application.Interfaces.Validators.Hub
{
    public interface IRankMatchMakerValidator
    {
        public void ValidateVariableJoinRankMatchMaker(Guid idPlayer, Guid idTeam, TimeOnly hoursGame, string connectionId);
        public void ValidateHoursToMatch(double differenteHoursNowAndGame, Matches match);
        public void ValidateTeamJoinRankMatchMaker(Teams? team);
        public void ValidateJoinRankMatchMaker(Teams team, float averageAge, string city, bool? findUser, ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub);
        public void ValidateLeaveRankMatchMaker(Guid idTeam);
    }
}
