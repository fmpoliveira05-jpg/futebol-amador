namespace Domain.Exceptions
{
    /// <summary>
    /// O utilizador está autenticado mas não tem permissão para a operação (por exemplo, não é
    /// administrador da equipa ou não é o dono do recurso). A API responde 403.
    /// </summary>
    /// <remarks>
    /// Distingue-se de <see cref="UnauthorizedAccessException"/>, que significa "sem sessão" (401).
    /// A diferença importa para os clientes: um 401 faz o frontend terminar a sessão.
    /// </remarks>
    public class ForbiddenException : Exception
    {
        public ForbiddenException()
        {
        }

        public ForbiddenException(string message)
            : base(message)
        {
        }

        public ForbiddenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
