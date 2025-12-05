namespace Application.Interfaces
{
    public interface INotificationFirebaseService
    {
        Task SendNotificationToUser(string userId, string titulo, string corpo);
    }
}
