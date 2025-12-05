using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Hub;

namespace Api.BackGroundServices
{
    /// <summary>
    /// Serviço de plano de fundo (Hosted Service) responsável pelo envio automático de notificações de "Dia de Jogo".
    /// 
    /// Este serviço executa periodicamente para verificar se existem partidas agendadas para o dia atual
    /// e envia alertas para as equipas envolvidas através do [INotificationService] (SignalR).
    /// </summary>
    public class NotificationBackGroundService : BackgroundService
    {
        #region Initializer
        /// <summary>
        /// Fábrica de escopos de serviço. Necessária porque este BackgroundService é um Singleton,
        /// mas os Repositories e Services que ele utiliza são Scoped (por pedido).
        /// </summary>
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationBackGroundService> _logger;

        /// <summary>
        /// Construtor do serviço de notificações em background.
        /// </summary>
        /// <param name="scopeFactory">Fábrica para criar escopos isolados e resolver dependências Scoped.</param>
        /// <param name="logger">Logger para registar o início, paragem e envio de notificações.</param>
        public NotificationBackGroundService(IServiceScopeFactory scopeFactory, ILogger<NotificationBackGroundService> logger)
        {
            _scopeFactory = scopeFactory; 
            _logger = logger;        
        }
        #endregion

        /// <summary>
        /// Método principal de execução do serviço.
        /// Contém um loop infinito que executa a lógica de notificação e depois "dorme" por um intervalo definido.
        /// </summary>
        /// <remarks>
        /// O intervalo de execução está definido para **3 horas**.
        /// O padrão <c>using (var scope = _scopeFactory.CreateScope())</c> é utilizado para garantir que os serviços
        /// injetados são descartados corretamente após cada ciclo de execução.
        /// </remarks>
        /// <param name="stoppingToken">Token de cancelamento para parar o serviço graciosamente.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var matchRepository = scope.ServiceProvider.GetRequiredService<IMatchRepository>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        await CheckAndNotifyMatchesAsync(stoppingToken, matchRepository, notificationService);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending match notifications.");
                }

                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
            }

            _logger.LogInformation("Notification background service stopped.");
        }

        #region Private methods

        /// <summary>
        /// Lógica de negócio que consulta as partidas de hoje e envia as notificações.
        /// </summary>
        /// <param name="ct">Token de cancelamento.</param>
        /// <param name="matchRepository">Repositório para consultar jogos.</param>
        /// <param name="notificationService">Serviço (Hub) para enviar mensagens em tempo real.</param>
        private async Task CheckAndNotifyMatchesAsync( CancellationToken ct, IMatchRepository matchRepository, INotificationService notificationService)
        {
            var today = DateTime.UtcNow.Date;

            var matchesToday = await matchRepository.GetMatchesByDateAsync(today);

            foreach (var match in matchesToday)
            {
                if (ct.IsCancellationRequested) break;

                await notificationService.SendTeamAsync(
                    match.Team.IdTeam.ToString(),
                    "Game Day!",
                    $"You have a match today against {match.Opponent.Name} at {match.GameDate:t}"
                );

                await notificationService.SendTeamAsync(
                    match.Opponent.IdTeam.ToString(),
                    "Game Day!",
                    $"You have a match today against {match.Team.Name} at {match.GameDate:t}"
                );

                _logger.LogInformation($"Sent match notifications for match {match.IdMatch}");
            }
        }
        #endregion
    }
}