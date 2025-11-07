using Api.Hubs;
using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Hub;
using Application.Interfaces.Services.Hub;
using Domain.Constants;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace Application.Services.BackGroundServices
{
    public class RankMatchMakerBackGroundService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;

        /** Permite comunicar com o Hub (para enviar notificações aos clientes) */
        private readonly IHubContext<RankMatchMakerHub, IRankMatchMakerHub> hubContext;

        private readonly ILogger<RankMatchMakerBackGroundService> logger;

        /** Intervalo de tempo entre cada verificação de matchmaking */
        private readonly TimeSpan interval = TimeSpan.FromSeconds(5);

        private readonly TimeSpan intervalToIncreaseCriteria = TimeSpan.FromMinutes(5);

        private readonly TimeSpan intervalToResetCriteria = TimeSpan.FromMinutes(ModelConstants.GeralTimeInHubConst.timeInMatchMackerHub);
        public RankMatchMakerBackGroundService(
            IServiceScopeFactory scopeFactory,
            IHubContext<RankMatchMakerHub, IRankMatchMakerHub> hubContext,
            ILogger<RankMatchMakerBackGroundService> logger)
        {
            this.scopeFactory = scopeFactory;
            this.hubContext = hubContext;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var minutesTaskUpdateCriteria = Stopwatch.StartNew();
            var minutesTaskResetCriteria = Stopwatch.StartNew();
            var diffAverageAge = ModelConstants.DeafultCriteriaMatchMaker.differenceAverageAge;
            var diffPoints = ModelConstants.DeafultCriteriaMatchMaker.differencePoint;
            CriteriaMatchMaker criteria;
            logger.LogInformation("RankMatchMakerBackgroundService iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (minutesTaskUpdateCriteria.Elapsed >= intervalToIncreaseCriteria
                        && diffAverageAge < ModelConstants.DeafultCriteriaMatchMaker.maxDifferenceAverageAge
                        && diffPoints < ModelConstants.DeafultCriteriaMatchMaker.maxDifferencePoint)
                    {
                        diffAverageAge = diffAverageAge + 0.2f;
                        diffPoints = diffPoints + 2;

                        minutesTaskUpdateCriteria.Restart();
                    }

                    criteria = new CriteriaMatchMaker
                    {
                        differencPoints = diffPoints,
                        diffAverageAge = diffAverageAge
                    };

                    //Forma que preciso de utilizar para chamar um service
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var matchMakerService = scope.ServiceProvider.GetRequiredService<IManagerRankMatchMakerService>();
                        var result = await matchMakerService.MatchMaker(criteria);
                        await HandleCloseGroups(result, stoppingToken);
                    }

                    if (minutesTaskResetCriteria.Elapsed >= intervalToResetCriteria)
                    {
                        diffAverageAge = ModelConstants.DeafultCriteriaMatchMaker.differenceAverageAge;
                        diffPoints = ModelConstants.DeafultCriteriaMatchMaker.differencePoint;

                        minutesTaskResetCriteria.Restart();
                    }

                    await Task.Delay(interval, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro no RankMatchMakerBackgroundService.");
                }
            }

            logger.LogInformation("RankMatchMakerBackgroundService terminado.");
        }

        private async Task HandleCloseGroups(Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub> matchesMakes, CancellationToken cancellationToken)
        {
            if (matchesMakes == null || matchesMakes.Count == 0)
            {
                return;
            }

            foreach (var keyValue in matchesMakes)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                // Tratar da equipa da chave
                if (keyValue.Key != null)
                {
                    await ProcessEntry(keyValue.Key, cancellationToken);
                }

                // Tratar da equipa de valor
                if (keyValue.Value != null)
                {
                    await ProcessEntry(keyValue.Value, cancellationToken);
                }
            }
        }

        private async Task ProcessEntry(EntryRankMatchMakerHub entry, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var connId = entry.ConnectionId;
            var teamId = entry.Team.IdTeam;

            if (string.IsNullOrWhiteSpace(connId) || teamId == Guid.Empty)
            {
                return;
            }

            var groupName = ModelConstants.RankMatchMakerHubConst.PrefixGroupName + teamId;

            try
            {
                // Notifica o grupo enquanto ainda tem membros
                await hubContext.Clients.Group(groupName).OnGroupClosed(groupName);
                await hubContext.Groups.RemoveFromGroupAsync(connId, groupName);

                logger.LogInformation("Grupo {GroupName} fechado e connection {ConnId} removida.", groupName, connId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao fechar grupo {GroupName} para connection {ConnId}.", groupName, connId);
            }
        }
    }
}
