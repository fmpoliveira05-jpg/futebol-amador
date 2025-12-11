using Application.Interfaces;
using Application.Interfaces.Repositories;
using FirebaseAdmin.Messaging;

namespace Application.Services
{
    /// <summary>
    /// Serviço responsável por gerenciar e enviar notificações push através do Firebase Cloud Messaging (FCM).
    /// Este serviço suporta envios individuais (Token único) e em lote (Multicast).
    /// </summary>
    public class NotificationFirebaseService: INotificationFirebaseService
    {
        private readonly IPlayerRepository playerRepository;

        /// <summary>
        /// Inicializa uma nova instância do serviço de notificação.
        /// </summary>
        /// <param name="playerRepository">Repositório para acesso aos dados dos jogadores, incluindo o DeviceToken.</param>
        public NotificationFirebaseService(IPlayerRepository playerRepository)
        {
            this.playerRepository = playerRepository;
        }

        /// <summary>
        /// Envia uma notificação a um único utilizador.
        /// </summary>
        /// <remarks>
        /// A notificação pode ser visual (título/corpo) ou apenas de dados (silenciosa).
        /// Se 'data' for nulo, são aplicados dados padrão para "invite".
        /// </remarks>
        /// <param name="userId">O ID do utilizador a notificar.</param>
        /// <param name="data">Dicionário de dados personalizados a ser enviado ao aplicativo (opcional).</param>
        /// <param name="title">O título da notificação visual (opcional).</param>
        /// <param name="body">O corpo da notificação visual (opcional).</param>
        /// <returns>Uma <see cref="Task"/> que representa a operação de envio assíncrona.</returns>
        public async Task SendNotificationToUser(string userId, Dictionary<string, string> data = null, string title = null, string body = null)
        {
            var user = await playerRepository.GetPlayerByIdAsync(userId);

            if (user == null || string.IsNullOrEmpty(user.DeviceToken))
            {
                return;
            }

            var finalData = GetDefaultDataIfNull(data);
            var notificationObject = CreateNotificationObject(title, body);

            var message = new Message()
            {
                Token = user.DeviceToken,
                Data = finalData,
                Notification = notificationObject
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

        /// <summary>
        /// Envia uma notificação em lote para múltiplos tokens de dispositivo (Multicast).
        /// </summary>
        /// <remarks>
        /// Usa <c>SendEachForMulticastAsync</c> para processamento eficiente de lotes.
        /// Se 'data' for nulo, são aplicados dados padrão para "invite".
        /// A ausência de 'title' e 'body' resulta em uma mensagem apenas de dados (silenciosa).
        /// </remarks>
        /// <param name="tokens">A lista de tokens de dispositivo a serem notificados.</param>
        /// <param name="data">Dicionário de dados personalizados (opcional).</param>
        /// <param name="title">O título da notificação visual (opcional).</param>
        /// <param name="body">O corpo da notificação visual (opcional).</param>
        /// <returns>Uma <see cref="Task"/> que representa a operação de envio assíncrona.</returns>
        public async Task SendMulticastNotification(Guid teamId, Dictionary<string, string> data = null, string title = null, string body = null)
        {
            var memberTokens = await playerRepository.GetDeviceTokensMembersTeam(teamId, null);
            var activeTokens = FilterActiveTokens(memberTokens);

            if (activeTokens == null || !activeTokens.Any())
            {
                return;
            }

            var finalData = GetDefaultDataIfNull(data);
            var notificationObject = CreateNotificationObject(title, body);

            var message = new MulticastMessage()
            {
                Tokens = activeTokens,
                Data = finalData,
                Notification = notificationObject
            };

            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
                if (response.FailureCount > 0)
                {
                    Console.WriteLine($"Sucesso: {response.SuccessCount}, Falhas: {response.FailureCount}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro crítico Firebase Multicast: {ex.Message}");
            }
        }

        public async Task sendNotificationToTeamsAsync(Guid teamId, Guid opponentId, string eventType,
            string title, string bodyForTeam, string bodyForOpponent)
        {
            var payloadTeam = new Dictionary<string, string>
            {
                { "type", eventType },
                { "teamId", teamId.ToString() },
                { "title", title },
                { "body", bodyForTeam }
            };

            SendMulticastNotification(teamId, payloadTeam, null, null);
            
            var payloadOpponent = new Dictionary<string, string>
            {
                { "type", eventType },
                { "teamId", opponentId.ToString() },
                { "title", title },
                { "body", bodyForOpponent }
            };

            await SendMulticastNotification(opponentId, payloadOpponent, null, null);
        }

        public async Task sendNotificationToTeamsWithDataAsync(Guid teamId, Guid opponentId, Dictionary<string, string> dataTeam, Dictionary<string, string> dataOpponent)
        {
            SendMulticastNotification(teamId, dataTeam, null, null);
            await SendMulticastNotification(opponentId, dataOpponent, null, null);
        }

        #region Method Private
        /// <summary>
        /// Cria o objeto Notification apenas se houver Título e Corpo.
        /// Retorna null caso contrário (mensagem silenciosa/apenas dados).
        /// </summary>
        private Notification CreateNotificationObject(string title, string body)
        {
            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(body))
            {
                return new Notification
                {
                    Title = title,
                    Body = body
                };
            }
            return null;
        }

        /// <summary>
        /// Define dados padrão para notificações de convite se nenhum dado for passado.
        /// </summary>
        private Dictionary<string, string> GetDefaultDataIfNull(Dictionary<string, string> data)
        {
            if (data != null) return data;

            return new Dictionary<string, string>()
            {
                { "click_action", "FLUTTER_NOTIFICATION_CLICK" },
                { "type", "invite" }
            };
        }

        private List<string> FilterActiveTokens(List<string> tokens)
        {
            return tokens.Where(token => !string.IsNullOrEmpty(token)).ToList();
        }
        #endregion
    }
}
