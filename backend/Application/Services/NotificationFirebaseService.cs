using Application.Interfaces;
using Application.Interfaces.Repositories;
using FirebaseAdmin.Messaging;

namespace Application.Services
{
    public class NotificationFirebaseService: INotificationFirebaseService
    {
        private readonly IPlayerRepository _playerRepository;

        public NotificationFirebaseService(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task SendNotificationToUser(string userId, string titulo, string corpo)
        {
            var user = await _playerRepository.GetPlayerByIdAsync(userId);

            if (user == null || string.IsNullOrEmpty(user.DeviceToken))
            {
                return;
            }

            var message = new Message()
            {
                Token = user.DeviceToken,
                Notification = new Notification()
                {
                    Title = titulo,
                    Body = corpo
                },
                Data = new Dictionary<string, string>()
                {
                    { "click_action", "FLUTTER_NOTIFICATION_CLICK" },
                    { "type", "invite" }
                }
            };

            try
            {
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                Console.WriteLine($"Mensagem enviada: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Firebase: {ex.Message}");
            }
        }

        public Task SendNotificationAdminsTeam(Guid idTeam, string titulo, string corpo)
        {
            throw new NotImplementedException();
        }

        public Task SendNotificationMembersTeam(Guid idTeam, string titulo, string corpo)
        {
            throw new NotImplementedException();
        }

    }
}
