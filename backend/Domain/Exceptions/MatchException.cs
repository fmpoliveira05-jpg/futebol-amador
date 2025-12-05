namespace Domain.Exceptions
{
    /// <summary>
    /// Representa erros que ocorrem especificamente durante as operações de gestão de partidas (Match).
    /// 
    /// Esta exceção é lançada na camada de Serviço/Domínio para sinalizar problemas relacionados com o ciclo de vida,
    /// estado (Status), ou validade de um jogo (ex: tentar iniciar um jogo já finalizado, ou aceitar um convite inválido).
    /// </summary>
    public class MatchException : Exception
    {
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="MatchException"/>.
        /// Construtor padrão (sem argumentos).
        /// </summary>
        public MatchException()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="MatchException"/> com uma mensagem de erro especificada.
        /// </summary>
        /// <param name="message">A mensagem que descreve o erro de validação ou estado da partida.</param>
        public MatchException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="MatchException"/> com uma mensagem de erro especificada
        /// e uma referência à exceção interna que causou a exceção atual.
        /// </summary>
        /// <param name="message">A mensagem que descreve o erro.</param>
        /// <param name="innerException">A exceção que é a causa da exceção atual.</param>
        public MatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}