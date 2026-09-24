namespace Domain.Exceptions
{
    /// <summary>
    /// Representa erros que ocorrem quando a validação de dados ou regras de domínio falha no processo de validação dedicado.
    /// 
    /// Esta exceção é lançada na camada de Serviço/Domínio após a execução de um objeto Validador (Validator)
    /// que detetou dados inválidos ou inconsistentes.
    /// Exemplo: Tentar criar uma entidade com um campo obrigatório em falta.
    /// </summary>
    public class ValidatorException : Exception
    {
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="ValidatorException"/>.
        /// Construtor padrão (sem argumentos).
        /// </summary>
        public ValidatorException()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="ValidatorException"/> com uma mensagem de erro especificada.
        /// </summary>
        /// <param name="message">A mensagem que descreve a falha de validação.</param>
        public ValidatorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="ValidatorException"/> com uma mensagem de erro especificada
        /// e uma referência à exceção interna que causou a exceção atual.
        /// </summary>
        /// <param name="message">A mensagem que descreve o erro.</param>
        /// <param name="innerException">A exceção que é a causa da exceção atual.</param>
        public ValidatorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}