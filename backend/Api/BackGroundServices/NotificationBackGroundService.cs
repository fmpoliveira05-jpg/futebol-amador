using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Hub;
using Microsoft.Extensions.DependencyInjection; // <-- Adicione este using
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.BackGroundServices
{
    public class NotificationBackGroundService : BackgroundService
    {
        // 1. Apenas mantenha dependências Singleton (Logger e ScopeFactory)
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationBackGroundService> _logger;

        // 2. Injete o IServiceScopeFactory em vez dos seus repositórios/serviços Scoped
        public NotificationBackGroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationBackGroundService> logger)
        {
            _scopeFactory = scopeFactory; // Correção: Atribui ao field da classe
            _logger = logger;         // Correção: Atribui ao field da classe
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification background service started."); // Descomentado

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 3. Crie um "scope" manual para esta execução
                    //    Tudo o que for criado aqui dentro será destruído no fim do 'using'
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        // 4. Resolva os seus serviços Scoped *dentro* deste scope
                        var matchRepository = scope.ServiceProvider.GetRequiredService<IMatchRepository>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        // 5. Execute a sua lógica (o código do seu antigo método 'CheckAndNotifyMatchesAsync')
                        await CheckAndNotifyMatchesAsync(stoppingToken, matchRepository, notificationService);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending match notifications.");
                }

                // 6. Espere pelo próximo ciclo
                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
            }

            _logger.LogInformation("Notification background service stopped.");
        }

        #region Private methods

        // O método agora recebe as dependências como parâmetros, 
        // pois elas são resolvidas dentro do scope em ExecuteAsync
        private async Task CheckAndNotifyMatchesAsync(
            CancellationToken ct,
            IMatchRepository matchRepository,
            INotificationService notificationService)
        {
            var today = DateTime.UtcNow.Date;

            // Use o 'matchRepository' que foi passado como parâmetro
            var matchesToday = await matchRepository.GetMatchesByDateAsync(today);

            foreach (var match in matchesToday)
            {
                if (ct.IsCancellationRequested) break;

                // Use o 'notificationService' que foi passado como parâmetro
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