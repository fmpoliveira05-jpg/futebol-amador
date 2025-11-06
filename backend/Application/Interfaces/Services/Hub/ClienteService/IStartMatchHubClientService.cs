namespace Application.Interfaces.Services.Hub.ClienteService
{
    public interface IStartMatchHubClientService
    {
        public Task InitializeAsync();
        public Task JoinStartMatchAsync(Guid idMatch, Guid idTeam);
        public Task LeaveStartMatchAsync();
    }
}
