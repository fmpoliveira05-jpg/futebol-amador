using Application.DTOs;
using Application.Hubs;
using Application.Interfaces.Hub;
using Application.Interfaces.Services;
using Application.Interfaces.Validators.Hub;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    public class FinishMatchHub: Hub<IFinishMatchHub>
    {
        private readonly IManagerFinishMatchService managerFinishMatchService;
        private readonly IHubFinshMatchValidator validator;
        private readonly IGeralHubValidator geralValidator;
        public FinishMatchHub(IManagerFinishMatchService managerFinishMatchService,
            IHubFinshMatchValidator validator, IGeralHubValidator geralValidator)
        {
            this.managerFinishMatchService = managerFinishMatchService;
            this.validator = validator;
            this.geralValidator = geralValidator;
        }

        public async Task JoinStartMatch(Guid idMatch, FinishMatchDTO finishMatch)
        {
            var connectionId = Context.ConnectionId;
            var userId = Guid.Parse(Context.User.Identity.Name);
            JoinFinishMatch result;
            var groupName = GetGroupName(idMatch);

            try
            {                
                result = await managerFinishMatchService.JoinHubAsync(idMatch, finishMatch, userId, connectionId);
            }
            catch (ArgumentNullException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }

            await Groups.AddToGroupAsync(connectionId, groupName);

            Context.Items["HubMatchId"] = idMatch;
            Context.Items["HubTeamId"] = result.IdTeam;
            
            if (result.IsCoincides.HasValue)
            {
                await CleanHub(groupName, result.FirstAdminConnectionId, connectionId);
            }

            //Meter aqui um else para mandar um aviso a dizer que os resultados não coincidem
            return;
        }

        public async Task EditResult(Guid idMatch, FinishMatchDTO finishMatch)
        {
            var connectionId = Context.ConnectionId;
            var userId = Guid.Parse(Context.User.Identity.Name);
            var groupName = GetGroupName(idMatch);
            JoinFinishMatch result;
            
            try
            {
                result = await managerFinishMatchService.UpdateResult(idMatch, finishMatch, userId, connectionId);
            }
            catch (ArgumentNullException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }

            try
            {
                validator.ValidateIsCoincide(result.IsCoincides);
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
            }

            await CleanHub(groupName, result.FirstAdminConnectionId, connectionId);
            
            return;
        }

        public async Task LeaveStartMatch()
        {
            try
            {
                bool success = await HandleLeaveHub();
                geralValidator.ValidateLeaveMatch(success);
            }
            catch (ArgumentNullException ex)
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
            Guid? matchId = null;
            Guid? teamId = null;

            //Limpeza do Context
            if (Context.Items.TryGetValue("LobbyMatchId", out var m) && m is Guid g)
            {
                matchId = g;
            }

            if (Context.Items.TryGetValue("LobbyTeamId", out var t) && t is Guid tg)
            {
                teamId = tg;
            }

            await managerFinishMatchService.HandleDisconnectAsync(matchId, teamId, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        private string GetGroupName(Guid idMatch)
        {
            return $"hubFinishMatch-{idMatch}";
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
                var removed = await managerFinishMatchService.LeaveHubAsync(matchId, teamId, connectionId);

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

        private async Task CleanHub(string groupName, string? fisrtAdminConnectionId, string SecondAdminConnectionId)
        {
            if (!string.IsNullOrEmpty(fisrtAdminConnectionId))
            {
                await Groups.RemoveFromGroupAsync(fisrtAdminConnectionId, groupName);
            }

            await Groups.RemoveFromGroupAsync(SecondAdminConnectionId, groupName);

            Context.Items.Remove("HubMatchId");
            Context.Items.Remove("HubTeamId");
        }
    }
}
