using Microsoft.AspNetCore.Mvc;

namespace Api.Seguranca
{
    /// <summary>
    /// Proteção CSRF para a sessão em cookies.
    /// </summary>
    /// <remarks>
    /// Um pedido que altera dados (<c>POST</c>, <c>PUT</c>, <c>PATCH</c>, <c>DELETE</c>), leva um cookie
    /// da sessão e não traz <c>Authorization</c> tem de ter:
    /// <list type="number">
    /// <item>o cabeçalho <c>X-Requested-With: FutebolAmador</c> — um formulário ou uma imagem de outro
    /// site não o consegue pôr, e um <c>fetch</c> com cabeçalhos próprios obriga a um preflight CORS,
    /// que só as origens autorizadas passam;</item>
    /// <item>um <c>Origin</c> da lista <c>Cors:Origins</c>.</item>
    /// </list>
    /// Os pedidos com <c>Authorization: Bearer</c> (app Android) não dependem de credenciais que o
    /// browser envie sozinho, por isso não precisam desta verificação.
    /// </remarks>
    public sealed class ProtecaoCsrf
    {
        private static readonly HashSet<string> MetodosSeguros =
            new(StringComparer.OrdinalIgnoreCase) { "GET", "HEAD", "OPTIONS", "TRACE" };

        private readonly RequestDelegate next;
        private readonly SessaoWeb sessaoWeb;
        private readonly ILogger<ProtecaoCsrf> logger;

        public ProtecaoCsrf(RequestDelegate next, SessaoWeb sessaoWeb, ILogger<ProtecaoCsrf> logger)
        {
            this.next = next;
            this.sessaoWeb = sessaoWeb;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (PrecisaDeVerificacao(context.Request) && !PedidoLegitimo(context.Request))
            {
                logger.LogWarning("Pedido recusado pela proteção CSRF em {Caminho}.", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Pedido recusado",
                    Detail = "O pedido não veio da aplicação web autorizada.",
                });
                return;
            }

            await next(context);
        }

        private static bool PrecisaDeVerificacao(HttpRequest request) =>
            !MetodosSeguros.Contains(request.Method) &&
            string.IsNullOrEmpty(request.Headers.Authorization) &&
            (request.Cookies.ContainsKey(SessaoWeb.CookieSessao) || request.Cookies.ContainsKey(SessaoWeb.CookieRefresh));

        private bool PedidoLegitimo(HttpRequest request) =>
            string.Equals(request.Headers[SessaoWeb.CabecalhoCsrf], SessaoWeb.ValorCsrf, StringComparison.Ordinal) &&
            sessaoWeb.OrigemPermitida(request.Headers.Origin);
    }
}
