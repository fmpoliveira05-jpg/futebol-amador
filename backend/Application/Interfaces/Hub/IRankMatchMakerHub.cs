using Application.DTOs.RankMatchMaker;

namespace Application.Interfaces.Hub
{
    public interface IRankMatchMakerHub
    {
        public Task JoinRankMatchMaker(Guid idPlayer, Guid idTeam);
        public Task LeaveRankMatchMaker();
        public Task NotifyAllTeams(List<EntryRankMatchMakerHub> teamsToNotify);
        public Task OnGroupClosed(string groupName);
    }
}
