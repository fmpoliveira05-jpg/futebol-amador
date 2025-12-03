namespace Domain.Exceptions
{
    /// <summary>
    /// Representa erros que ocorrem quando uma entidade ou recurso específico procurado na base de dados ou no domínio não é encontrado.
    /// 
    /// Esta exceção é lançada na camada de Repositório/Serviço para sinalizar a ausência de um recurso
    /// (corresponde ao código de status HTTP 404 - Not Found).
    /// Exemplo: Tentar obter um objeto por um ID que não existe.
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="NotFoundException"/>.
        /// Construtor padrão (sem argumentos).
        /// </summary>
        public NotFoundException()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="NotFoundException"/> com uma mensagem de erro especificada.
        /// </summary>
        /// <param name="message">A mensagem que descreve o recurso que não foi encontrado (ex: "O perfil de equipa não foi encontrado.").</param>
        public NotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="NotFoundException"/> com uma mensagem de erro especificada
        /// e uma referência à exceção interna que causou a exceção atual.
        /// </summary>
        /// <param name="message">A mensagem que descreve o erro.</param>
        /// <param name="innerException">A exceção que é a causa da exceção atual.</param>
        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}