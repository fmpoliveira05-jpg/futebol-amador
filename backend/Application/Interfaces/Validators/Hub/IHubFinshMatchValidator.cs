namespace Application.Interfaces.Validators.Hub
{
    /// <summary>
    /// Contrato de Validador de Regras de Negócio específico para o processo de Finalização de Partida ([FinishMatchHub]).
    /// 
    /// Esta interface define a regra essencial de validação síncrona que garante o consenso entre os
    /// resultados submetidos pelos administradores das duas equipas.
    /// </summary>
    public interface IHubFinshMatchValidator
    {
        /// <summary>
        /// Valida se os resultados finais da partida submetidos por ambas as equipas são idênticos (coincidem).
        /// </summary>
        /// <remarks>
        /// O validador deve garantir que o valor não é nulo e que é verdadeiro.
        /// Se os resultados não coincidirem, lança uma exceção de argumento, impedindo a finalização do jogo.
        /// </remarks>
        /// <param name="isCoincide">Valor booleano que indica se os resultados submetidos coincidem.
        /// O valor não pode ser nulo.</param>
        /// <exception cref="System.ArgumentException">Lançada se o valor for nulo (<c>HasValue == false</c>) ou se o resultado for <c>false</c> (resultados divergentes).</exception>
        public void ValidateIsCoincide(bool? isCoincide);
    }
}