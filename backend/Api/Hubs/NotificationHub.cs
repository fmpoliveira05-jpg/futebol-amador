//Classe responsavel pelo envio instantâneo de notificações (convites, alterações de partida)
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators.Hub;
using Domain.Constants;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly INotificationValidator notificationValidator;
        private readonly IGeralHubValidator validator;

        public NotificationHub(INotificationValidator notificationValidator, IGeralHubValidator validator)
        {
            this.notificationValidator = notificationValidator;
            this.validator = validator;
        }

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

        private string GetGroupName(Guid teamId)
        {
            return ModelConstants.NotificationHubConst.PrefixGroupName + teamId;
        }
        #endregion
    }
}
