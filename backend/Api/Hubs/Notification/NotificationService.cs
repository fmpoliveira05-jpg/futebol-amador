using Application.Interfaces.Services.Hub;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs.Notification
{
    /// <summary>
    /// Implementação do serviço de envio de notificações em tempo real.
    /// 
    /// Esta classe utiliza o [IHubContext] para invocar métodos nos clientes ligados ao [NotificationHub]
    /// a partir de qualquer ponto da aplicação (fora do contexto do Hub).
    /// </summary>
    public class NotificationService : INotificationService
    {
        /// <summary>
        /// O contexto do Hub injetado, que permite o envio de mensagens para os clientes conectados.
        /// </summary>
        private readonly IHubContext<NotificationHub> hubContext;
    
        /// <summary>
        /// Construtor do NotificationService.
        /// </summary>
        /// <param name="hubContext">Contexto do [NotificationHub] injetado via Dependency Injection.</param>
        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        /// <summary>
        /// Envia uma notificação direcionada a um utilizador específico.
        /// </summary>
        /// <remarks>
        /// O [userId] deve corresponder ao identificador do utilizador autenticado (ClaimTypes.NameIdentifier)
        /// mapeado pelo SignalR.
        /// </remarks>
        /// <param name="userId">O ID do utilizador de destino.</param>
        /// <param name="title">O título da notificação.</param>
        /// <param name="body">O corpo da mensagem.</param>
        /// <param name="data">Objeto opcional com dados extra (payload) para ser processado pelo cliente.</param>
        public async Task SendUserAsync(string userId, string title, string body, object? data = null) =>
            await hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new { title, body, data });

        /// <summary>
        /// Envia uma notificação para um grupo de utilizadores (uma Equipa).
        /// </summary>
        /// <remarks>
        /// Utiliza o [teamId] como nome do grupo SignalR.
        /// **Nota:** Certifique-se de que o nome do grupo aqui corresponde à lógica de `Join` no Hub (ex: prefixos).
        /// </remarks>
        /// <param name="teamId">O ID da equipa (nome do grupo) para onde enviar a mensagem.</param>
        /// <param name="title">O título da notificação.</param>
        /// <param name="body">O corpo da mensagem.</param>
        /// <param name="data">Objeto opcional com dados extra.</param>
        public async Task SendTeamAsync(string teamId, string title, string body, object? data = null) =>
            await hubContext.Clients.Group(teamId).SendAsync("ReceiveNotification", new { title, body, data });

        /// <summary>
        /// Envia uma notificação global para TODOS os clientes conectados (Broadcast).
        /// </summary>
        /// <param name="title">O título da notificação.</param>
        /// <param name="body">O corpo da mensagem.</param>
        /// <param name="data">Objeto opcional com dados extra.</param>
        public async Task BroadcastAsync(string title, string body, object? data = null) =>
            await hubContext.Clients.All.SendAsync("ReceiveNotification", new { title, body, data });
    }
}
