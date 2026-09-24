namespace Domain.Exceptions
{
    /// <summary>
    /// Representa erros que ocorrem quando uma regra de negócio ou uma lógica de domínio da aplicação é violada.
    /// 
    /// Esta exceção é lançada na camada de Serviço/Domínio para sinalizar que a operação não pode ser concluída,
    /// mesmo que não haja falha de sistema, rede ou autenticação.
    /// Exemplo: Tentar agendar uma partida para uma data passada.
    /// </summary>
    public class BusinessRuleException : Exception
    {
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="BusinessRuleException"/>.
        /// Construtor padrão (sem argumentos).
        /// </summary>
        public BusinessRuleException()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="BusinessRuleException"/> com uma mensagem de erro especificada.
        /// </summary>
        /// <param name="message">A mensagem que descreve a violação da regra de negócio.</param>
        public BusinessRuleException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="BusinessRuleException"/> com uma mensagem de erro especificada
        /// e uma referência à exceção interna que causou a exceção atual.
        /// </summary>
        /// <param name="message">A mensagem que descreve a violação da regra de negócio.</param>
        /// <param name="innerException">A exceção que é a causa da exceção atual.</param>
        public BusinessRuleException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}