using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Middlewares
{
    /// <summary>
    /// Middleware global de tratamento de exceções.
    /// 
    /// Esta classe implementa a interface [IExceptionHandler] e é responsável por interceptar
    /// qualquer exceção não tratada que ocorra durante o processamento de um pedido HTTP.
    /// Converte a exceção numa resposta JSON padronizada (RFC 7807 - ProblemDetails),
    /// mapeando tipos de exceção específicos para códigos de estado HTTP apropriados.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> Logger;

        /// <summary>
        /// Construtor do GlobalExceptionHandler.
        /// </summary>
        /// <param name="logger">O serviço de logging para registar erros não tratados.</param>
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Tenta tratar a exceção ocorrida durante o pipeline do pedido.
        /// </summary>
        /// <remarks>
        /// Mapeia exceções de domínio ([ValidationException], [NotFoundException], etc.) para
        /// códigos HTTP 4xx (erros do cliente) e exceções genéricas para 500 (erro do servidor).
        /// </remarks>
        /// <param name="httpContext">O contexto HTTP atual.</param>
        /// <param name="exception">A exceção que foi lançada.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns><c>true</c> se a exceção foi tratada com sucesso (o que é sempre o caso aqui), <c>false</c> caso contrário.</returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // Usa ProblemDetails para um formato de erro padrão (RFC 7807)
            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path
            };

            switch (exception)
            {
                case ValidationException ex:
                {
                    problemDetails.Title = "Erro de Validação";
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Detail = ex.Message;
                    break;
                }
                case NotFoundException ex:
                {
                    problemDetails.Title = "Recurso Não Encontrado";
                    problemDetails.Status = (int)HttpStatusCode.NotFound;
                    problemDetails.Detail = ex.Message;
                    break;
                }
                case InvalidOperationException ex:
                {
                    problemDetails.Title = "Operação Inválida";
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Detail = ex.Message;
                    break;
                }
                case ArgumentException ex:
                {
                    problemDetails.Title = "Argumento Inválido";
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Detail = ex.Message;
                    break;
                }
                case BusinessRuleException ex:
                {
                    problemDetails.Title = "Problema no não cumprimento de regra de negocio";
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Detail = ex.Message;
                    break;
                }
                case MatchInviteException ex:
                {
                    problemDetails.Title = "Problema no convite de partida";
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Detail = ex.Message;
                    break;
                }
                case UnauthorizedAccessException ex:
                {
                    problemDetails.Title = "Acesso Não Autorizado";
                    problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                    problemDetails.Detail = ex.Message;
                    break;
                }
                case AuthenticationException ex:
                {
                    problemDetails.Title = "Erro na autenticação";
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Detail = ex.Message;
                    break;
                }
                case Exception ex:
                {
                    problemDetails.Title = "Erro Geral";
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Detail = ex.Message;
                    break;
                }
                default:
                {
                    Logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);

                    problemDetails.Title = "Erro Interno do Servidor";
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Detail = "Ocorreu um erro inesperado. Tente novamente mais tarde.";
                    break;
                }
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            // Escreve o JSON de erro na resposta
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}