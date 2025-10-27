namespace Application.Interfaces.Services.Hub
{
    public interface IStartMatchHubClientService
    {
        public Task JoinStartMatchAsync(Guid idMatch);
        public Task LeaveStartMatchAsync();
    }
}
