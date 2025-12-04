namespace Domain.Exceptions
{
    /// <summary>
    /// Representa erros que ocorrem quando uma operação esperava uma coleção, lista ou resultado que não fosse vazio, mas recebeu uma coleção com zero elementos.
    /// 
    /// Esta exceção é útil na camada de Serviço/Domínio para sinalizar à camada de apresentação que não há dados a serem processados.
    /// Exemplo: Tentar obter o primeiro elemento de uma lista vazia.
    /// </summary>
    public class EmptyCollectionException : Exception
    {
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="EmptyCollectionException"/>.
        /// Construtor padrão (sem argumentos).
        /// </summary>
        public EmptyCollectionException()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="EmptyCollectionException"/> com uma mensagem de erro especificada.
        /// </summary>
        /// <param name="message">A mensagem que descreve o erro (ex: "A lista de membros não pode estar vazia.").</param>
        public EmptyCollectionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="EmptyCollectionException"/> com uma mensagem de erro especificada
        /// e uma referência à exceção interna que causou a exceção atual.
        /// </summary>
        /// <param name="message">A mensagem que descreve o erro.</param>
        /// <param name="innerException">A exceção que é a causa da exceção atual.</param>
        public EmptyCollectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}