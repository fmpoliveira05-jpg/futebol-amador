namespace Application.Interfaces
{
    public interface INotificationFirebaseService
    {
        Task SendNotificationToUser(string userId, Dictionary<string, string> data = null, string title = null, string body = null);

        Task SendMulticastNotification(List<string> tokens, Dictionary<string, string> data = null, string title = null, string body = null);
    }
}
