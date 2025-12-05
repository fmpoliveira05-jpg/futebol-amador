namespace Domain.Exceptions
{
    /// <summary>
    /// Representa erros que ocorrem especificamente durante o ciclo de vida de um convite de partida (Match Invite).
    /// 
    /// Esta exceção é lançada na camada de Serviço/Domínio para sinalizar problemas relacionados com a validade,
    /// estado, ou regras de negócio de um convite (ex: tentar negociar um convite já aceite ou rejeitado).
    /// </summary>
    public class MatchInviteException : Exception
    {
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="MatchInviteException"/>.
        /// Construtor padrão (sem argumentos).
        /// </summary>
        public MatchInviteException()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="MatchInviteException"/> com uma mensagem de erro especificada.
        /// </summary>
        /// <param name="message">A mensagem que descreve o erro de validação ou estado do convite.</param>
        public MatchInviteException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="MatchInviteException"/> com uma mensagem de erro especificada
        /// e uma referência à exceção interna que causou a exceção atual.
        /// </summary>
        /// <param name="message">A mensagem que descreve o erro.</param>
        /// <param name="innerException">A exceção que é a causa da exceção atual.</param>
        public MatchInviteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}