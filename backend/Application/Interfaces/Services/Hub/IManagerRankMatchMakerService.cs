using Application.DTOs.RankMatchMaker;

namespace Application.Interfaces.Services.Hub
{
    public interface IManagerRankMatchMakerService
    {
        public Task<EntryRankMatchMakerHub> JoinRankMatchMaker(string idPlayer, Guid idTeam, TimeOnly hoursGame, string connectionId);
        public Task<bool> LeaveRankMatchMakerAsync(Guid teamId, string connectionId);
        public Task<bool> HandleDisconnectAsync(Guid? maybeTeamId, string connectionId);
        public Task<Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub>> MatchMaker(CriteriaMatchMaker criteria);
    }
}
