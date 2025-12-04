using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>
    /// Contrato de Repositório para operações de acesso a dados da entidade [Pitch] (Campo de Jogo).
    /// 
    /// Define os métodos de consulta necessários para buscar locais de jogo por ID ou por nome.
    /// </summary>
    public interface IPitchRepository
    {
        /// <summary>
        /// Obtém um Campo de Jogo ([Pitch]) pelo seu identificador único (ID).
        /// </summary>
        /// <param name="id">O ID (GUID) do campo a procurar.</param>
        /// <returns>A entidade [Pitch] correspondente ou null se não for encontrada.</returns>
        public Task<Pitch?> GetPitchById(Guid id);

        /// <summary>
        /// Obtém um Campo de Jogo ([Pitch]) pelo seu nome de forma assíncrona.
        /// </summary>
        /// <remarks>
        /// A consulta utiliza a correspondência exata no nome do campo.
        /// </remarks>
        /// <param name="namePitch">O nome completo do campo de jogo.</param>
        /// <returns>A entidade [Pitch] correspondente ou null se não for encontrada.</returns>
        public Task<Pitch?> GetPitchByName(string namePitch);
    }
}