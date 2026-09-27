using Microsoft.AspNetCore.Http;

namespace Api.Middlewares
{
    /// <summary>
    /// Personaliza os <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> gerados pela framework
    /// (rotas inexistentes, métodos não permitidos, 401/403 sem corpo, erros de validação).
    /// </summary>
    /// <remarks>
    /// O título e o detalhe ficam em português e nunca levam informação interna. Mantém-se só o
    /// <c>traceId</c>, que permite encontrar o pedido nos logs sem expor nada ao cliente.
    /// </remarks>
    public static class RespostasErro
    {
        /// <summary>Título e detalhe por código HTTP, para as respostas sem detalhe próprio.</summary>
        internal static readonly IReadOnlyDictionary<int, (string Titulo, string Detalhe)> Textos =
            new Dictionary<int, (string, string)>
            {
                [StatusCodes.Status400BadRequest] = ("Pedido inválido", "O pedido não pôde ser processado. Verifica os dados e tenta outra vez."),
                [StatusCodes.Status401Unauthorized] = ("Não autenticado", "É preciso iniciar sessão."),
                [StatusCodes.Status403Forbidden] = ("Sem permissão", "Não tens permissão para fazer isto."),
                [StatusCodes.Status404NotFound] = ("Recurso não encontrado", "O endereço pedido não existe."),
                [StatusCodes.Status405MethodNotAllowed] = ("Método não permitido", "Este endereço não aceita este método HTTP."),
                [StatusCodes.Status406NotAcceptable] = ("Formato não suportado", "A API só responde em JSON."),
                [StatusCodes.Status408RequestTimeout] = ("Pedido demorou demasiado", "O pedido demorou demasiado a chegar. Tenta outra vez."),
                [StatusCodes.Status409Conflict] = ("Conflito", "Os dados foram alterados entretanto. Atualiza a página e tenta outra vez."),
                [StatusCodes.Status413PayloadTooLarge] = ("Pedido demasiado grande", "O pedido excede o tamanho máximo permitido."),
                [StatusCodes.Status415UnsupportedMediaType] = ("Formato não suportado", "O corpo do pedido tem de ser JSON."),
                [StatusCodes.Status429TooManyRequests] = ("Demasiados pedidos", "Fizeste demasiados pedidos seguidos. Espera um pouco e tenta outra vez."),
                [StatusCodes.Status500InternalServerError] = ("Erro interno do servidor", "Ocorreu um erro inesperado. Tenta novamente mais tarde."),
                [StatusCodes.Status503ServiceUnavailable] = ("Serviço indisponível", "O serviço está temporariamente indisponível. Tenta mais tarde."),
            };

        /// <summary>Opções passadas a <c>AddProblemDetails</c>.</summary>
        public static void Configurar(ProblemDetailsOptions options)
        {
            options.CustomizeProblemDetails = contexto =>
            {
                var problema = contexto.ProblemDetails;
                var estado = problema.Status ?? contexto.HttpContext.Response.StatusCode;

                if (Textos.TryGetValue(estado, out var texto))
                {
                    // Os títulos da framework vêm em inglês ("Not Found"); os do GlobalExceptionHandler
                    // já estão em português e têm detalhe próprio, por isso ficam.
                    if (string.IsNullOrEmpty(problema.Detail))
                    {
                        problema.Title = texto.Titulo;
                        problema.Detail = texto.Detalhe;
                    }
                }

                // O "type" da framework aponta para o RFC; não acrescenta nada ao cliente.
                problema.Type = null;
                problema.Instance ??= contexto.HttpContext.Request.Path;
                problema.Extensions.Remove("exception");
                problema.Extensions.TryAdd("traceId", System.Diagnostics.Activity.Current?.Id ?? contexto.HttpContext.TraceIdentifier);
            };
        }
    }
}
