using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services.Hub
{
    public interface INotificationService
    {
        public Task JoinNotificationHub(Guid teamId);

        public Task LeaveNotificationHub();

        public Task SendUserAsync(string userId, string title, string body, object? data = null);

        public Task SendTeamAsync(string teamId, string title, string body, object? data = null);

        public Task BroadcastAsync(string title, string body, object? data = null);
    }
}
