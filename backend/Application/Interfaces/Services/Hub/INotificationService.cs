namespace Application.Interfaces.Services.Hub
{
    public interface INotificationService
    {
        public Task SendUserAsync(string userId, string title, string body, object? data = null);

        public Task SendTeamAsync(string teamId, string title, string body, object? data = null);

        public Task BroadcastAsync(string title, string body, object? data = null);
    }
}
