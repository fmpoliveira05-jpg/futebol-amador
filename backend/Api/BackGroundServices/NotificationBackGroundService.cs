using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Hub;

namespace Api.BackGroundServices
{
    public class NotificationBackGroundService : BackgroundService
    {
        #region Initializer
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationBackGroundService> _logger;

        public NotificationBackGroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationBackGroundService> logger)
        {
            _scopeFactory = scopeFactory; 
            _logger = logger;        
        }
        #endregion

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

        private async Task CheckAndNotifyMatchesAsync(
            CancellationToken ct,
            IMatchRepository matchRepository,
            INotificationService notificationService)
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