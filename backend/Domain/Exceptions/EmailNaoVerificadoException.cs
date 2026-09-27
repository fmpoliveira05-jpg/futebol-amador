namespace Domain.Exceptions
{
    /// <summary>
    /// As credenciais estão certas, mas o utilizador ainda não confirmou o e-mail. A API responde
    /// 403 com o código <c>email_nao_verificado</c>, para o cliente sugerir o reenvio da mensagem.
    /// </summary>
    /// <remarks>
    /// Só é lançada depois de a palavra-passe ser validada, por isso não revela se um e-mail existe.
    /// </remarks>
    public class EmailNaoVerificadoException : Exception
    {
        /// <summary>Código devolvido no campo <c>codigo</c> do ProblemDetails.</summary>
        public const string Codigo = "email_nao_verificado";

        public EmailNaoVerificadoException()
            : base("Confirma o teu e-mail antes de entrar. Se não recebeste a mensagem, pede para a reenviar.")
        {
        }
    }
}
