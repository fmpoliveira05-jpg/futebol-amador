using Application.Interfaces.Services;

namespace Api.BackGroundServices
{
    /// <summary>
    /// Tarefas periódicas das ligas (a cada 5 minutos):
    /// <list type="bullet">
    /// <item>preenche os onzes que ficaram por definir depois do prazo;</item>
    /// <item>sorteia o calendário das épocas que começam na próxima semana;</item>
    /// <item>fecha as épocas que chegaram à data de fim (troféu, subidas e descidas).</item>
    /// </list>
    /// </summary>
    public class CompetitionBackGroundService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<CompetitionBackGroundService> logger;

        public CompetitionBackGroundService(IServiceScopeFactory scopeFactory, ILogger<CompetitionBackGroundService> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);
            do
            {
                await RunOnceAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        /// <summary>Uma volta das tarefas (exposto para os testes).</summary>
        public async Task RunOnceAsync(CancellationToken ct)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var lineups = scope.ServiceProvider.GetRequiredService<IMatchDetailsService>();
                var filled = await lineups.AutoFillMissingLineupsAsync(ct);

                using var scope2 = scopeFactory.CreateScope();
                var leagues = scope2.ServiceProvider.GetRequiredService<ILeagueService>();
                var seasons = await leagues.RunScheduledTasksAsync(ct);

                if (filled + seasons > 0)
                {
                    logger.LogInformation("Ligas: {Onzes} onzes preenchidos, {Epocas} épocas sorteadas ou fechadas.", filled, seasons);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                // Uma falha (por exemplo, a base de dados em baixo) não pode parar o serviço.
                logger.LogError(ex, "Falha nas tarefas periódicas das ligas.");
            }
        }
    }
}
