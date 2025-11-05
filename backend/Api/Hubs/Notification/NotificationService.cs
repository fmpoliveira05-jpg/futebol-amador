using Application.Interfaces.Services.Hub;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public Task JoinNotificationHub(Guid teamId)
        {
            throw new NotImplementedException();
        }

        public Task LeaveNotificationHub()
        {
            throw new NotImplementedException();
        }

        public async Task SendUserAsync(string userId, string title, string body, object? data = null) =>
            await hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new { title, body, data });

        public async Task SendTeamAsync(string teamId, string title, string body, object? data = null) =>
            await hubContext.Clients.Group(teamId).SendAsync("ReceiveNotification", new { title, body, data });

        public async Task BroadcastAsync(string title, string body, object? data = null) =>
            await hubContext.Clients.All.SendAsync("ReceiveNotification", new { title, body, data });
    }
}
