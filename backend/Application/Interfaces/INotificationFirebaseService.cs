namespace Application.Interfaces
{
    public interface INotificationFirebaseService
    {
        Task SendNotificationToUser(string userId, Dictionary<string, string> data = null, string title = null, string body = null);

        Task SendMulticastNotification(Guid teamId, Dictionary<string, string> data = null, string title = null, string body = null);

        Task sendNotificationToTeamsAsync(Guid teamId, Guid opponentId, string eventType,
                string title, string bodyForTeam, string bodyForOpponent);
    }
}
