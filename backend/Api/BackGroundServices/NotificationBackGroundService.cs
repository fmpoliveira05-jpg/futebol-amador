
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Hub;

namespace Api.BackGroundServices
{
    public class NotificationBackGroundService : BackgroundService
    {
        private readonly IMatchRepository matchRepository;
        private readonly INotificationService notificationService;
        private readonly ILogger<NotificationBackGroundService> logger;

        public NotificationBackGroundService(
            IMatchRepository matchRepository, 
            INotificationService notificationService,
            ILogger<NotificationBackGroundService> logger)
        {
            this.matchRepository = matchRepository;
            this.notificationService = notificationService;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Notification background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndNotifyMatchesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while sending match notifications.");
                }

                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
            }
        }

        #region Private methods
        private async Task CheckAndNotifyMatchesAsync(CancellationToken ct)
        {
            var today = DateTime.UtcNow.Date;

            var matchesToday = await matchRepository.GetMatchesByDateAsync(today);

            foreach (var match in matchesToday)
            {
                await notificationService.SendTeamAsync(
                    match.Team.IdTeam.ToString(),
                    "Game Day!",
                    $"You have a match today against {match.Opponent.Name} at {match.GameDate:t}."
                );

                await notificationService.SendTeamAsync(
                    match.Opponent.IdTeam.ToString(),
                    "Game Day!",
                    $"You have a match today against {match.Team.Name} at {match.GameDate:t}."
                );

                logger.LogInformation($"Sent match notifications for match {match.IdMatch}");
            }
        }
        #endregion
    }
}
