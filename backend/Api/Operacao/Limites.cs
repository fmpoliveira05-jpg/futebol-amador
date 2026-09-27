using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Api.Operacao
{
    /// <summary>
    /// Tempos máximos e limites: Kestrel (cabeçalhos, corpo), duração de cada pedido à API e
    /// pedidos da API a serviços externos (Firebase REST, Turnstile, Cloudinary).
    /// </summary>
    /// <remarks>
    /// Valores por omissão sobrepostos pela secção <c>Limites</c> da configuração
    /// (por exemplo <c>Limites__TamanhoMaximoPedidoBytes=2097152</c>).
    /// </remarks>
    public static class Limites
    {
        /// <summary>Nome da política de timeout sem limite (hubs SignalR, ligações longas).</summary>
        public const string SemTimeout = "sem-timeout";

        /// <summary>Corpo máximo de um pedido: 1 MB (o maior é um emblema em base64, até ~150 KB).</summary>
        public const long TamanhoMaximoPedidoPredefinido = 1024 * 1024;

        public static void ConfigurarKestrel(this ConfigureWebHostBuilder webHost, IConfiguration configuracao)
        {
            webHost.ConfigureKestrel(opcoes =>
            {
                // Sem o cabeçalho "Server: Kestrel" (não se anuncia o servidor).
                opcoes.AddServerHeader = false;

                opcoes.Limits.MaxRequestBodySize = configuracao.GetValue("Limites:TamanhoMaximoPedidoBytes", TamanhoMaximoPedidoPredefinido);
                // Clientes lentos a enviar cabeçalhos (slowloris) são desligados.
                opcoes.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(configuracao.GetValue("Limites:TimeoutCabecalhosSegundos", 15));
                opcoes.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(configuracao.GetValue("Limites:KeepAliveSegundos", 120));
                opcoes.Limits.MaxRequestHeadersTotalSize = 32 * 1024;
                opcoes.Limits.MaxConcurrentUpgradedConnections = configuracao.GetValue<long?>("Limites:MaxLigacoesWebSocket", 1000);
            });
        }

        /// <summary>
        /// Cada pedido HTTP à API tem no máximo <c>Limites:TimeoutPedidoSegundos</c> (30 s) para
        /// responder; depois responde 503 e o trabalho é cancelado. Os hubs SignalR ficam de fora.
        /// </summary>
        public static IServiceCollection AddTimeoutsPedidos(this IServiceCollection services, IConfiguration configuracao)
        {
            services.AddRequestTimeouts(opcoes =>
            {
                opcoes.DefaultPolicy = new RequestTimeoutPolicy
                {
                    Timeout = TimeSpan.FromSeconds(configuracao.GetValue("Limites:TimeoutPedidoSegundos", 30)),
                    TimeoutStatusCode = StatusCodes.Status503ServiceUnavailable,
                };
                opcoes.AddPolicy(SemTimeout, new RequestTimeoutPolicy { Timeout = Timeout.InfiniteTimeSpan });
            });
            return services;
        }

        /// <summary>
        /// Resiliência dos pedidos a serviços externos (Microsoft.Extensions.Http.Resilience / Polly):
        /// tempo máximo por tentativa e total, disjuntor (circuit breaker) e limite de pedidos em curso.
        /// </summary>
        /// <remarks>
        /// As novas tentativas só se fazem em métodos idempotentes (GET, DELETE, ...): um POST ao
        /// Firebase (login, envio de e-mail) nunca é repetido, para não enviar duas mensagens.
        /// </remarks>
        public static IHttpClientBuilder AddResilienciaExterna(this IHttpClientBuilder cliente, IConfiguration configuracao)
        {
            var porTentativa = TimeSpan.FromSeconds(configuracao.GetValue("Limites:Externos:TimeoutTentativaSegundos", 10));
            var total = TimeSpan.FromSeconds(configuracao.GetValue("Limites:Externos:TimeoutTotalSegundos", 25));

            // O timeout do HttpClient fica acima do da resiliência, que é quem decide.
            cliente.ConfigureHttpClient(http => http.Timeout = total + TimeSpan.FromSeconds(5));

            cliente.AddStandardResilienceHandler(opcoes =>
            {
                opcoes.AttemptTimeout.Timeout = porTentativa;
                opcoes.TotalRequestTimeout.Timeout = total;
                opcoes.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(Math.Max(30, porTentativa.TotalSeconds * 2));
                opcoes.Retry.MaxRetryAttempts = 2;
                opcoes.Retry.BackoffType = DelayBackoffType.Exponential;
                opcoes.Retry.UseJitter = true;
                opcoes.Retry.DisableForUnsafeHttpMethods();
                opcoes.RateLimiter.DefaultRateLimiterOptions.PermitLimit = 200;
                opcoes.RateLimiter.DefaultRateLimiterOptions.QueueLimit = 0;
            });

            return cliente;
        }
    }
}
