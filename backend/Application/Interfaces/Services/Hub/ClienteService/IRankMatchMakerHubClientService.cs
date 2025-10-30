namespace Application.Interfaces.Services.Hub.ClienteService
{
    public interface IRankMatchMakerHubClientService
    {
        public Task InitializeAsync();
        public Task JoinRankMatchMakerAsync(Guid idPlayer, Guid idTeam, TimeOnly hoursGame);
        public Task LeaveRankMatchMakerAsync();
    }
}
