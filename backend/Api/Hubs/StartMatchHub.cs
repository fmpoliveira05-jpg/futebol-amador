using Application.Hubs;
using Application.Interfaces.Hub;
using Application.Interfaces.Services;
using Application.Interfaces.Validators.Hub;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    //Descomentar isto quando houver aut para so pessoas autenticadas acederem
    //[Authorize]
    public class StartMatchHub: Hub<IStartMatchHub>
    {
        private readonly IManagerStartMatchService startMatchManager;
        private readonly IGeralHubValidator geralValidator;
        public StartMatchHub(IManagerStartMatchService startMatchManager, IGeralHubValidator geralValidator)
        {
            this.startMatchManager = startMatchManager;
            this.geralValidator = geralValidator;
        }

        public async Task JoinStartMatch(Guid idMatch, Guid idTeam)
        {
            var connectionId = Context.ConnectionId;
            var userId = Guid.Parse(Context.User.Identity.Name);
            var groupName = GetGroupName(idMatch);
            JoinStartMatchResult result;
            
            try
            {
                result = await startMatchManager.JoinHubAsync(idMatch, userId, idTeam, connectionId);
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }

            if (result.IsFirstAdmin)
            {
                await Groups.AddToGroupAsync(connectionId, groupName);

                Context.Items["HubMatchId"] = idMatch;
                Context.Items["HubTeamId"] = result.TeamId;
            }
            else if (result.MatchStarted)
            {
                if (!string.IsNullOrEmpty(result.FirstAdminConnectionId))
                {
                    await Groups.RemoveFromGroupAsync(result.FirstAdminConnectionId, groupName);
                }
                await Clients.Group(groupName).ReceiveStartMatch("O jogo começou!");
            }
        }

        public async Task LeaveStartMatch()
        {
            try
            {
                bool success = await HandleLeaveHub();
                geralValidator.ValidateLeaveMatch(success);
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }

            return;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //tentar cleanup via Context items
            Guid? matchId = null;
            Guid? teamId = null;

            if (Context.Items.TryGetValue("LobbyMatchId", out var m) && m is Guid g)
            {
                matchId = g;
            }

            if (Context.Items.TryGetValue("LobbyTeamId", out var t) && t is Guid tg)
            {
                teamId = tg;
            }

            await startMatchManager.HandleDisconnectAsync(matchId, teamId, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        private string GetGroupName(Guid idMatch)
        {
            return $"match-{idMatch}";
        }

        /*
         * Contém a lógica de limpeza que é partilhada
         * Retorna 'true' se limpou algo, 'false' se não encontrou nada
        */
        private async Task<bool> HandleLeaveHub()
        {
            if (Context.Items.TryGetValue("HubMatchId", out var matchIdObj) &&
                Context.Items.TryGetValue("HubTeamId", out var teamIdObj))
            {
                var matchId = (Guid)matchIdObj;
                var teamId = (Guid)teamIdObj;
                var connectionId = Context.ConnectionId;

                //Remoção do admin do hub
                var removed = await startMatchManager.LeaveHubAsync(matchId, teamId, connectionId);

                if (removed)
                {
                    var groupName = GetGroupName(matchId);
                    await Groups.RemoveFromGroupAsync(connectionId, groupName);

                    // limpar context items
                    Context.Items.Remove("HubMatchId");
                    Context.Items.Remove("HubTeamId");
                }

                return removed;
            }

            return false;
        }
    }
}