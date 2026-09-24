namespace Domain.Exceptions
{
    /// <summary>
    /// Representa erros que ocorrem durante o processo de autenticação ou autorização de um utilizador.
    /// 
    /// Esta é uma classe de exceção personalizada que herda de [Exception] e é utilizada na camada de Serviço
    /// para sinalizar falhas de credenciais (senha/email incorretos) ou problemas de acesso.
    /// </summary>
    public class AuthenticationException : Exception
    {
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="AuthenticationException"/>.
        /// Construtor padrão (sem argumentos).
        /// </summary>
        public AuthenticationException()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="AuthenticationException"/> com uma mensagem de erro especificada.
        /// </summary>
        /// <param name="message">A mensagem que descreve o erro.</param>
        public AuthenticationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="AuthenticationException"/> com uma mensagem de erro especificada
        /// e uma referência à exceção interna que causou esta exceção.
        /// </summary>
        /// <param name="message">A mensagem que descreve o erro.</param>
        /// <param name="innerException">A exceção que é a causa da exceção atual.</param>
        public AuthenticationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}