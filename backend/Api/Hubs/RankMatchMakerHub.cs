using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Hub;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators.Hub;
using Domain.Constants;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    public class RankMatchMakerHub: Hub<IRankMatchMakerHub>
    {
        private readonly IManagerRankMatchMakerService service;
        private readonly IGeralHubValidator geralValidator;
        
        public RankMatchMakerHub(IManagerRankMatchMakerService service, 
            IGeralHubValidator geralValidator)
        {
            this.service = service;
            this.geralValidator = geralValidator;
        }

        public async Task JoinRankMatchMaker(Guid idPlayer, Guid idTeam)
        {
            var connectionId = Context.ConnectionId;
            var userId = Guid.Parse(Context.User.Identity.Name);
            InfoTeamRankMatchMakerDto result;
            var groupName = GetGroupName(idTeam);

            //Ver se tenho mais alguma exception
            try
            {
                result = await service.JoinRankMatchMaker(idPlayer, idTeam, connectionId);
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }

            await Groups.AddToGroupAsync(connectionId, groupName);

            Context.Items[ModelConstants.RankMatchMakerHubConst.ContentTeamId] = result.IdTeam;
         
            //Chmamar metodo ou inciar lógica para o team inciar a procura por outro team durante 5 minutos
        }

        public async Task LeaveRankMatchMaker() {
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
        }

        //Adaptar para depois isto
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Guid? idTeam = null;

            if (Context.Items.TryGetValue(ModelConstants.RankMatchMakerHubConst.ContentTeamId, out var t) && t is Guid tg)
            {
                idTeam = tg;
            }

            await service.HandleDisconnectAsync(idTeam, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        private async Task<bool> HandleLeaveHub()
        {
            if (Context.Items.TryGetValue(ModelConstants.RankMatchMakerHubConst.ContentTeamId, out var teamIdObj))
            {
                var teamId = (Guid)teamIdObj;
                var connectionId = Context.ConnectionId;

                var removed = await service.LeaveRankMatchMakerAsync(teamId, connectionId);

                if (removed)
                {
                    var groupName = GetGroupName(teamId);
                    await Groups.RemoveFromGroupAsync(connectionId, groupName);

                    Context.Items.Remove(ModelConstants.RankMatchMakerHubConst.ContentTeamId);
                }

                return removed;
            }

            return false;
        }

        private string GetGroupName(Guid idMatch)
        {
            return $"matchRankMaker-{idMatch}";
        }
    }
}
