using Domain.Exceptions; 
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Middlewares
{

    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Usa ProblemDetails para um formato de erro padrão (RFC 7807)
            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path
            };

            switch (exception)
            {
                case ValidationException ex:
                    problemDetails.Title = "Erro de Validação";
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Detail = ex.Message;
                    break;

                case NotFoundException ex:
                    problemDetails.Title = "Recurso Não Encontrado";
                    problemDetails.Status = (int)HttpStatusCode.NotFound;
                    problemDetails.Detail = ex.Message;
                    break;

                case MatchInviteException ex:
                    problemDetails.Title = "Problema no convite de partida";
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Detail = ex.Message;
                    break;

                default:
                    _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);

                    problemDetails.Title = "Erro Interno do Servidor";
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Detail = "Ocorreu um erro inesperado. Tente novamente mais tarde.";
                    break;
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            // Escreve o JSON de erro na resposta
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
