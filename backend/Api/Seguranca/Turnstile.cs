using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json.Serialization;

namespace Api.Seguranca
{
    /// <summary>
    /// Verificação do Cloudflare Turnstile (proteção contra robôs) no servidor.
    /// </summary>
    /// <remarks>
    /// Desligado enquanto <c>Turnstile:SecretKey</c> estiver vazio (desenvolvimento e testes).
    /// </remarks>
    public interface IVerificadorTurnstile
    {
        /// <summary>Há chave secreta configurada.</summary>
        bool Ativo { get; }

        /// <summary>Confirma o token do widget na API do Cloudflare.</summary>
        Task<bool> VerificarAsync(string token, string? ipCliente, CancellationToken cancellationToken = default);
    }

    /// <summary>Implementação com <c>challenges.cloudflare.com/turnstile/v0/siteverify</c>.</summary>
    public sealed class VerificadorTurnstile : IVerificadorTurnstile
    {
        private const string UrlVerificacao = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

        private readonly HttpClient http;
        private readonly ILogger<VerificadorTurnstile> logger;
        private readonly string? chaveSecreta;

        public VerificadorTurnstile(HttpClient http, IConfiguration configuration, ILogger<VerificadorTurnstile> logger)
        {
            this.http = http;
            this.logger = logger;
            chaveSecreta = configuration["Turnstile:SecretKey"];
        }

        public bool Ativo => !string.IsNullOrWhiteSpace(chaveSecreta);

        public async Task<bool> VerificarAsync(string token, string? ipCliente, CancellationToken cancellationToken = default)
        {
            if (!Ativo)
            {
                return true;
            }

            var campos = new Dictionary<string, string>
            {
                ["secret"] = chaveSecreta!,
                ["response"] = token,
            };
            if (!string.IsNullOrEmpty(ipCliente))
            {
                campos["remoteip"] = ipCliente;
            }

            try
            {
                var resposta = await http.PostAsync(UrlVerificacao, new FormUrlEncodedContent(campos), cancellationToken);
                var resultado = await resposta.Content.ReadFromJsonAsync<RespostaTurnstile>(cancellationToken: cancellationToken);
                if (resultado?.Sucesso != true)
                {
                    logger.LogInformation("Turnstile recusou o pedido ({Erros}).", string.Join(",", resultado?.Erros ?? Array.Empty<string>()));
                }
                return resultado?.Sucesso == true;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException or Polly.ExecutionRejectedException)
            {
                // Falha fechada: sem confirmação, o formulário web não passa (a app Android não usa Turnstile).
                logger.LogWarning("Não foi possível contactar o Turnstile ({Tipo}).", ex.GetType().Name);
                return false;
            }
        }

        private sealed class RespostaTurnstile
        {
            [JsonPropertyName("success")] public bool Sucesso { get; set; }
            [JsonPropertyName("error-codes")] public string[]? Erros { get; set; }
        }
    }

    /// <summary>
    /// Exige um token Turnstile válido no cabeçalho <c>X-Turnstile-Token</c> nos pedidos do frontend
    /// web (com <c>Origin</c>). Usado no login, no registo, no reenvio da confirmação do e-mail e na
    /// recuperação da palavra-passe.
    /// </summary>
    /// <remarks>
    /// A app Android não tem Turnstile (o equivalente móvel é o Firebase App Check / Play Integrity) e
    /// não envia <c>Origin</c>, por isso não é verificada aqui: nesses pedidos a proteção é a limitação
    /// de pedidos por IP. Ver docs/SEGURANCA.md.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class ExigirTurnstileAttribute : Attribute, IAsyncActionFilter
    {
        public const string Cabecalho = "X-Turnstile-Token";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;
            var verificador = http.RequestServices.GetRequiredService<IVerificadorTurnstile>();

            if (verificador.Ativo && !string.IsNullOrEmpty(http.Request.Headers.Origin))
            {
                var token = http.Request.Headers[Cabecalho].ToString();
                var valido = !string.IsNullOrWhiteSpace(token) && token.Length <= 2048 &&
                             await verificador.VerificarAsync(token, LimitacaoPedidos.EnderecoCliente(http), http.RequestAborted);

                if (!valido)
                {
                    context.Result = new ObjectResult(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Verificação falhou",
                        Detail = "Não foi possível confirmar que não és um robô. Recarrega a página e tenta outra vez.",
                    })
                    { StatusCode = StatusCodes.Status400BadRequest };
                    return;
                }
            }

            await next();
        }
    }
}
