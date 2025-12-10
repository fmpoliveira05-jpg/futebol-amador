namespace Application.Interfaces
{
    public interface INotificationFirebaseService
    {
        Task SendNotificationToUser(string userId, string titulo, string corpo);

        Task SendNotificationMembersTeam(Guid idTeam, string titulo, string corpo);

        Task SendNotificationAdminsTeam(Guid idTeam, string titulo, string corpo);
    }
}
