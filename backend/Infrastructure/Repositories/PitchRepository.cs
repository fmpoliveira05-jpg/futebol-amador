using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repositório específico para operações de persistência e consulta de dados da entidade [Pitch] (Campo de Jogo).
    /// 
    /// Implementa o contrato [IPitchRepository] e utiliza o [AmateurFootballContext] para interagir
    /// com a tabela de Campos.
    /// </summary>
    public class PitchRepository : IPitchRepository
    {
        /// <summary>
        /// O contexto da base de dados ([AmateurFootballContext]) injetado.
        /// Utilizado para aceder às tabelas e executar operações de consulta.
        /// </summary>
        private readonly AmateurFootballContext context;

        /// <summary>
        /// Construtor da classe [PitchRepository].
        /// </summary>
        /// <param name="context">O contexto da base de dados (Db Context) injetado via Dependency Injection.</param>
        public PitchRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Obtém um Campo de Jogo (Pitch) pelo seu identificador único (ID) de forma assíncrona.
        /// </summary>
        /// <param name="id">O ID (GUID) do campo a procurar.</param>
        /// <returns>A entidade [Pitch] correspondente ou null se não for encontrada.</returns>
        public async Task<Pitch?> GetPitchById(Guid id)
        {
            return await context.Pitch.FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Obtém um Campo de Jogo (Pitch) pelo seu nome de forma assíncrona.
        /// </summary>
        /// <remarks>
        /// A consulta usa a comparação de igualdade estrita no nome do campo.
        /// </remarks>
        /// <param name="namePitch">O nome completo do campo de jogo.</param>
        /// <returns>A entidade [Pitch] correspondente ou null se não for encontrada.</returns>
        public async Task<Pitch?> GetPitchByName(string namePitch)
        {
            return await context.Pitch.FirstOrDefaultAsync(p => p.Name == namePitch);
        }
    }
}