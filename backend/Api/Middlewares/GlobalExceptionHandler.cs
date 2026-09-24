using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Middlewares
{
    /// <summary>
    /// Converte as exceções em respostas <see cref="ProblemDetails"/> (RFC 7807) com o código HTTP
    /// certo. Está ligado ao pipeline por <c>app.UseExceptionHandler()</c> no Program.cs.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>400 — regras de negócio e validações (<see cref="ValidationException"/>,
    /// <see cref="BusinessRuleException"/>, <see cref="InvalidOperationException"/>, ...).</item>
    /// <item>401 — pedido sem utilizador identificado (<see cref="UnauthorizedAccessException"/>).</item>
    /// <item>403 — utilizador sem permissão (<see cref="ForbiddenException"/>).</item>
    /// <item>404 — recurso inexistente (<see cref="NotFoundException"/>).</item>
    /// <item>500 — tudo o resto. A mensagem original fica só no log, para não expor detalhes
    /// internos (SQL, caminhos, stack traces) ao cliente.</item>
    /// </list>
    /// </remarks>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> logger;
        private readonly IProblemDetailsService problemDetailsService;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService)
        {
            this.logger = logger;
            this.problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, titulo) = Classificar(exception);

            string detalhe;
            if (status == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Erro não tratado em {Metodo} {Caminho}", httpContext.Request.Method, httpContext.Request.Path);
                detalhe = "Ocorreu um erro inesperado. Tenta novamente mais tarde.";
            }
            else
            {
                logger.LogInformation("Pedido recusado ({Estado}) em {Caminho}: {Mensagem}", status, httpContext.Request.Path, exception.Message);
                detalhe = exception.Message;
            }

            httpContext.Response.StatusCode = status;

            var problema = new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = detalhe,
                Instance = httpContext.Request.Path,
            };

            var escrito = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problema,
            });

            if (!escrito)
            {
                await httpContext.Response.WriteAsJsonAsync(problema, cancellationToken);
            }

            return true;
        }

        /// <summary>Código HTTP e título para cada tipo de exceção.</summary>
        internal static (int Status, string Titulo) Classificar(Exception exception) => exception switch
        {
            ForbiddenException => (StatusCodes.Status403Forbidden, "Sem permissão"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Não autenticado"),
            NotFoundException or NotFindException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            ValidationException or ValidatorException => (StatusCodes.Status400BadRequest, "Erro de validação"),
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Regra de negócio não cumprida"),
            MatchInviteException => (StatusCodes.Status400BadRequest, "Problema no convite de jogo"),
            MatchException => (StatusCodes.Status400BadRequest, "Problema no jogo"),
            EmptyCollectionException => (StatusCodes.Status400BadRequest, "Pedido inválido"),
            AuthenticationException => (StatusCodes.Status400BadRequest, "Erro na autenticação"),
            // ArgumentNullException é um ArgumentException, por isso fica também aqui.
            ArgumentException => (StatusCodes.Status400BadRequest, "Argumento inválido"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Operação inválida"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor"),
        };
    }
}
