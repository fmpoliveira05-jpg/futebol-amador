using Application.Interfaces.Validators.Hub;

namespace Application.Validators.Hubs
{
    /// <summary>
    /// Validador de Regras de Negócio específico para o processo de Finalização de Partida (Finish Match Hub).
    /// 
    /// Esta classe verifica o requisito essencial de que os resultados submetidos por ambos os administradores
    /// das equipas devem ser idênticos.
    /// </summary>
    public class HubFinshMatchValidator : IHubFinshMatchValidator
    {
        /// <summary>
        /// Construtor padrão da classe [HubFinshMatchValidator].
        /// </summary>
        public HubFinshMatchValidator() { }

        /// <summary>
        /// Valida se os resultados finais da partida submetidos por ambas as equipas são idênticos (coincidem).
        /// </summary>
        /// <param name="isCoincide">Valor booleano que indica se os resultados submetidos coincidem.
        /// O valor não pode ser nulo.</param>
        /// <exception cref="ArgumentException">Lançada se o valor for nulo (HasValue == false), ou se o resultado não coincidir (regra de negócio).</exception>
        public void ValidateIsCoincide(bool? isCoincide)
        {
            if (!isCoincide.HasValue)
            {
                throw new ArgumentException("O resultado da partida introduzido pelas duas equipas não coincidem");
            }
        }
    }
}