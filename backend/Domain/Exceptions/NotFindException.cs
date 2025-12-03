namespace Domain.Exceptions
{
    /// <summary>
    /// Representa erros que ocorrem quando uma entidade ou recurso específico procurado na base de dados ou no domínio não é encontrado.
    /// 
    /// Esta exceção é lançada na camada de Repositório/Serviço e geralmente mapeia para um código de erro HTTP 404 (Not Found) na camada de API.
    /// Exemplo: Tentar obter um jogador por ID que não existe.
    /// </summary>
    public class NotFindException : Exception
    {
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="NotFindException"/>.
        /// Construtor padrão (sem argumentos).
        /// </summary>
        public NotFindException()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="NotFindException"/> com uma mensagem de erro especificada.
        /// </summary>
        /// <param name="message">A mensagem que descreve o recurso que não foi encontrado (ex: "Equipa com ID X não encontrada.").</param>
        public NotFindException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="NotFindException"/> com uma mensagem de erro especificada
        /// e uma referência à exceção interna que causou a exceção atual.
        /// </summary>
        /// <param name="message">A mensagem que descreve o erro.</param>
        /// <param name="innerException">A exceção que é a causa da exceção atual.</param>
        public NotFindException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}