namespace Application.Interfaces.Hub
{
    public interface IRankMatchMakerHub
    {
        public Task JoinRankMatchMaker(Guid idPlayer, Guid idTeam);
        public Task LeaveRankMatchMaker();
    }
}
