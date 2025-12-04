namespace Domain.Exceptions
{
    /// <summary>
    /// Representa erros que ocorrem quando a validação de dados falha.
    /// 
    /// Esta exceção é lançada na camada de Domínio/Serviço quando os dados de entrada (input)
    /// não cumprem as regras de formato, comprimento ou intervalo (range) esperadas.
    /// Geralmente mapeia para um código de erro HTTP 400 (Bad Request).
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="ValidationException"/>.
        /// Construtor padrão (sem argumentos).
        /// </summary>
        public ValidationException()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="ValidationException"/> com uma mensagem de erro especificada.
        /// </summary>
        /// <param name="message">A mensagem que descreve o erro de validação (ex: "O nome excede o limite máximo de 50 caracteres.").</param>
        public ValidationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="ValidationException"/> com uma mensagem de erro especificada
        /// e uma referência à exceção interna que causou a exceção atual.
        /// </summary>
        /// <param name="message">A mensagem que descreve o erro.</param>
        /// <param name="innerException">A exceção que é a causa da exceção atual.</param>
        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}