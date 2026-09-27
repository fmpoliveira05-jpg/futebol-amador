using Application.Interfaces.Services;

namespace Api.BackGroundServices
{
    /// <summary>
    /// Limitação da conservação (RGPD, art. 5.º, n.º 1, alínea e): uma vez por dia apaga contas que
    /// nunca confirmaram o e-mail, pedidos de adesão e convites de jogo antigos (ver docs/RGPD.md).
    /// </summary>
    /// <remarks>Desliga-se com <c>Rgpd:RetencaoAtiva=false</c>.</remarks>
    public sealed class RetencaoDadosBackGroundService : BackgroundService
    {
        private readonly IServiceScopeFactory scopes;
        private readonly ILogger<RetencaoDadosBackGroundService> logger;
        private readonly TimeProvider relogio;
        private readonly bool ativa;

        public RetencaoDadosBackGroundService(IServiceScopeFactory scopes, ILogger<RetencaoDadosBackGroundService> logger,
            TimeProvider relogio, IConfiguration configuracao)
        {
            this.scopes = scopes;
            this.logger = logger;
            this.relogio = relogio;
            ativa = configuracao.GetValue("Rgpd:RetencaoAtiva", true);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!ativa)
            {
                return;
            }

            // Primeira execução uns minutos depois do arranque, depois a cada 24 horas.
            using var temporizador = new PeriodicTimer(TimeSpan.FromHours(24));
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                do
                {
                    try
                    {
                        using var scope = scopes.CreateScope();
                        var contas = scope.ServiceProvider.GetRequiredService<IContaService>();
                        var (apagadas, pedidos, convites) = await contas.AplicarRetencaoAsync(relogio.GetUtcNow().UtcDateTime);
                        logger.LogInformation("Retenção: {Contas} contas por confirmar, {Pedidos} pedidos de adesão e {Convites} convites apagados.",
                            apagadas, pedidos, convites);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogError(ex, "Falha na limpeza periódica dos dados (retenção).");
                    }
                }
                while (await temporizador.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
                // A API está a parar.
            }
        }
    }
}
