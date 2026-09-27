using Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace Api.Operacao
{
    /// <summary>
    /// Verificações de saúde para a monitorização de disponibilidade (UptimeRobot, Better Stack,
    /// workflow <c>uptime.yml</c>, orquestradores de contentores).
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>GET /health/live</c> — o processo responde (sem dependências). Serve de
    /// <em>liveness probe</em>: se falhar, reinicia-se o contentor.</item>
    /// <item><c>GET /health/ready</c> — a base de dados responde e a configuração do Firebase está
    /// presente. Serve de <em>readiness probe</em> e para o monitor externo.</item>
    /// </list>
    /// Os dois são anónimos, não contam para a limitação de pedidos e não revelam detalhes: só o
    /// estado de cada verificação (sem mensagens de exceção, nomes de servidores ou caminhos).
    /// </remarks>
    public static class Saude
    {
        public const string CaminhoLive = "/health/live";
        public const string CaminhoReady = "/health/ready";
        private const string EtiquetaReady = "ready";

        public static IServiceCollection AddSaude(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<BaseDadosHealthCheck>("base-dados", tags: new[] { EtiquetaReady })
                .AddCheck<ConfiguracaoFirebaseHealthCheck>("firebase-configuracao", tags: new[] { EtiquetaReady });

            return services;
        }

        public static IEndpointRouteBuilder MapSaude(this IEndpointRouteBuilder app)
        {
            app.MapHealthChecks(CaminhoLive, new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = EscreverAsync,
            }).AllowAnonymous().DisableRateLimiting();

            app.MapHealthChecks(CaminhoReady, new HealthCheckOptions
            {
                Predicate = c => c.Tags.Contains(EtiquetaReady),
                ResponseWriter = EscreverAsync,
            }).AllowAnonymous().DisableRateLimiting();

            return app;
        }

        /// <summary>JSON curto: estado geral e de cada verificação, sem descrições nem exceções.</summary>
        internal static Task EscreverAsync(HttpContext contexto, HealthReport relatorio)
        {
            contexto.Response.ContentType = "application/json; charset=utf-8";
            contexto.Response.Headers.CacheControl = "no-store";

            var corpo = new
            {
                estado = relatorio.Status.ToString(),
                duracaoMs = (int)relatorio.TotalDuration.TotalMilliseconds,
                verificacoes = relatorio.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString()),
            };

            return contexto.Response.WriteAsync(JsonSerializer.Serialize(corpo));
        }
    }

    /// <summary>A base de dados aceita ligações (com tempo máximo de 5 segundos).</summary>
    public sealed class BaseDadosHealthCheck : IHealthCheck
    {
        private readonly AmateurFootballContext db;

        public BaseDadosHealthCheck(AmateurFootballContext db) => this.db = db;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using var limite = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            limite.CancelAfter(TimeSpan.FromSeconds(5));
            try
            {
                return await db.Database.CanConnectAsync(limite.Token)
                    ? HealthCheckResult.Healthy()
                    : HealthCheckResult.Unhealthy();
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                // A exceção fica nos logs do HealthCheckService, não na resposta.
                return HealthCheckResult.Unhealthy(exception: ex);
            }
        }
    }

    /// <summary>
    /// A configuração do Firebase está presente (projeto e ficheiro de credenciais). Não contacta a
    /// Google: uma falha da Google não deve tirar a API do balanceador.
    /// </summary>
    public sealed class ConfiguracaoFirebaseHealthCheck : IHealthCheck
    {
        private readonly IConfiguration configuracao;
        private readonly IWebHostEnvironment ambiente;

        public ConfiguracaoFirebaseHealthCheck(IConfiguration configuracao, IWebHostEnvironment ambiente)
        {
            this.configuracao = configuracao;
            this.ambiente = ambiente;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // Nos testes de integração o Firebase é substituído por mocks.
            if (ambiente.IsEnvironment("Testing"))
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }

            var projeto = configuracao["Firebase:ProjectId"];
            var credenciais = configuracao["Firebase:CredentialPath"];
            var ok = !string.IsNullOrWhiteSpace(projeto) && !string.IsNullOrWhiteSpace(credenciais) && File.Exists(credenciais);

            return Task.FromResult(ok ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
        }
    }
}
