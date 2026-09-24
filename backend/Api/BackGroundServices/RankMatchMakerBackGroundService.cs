using Api.Hubs;
using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Hub;
using Application.Interfaces.Services.Hub;
using Domain.Constants;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace Application.Services.BackGroundServices
{
    /// <summary>
    /// Serviço de plano de fundo (Hosted Service) responsável pelo motor de Matchmaking de partidas ranqueadas.
    /// 
    /// Este serviço executa continuamente para emparelhar equipas que estão à espera no lobby ([RankMatchMakerHub]).
    /// Implementa um algoritmo de **relaxamento progressivo de critérios**: se não encontrar adversários ideais
    /// rapidamente, alarga gradualmente os limites de diferença de idade e pontos para facilitar o emparelhamento.
    /// </summary>
    public class RankMatchMakerBackGroundService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;

        /// <summary>
        /// Contexto do Hub SignalR utilizado para enviar notificações em tempo real aos clientes (equipas).
        /// </summary>        
        private readonly IHubContext<RankMatchMakerHub, IRankMatchMakerHub> hubContext;

        private readonly ILogger<RankMatchMakerBackGroundService> logger;

        /// <summary>
        /// Intervalo de tempo entre cada ciclo de execução do algoritmo de matchmaking (polling).
        /// </summary>
        private readonly TimeSpan interval = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Intervalo de tempo após o qual os critérios de emparelhamento são relaxados (tornam-se menos estritos).
        /// </summary>
        private readonly TimeSpan intervalToIncreaseCriteria = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Tempo máximo de permanência no lobby. Após este tempo, os critérios são reiniciados ou o processo expira.
        /// Definido em [ModelConstants.GeralTimeInHubConst.timeInMatchMackerHub].
        /// </summary>
        private readonly TimeSpan intervalToResetCriteria = TimeSpan.FromMinutes(ModelConstants.GeralTimeInHubConst.timeInMatchMackerHub);

        /// <summary>
        /// Construtor do serviço de background de Matchmaking.
        /// </summary>
        /// <param name="scopeFactory">Fábrica para criar escopos de serviço (necessário para injetar serviços Scoped).</param>
        /// <param name="hubContext">Contexto do Hub para comunicação SignalR.</param>
        /// <param name="logger">Logger para registo de atividades.</param>
        public RankMatchMakerBackGroundService(IServiceScopeFactory scopeFactory, IHubContext<RankMatchMakerHub, 
            IRankMatchMakerHub> hubContext, ILogger<RankMatchMakerBackGroundService> logger)
        {
            this.scopeFactory = scopeFactory;
            this.hubContext = hubContext;
            this.logger = logger;
        }

        /// <summary>
        /// Método principal de execução do serviço.
        /// </summary>
        /// <remarks>
        /// O loop infinito executa as seguintes etapas:
        /// 1. Verifica se é necessário relaxar os critérios de pesquisa (aumentar tolerância de idade/pontos).
        /// 2. Cria um escopo e invoca o [IManagerRankMatchMakerService] para tentar emparelhar as equipas em espera.
        /// 3. Processa os resultados (notifica e remove equipas emparelhadas do lobby).
        /// 4. Verifica se é necessário reiniciar os critérios (timeout).
        /// 5. Aguarda o intervalo definido antes da próxima iteração.
        /// </remarks>
        /// <param name="stoppingToken">Token de cancelamento para parar o serviço.</param>
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

        /// <summary>
        /// Processa os pares de equipas que foram emparelhados com sucesso.
        /// </summary>
        /// <param name="matchesMakes">Dicionário contendo os pares de equipas (Chave vs Valor).</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
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

        /// <summary>
        /// Finaliza a sessão de uma equipa específica no Hub de Matchmaking.
        /// </summary>
        /// <remarks>
        /// 1. Notifica o cliente via SignalR (`OnGroupClosed`) que o processo terminou (encontrou jogo).
        /// 2. Remove a conexão do grupo SignalR.
        /// </remarks>
        /// <param name="entry">Os dados da equipa/conexão a processar.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
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