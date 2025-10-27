namespace Application.Interfaces.Services.Hub
{
    public interface IStartMatchHubClientService
    {
        public Task InitializeAsync(Guid idTeam);
        public Task JoinStartMatchAsync(Guid idMatch, Guid idTeam);
        public Task LeaveStartMatchAsync();
    }
}
