using Application.Interfaces.Validators.Hub;
using Domain.Constants;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    /// <summary>
    /// Hub SignalR responsável pelo sistema de notificações em tempo real.
    /// 
    /// Este Hub permite que os utilizadores (jogadores/admins) se conectem a um canal específico da sua equipa
    /// para receber alertas instantâneos, como avisos de "Game Day" enviados pelo serviço de background.
    /// </summary>
    public class NotificationHub : Hub
    {
        private readonly INotificationValidator notificationValidator;
        private readonly IGeralHubValidator validator;

        /// <summary>
        /// Construtor do NotificationHub.
        /// </summary>
        /// <param name="notificationValidator">Validador específico para regras de notificação (ex: pertença à equipa).</param>
        /// <param name="validator">Validador para regras gerais de Hub.</param>
        public NotificationHub(INotificationValidator notificationValidator, IGeralHubValidator validator)
        {
            this.notificationValidator = notificationValidator;
            this.validator = validator;
        }

        /// <summary>
        /// Método invocado por um cliente para subscrever o canal de notificações da sua equipa.
        /// 
        /// Fluxo:
        /// 1. Valida se o utilizador autenticado pertence realmente à equipa solicitada ([ValidateTeamMembershipAsync]).
        /// 2. Adiciona a conexão ao grupo SignalR correspondente à equipa.
        /// </summary>
        /// <param name="teamId">O ID da equipa cujas notificações se pretende receber.</param>
        /// <exception cref="HubException">Se o utilizador não tiver permissão ou não pertencer à equipa.</exception>
        public async Task JoinNotificationHub(Guid teamId)
        {
            var userId = Context.User?.Identity?.Name;
            var connectionId = Context.ConnectionId;
            
            try
            {
                await notificationValidator.ValidateTeamMembershipAsync(teamId, userId);
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }

            Context.Items[ModelConstants.NotificationHubConst.ContentTeamId] = teamId;

            var groupName = GetGroupName(teamId);

            await Groups.AddToGroupAsync(connectionId, groupName);
        }

        /// <summary>
        /// Método invocado explicitamente pelo cliente para parar de receber notificações (Sair do canal).
        /// </summary>
        /// <exception cref="HubException">Se ocorrer erro ao processar a saída.</exception>
        public async Task LeaveNotificationHub()
        {
            try
            {
                bool success = await HandleLeaveHub();
                validator.ValidateLeaveMatch(success);
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }
        }

        /// <summary>
        /// Chamado automaticamente quando a conexão cai ou o cliente fecha a app.
        /// </summary>
        /// <param name="exception">A exceção que causou a desconexão, se houver.</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Guid? teamId = null;

            if (Context.Items.TryGetValue(ModelConstants.NotificationHubConst.ContentTeamId, out var t) && t is Guid tg)
            {
                teamId = tg;
            }

            await base.OnDisconnectedAsync(exception);
        }

        #region Private Methods

        /// <summary>
        /// Lógica auxiliar para remover o utilizador do Grupo SignalR e limpar o contexto.
        /// </summary>
        /// <returns><c>true</c> se a remoção foi bem-sucedida; <c>false</c> caso contrário.</returns>
        private async Task<bool> HandleLeaveHub()
        {
            if (Context.Items.TryGetValue("TeamId", out var teamIdObj) && teamIdObj is Guid teamId)
            {
                var connectionId = Context.ConnectionId;
                await Groups.RemoveFromGroupAsync(connectionId, GetGroupName(teamId));
                Context.Items.Remove(ModelConstants.NotificationHubConst.ContentTeamId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gera o nome padronizado para o grupo de notificações de uma equipa.
        /// </summary>
        /// <param name="teamId">O ID da equipa.</param>
        /// <returns>Uma string no formato "notificationGroup-{Guid}".</returns>
        private string GetGroupName(Guid teamId)
        {
            return ModelConstants.NotificationHubConst.PrefixGroupName + teamId;
        }
        #endregion
    }
}
