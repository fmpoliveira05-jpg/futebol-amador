namespace Application.Interfaces.Services.Hub
{
    /// <summary>
    /// Contrato de serviço para o envio de notificações em tempo real através dos Hubs SignalR.
    /// 
    /// Esta interface abstrai a comunicação com o Hub ([NotificationHub]), permitindo que outros serviços
    /// (ex: Background Workers, Serviços de Domínio) enviem alertas sem depender diretamente do contexto do Hub.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Envia uma notificação direcionada a um utilizador específico.
        /// </summary>
        /// <remarks>
        /// Utiliza o ID do utilizador (Claim ID) para rotear a mensagem para a conexão ativa desse utilizador.
        /// </remarks>
        /// <param name="userId">O ID do utilizador de destino.</param>
        /// <param name="title">O título da notificação.</param>
        /// <param name="body">O corpo da mensagem.</param>
        /// <param name="data">Objeto opcional que contém dados adicionais (payload) para o cliente.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task SendUserAsync(string userId, string title, string body, object? data = null);

        /// <summary>
        /// Envia uma notificação para um grupo específico, tipicamente representando uma Equipa.
        /// </summary>
        /// <remarks>
        /// A mensagem é enviada a todas as conexões subscritas ao grupo identificado pelo [teamId].
        /// </remarks>
        /// <param name="teamId">O ID da equipa (usado como nome do grupo) para onde enviar a mensagem.</param>
        /// <param name="title">O título da notificação.</param>
        /// <param name="body">O corpo da mensagem.</param>
        /// <param name="data">Objeto opcional que contém dados adicionais.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task SendTeamAsync(string teamId, string title, string body, object? data = null);

        /// <summary>
        /// Envia uma notificação de transmissão para TODOS os clientes conectados ao Hub (Broadcast).
        /// </summary>
        /// <param name="title">O título da notificação.</param>
        /// <param name="body">O corpo da mensagem.</param>
        /// <param name="data">Objeto opcional que contém dados adicionais.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task BroadcastAsync(string title, string body, object? data = null);
    }
}