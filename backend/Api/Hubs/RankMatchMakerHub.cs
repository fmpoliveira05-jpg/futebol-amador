using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Hub;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators.Hub;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    //[Authorize]
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

        public async Task JoinRankMatchMaker(Guid idTeam, TimeOnly hoursGame)
        {
            var connectionId = Context.ConnectionId;
            var userId = Guid.Parse(Context.User.Identity.Name);
            EntryRankMatchMakerHub result;
            string groupName = "";

            //Ver se tenho mais alguma exception
            try
            {
                result = await service.JoinRankMatchMaker(userId, idTeam, hoursGame, connectionId);
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }

            Context.Items[ModelConstants.RankMatchMakerHubConst.ContentTeamId] = idTeam;

            if (result.ConnectionId == connectionId)
            {
                groupName = GetGroupName(idTeam);
            }
            else
            {
                groupName = GetGroupName(result.Team.IdTeam);
            }

            await Groups.AddToGroupAsync(connectionId, groupName);
            
            if (result.ConnectionId != connectionId) 
            {
                //Notificar os dois teams, ver no startMatch
                await CleanHub(groupName, result.ConnectionId, connectionId);
            }
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

        private static string GetGroupName(Guid idTeam)
        {
            return ModelConstants.RankMatchMakerHubConst.TeamGroupPrefix + idTeam;
        }

        private async Task CleanHub(string groupName, string? fisrtAdminConnectionId, string SecondAdminConnectionId)
        {
            if (!string.IsNullOrEmpty(fisrtAdminConnectionId))
            {
                await Groups.RemoveFromGroupAsync(fisrtAdminConnectionId, groupName);
            }

            await Groups.RemoveFromGroupAsync(SecondAdminConnectionId, groupName);

            Context.Items.Remove(ModelConstants.RankMatchMakerHubConst.ContentTeamId);
        }
    }
}
