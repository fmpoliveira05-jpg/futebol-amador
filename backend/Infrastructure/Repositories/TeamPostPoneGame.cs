using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repositório específico para operações de persistência e consulta da entidade [PostPoneMatch] (Pedidos de Adiamento).
    /// 
    /// Implementa o contrato [ITeamPostPoneGameRepository] e é o responsável por gerir o ciclo de vida
    /// dos pedidos de remarcação de partidas.
    /// </summary>
    public class TeamPostPoneGame : ITeamPostPoneGameRepository
    {
        /// <summary>
        /// O contexto da base de dados ([AmateurFootballContext]) injetado.
        /// Utilizado para aceder à tabela de pedidos de adiamento.
        /// </summary>
        private readonly AmateurFootballContext context;

        /// <summary>
        /// Construtor da classe [TeamPostPoneGame].
        /// </summary>
        /// <param name="context">O contexto da base de dados (Db Context) injetado via Dependency Injection.</param>
        public TeamPostPoneGame(AmateurFootballContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Adiciona um novo registo de pedido de adiamento à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="postPoneMatch">A entidade [PostPoneMatch] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        public async Task AddTeamPostPoneMatch(PostPoneMatch postPoneMatch)
        {
            await context.PostPoneMatch.AddAsync(postPoneMatch);
        }

        /// <summary>
        /// Marca um registo de pedido de adiamento existente para ser removido da base de dados.
        /// </summary>
        /// <param name="postPoneMatch">A entidade [PostPoneMatch] a ser removida (ex: após o pedido ter sido aceite ou cancelado).</param>
        public void RemoveTeamPostPoneMatch(PostPoneMatch postPoneMatch)
        {
            context.PostPoneMatch.Remove(postPoneMatch);
        }

        /// <summary>
        /// Obtém um pedido de adiamento específico com base na Equipa Remetente e na Partida Alvo.
        /// </summary>
        /// <remarks>
        /// Utiliza `.Include()` para carregar a entidade de navegação [Match] (Partida) associada (Eager Loading).
        /// </remarks>
        /// <param name="idTeam">O ID da equipa que fez o pedido.</param>
        /// <param name="idMatch">O ID da partida que está a ser adiada.</param>
        /// <returns>A entidade [PostPoneMatch] com a partida carregada, ou null se não existir.</returns>
        public async Task<PostPoneMatch?> GetTeamPostPoneMatch(Guid idTeam, Guid idMatch)
        {
            return await context.PostPoneMatch
                .Include(ts => ts.Match)
                .FirstOrDefaultAsync(ppm => ppm.IdTeamPostPone != idTeam && ppm.IdMatch == idMatch);
        }

        /// <summary>
        /// Obtém um pedido de adiamento específico, carregando a Partida e o Campo ([Pitch]) associado.
        /// </summary>
        /// <remarks>
        /// Utiliza `.Include().ThenInclude()` para carregar a relação profunda: PostPoneMatch -> Match -> Pitch.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa que fez o pedido.</param>
        /// <param name="idMatch">O ID da partida que está a ser adiada.</param>
        /// <returns>A entidade [PostPoneMatch] com a Partida e o Local do Jogo carregados.</returns>
        public async Task<PostPoneMatch?> GetTeamPostPoneMatchWithPitch(Guid idTeam, Guid idMatch)
        {
            return await context.PostPoneMatch
                .Include(ts => ts.Match)
                    .ThenInclude(m => m.Teams)
                        .ThenInclude(ts => ts.Team)
                .Include(ts => ts.Match)    
                    .ThenInclude(m => m.Pitch)
                .Include(ts => ts.Match)
                .FirstOrDefaultAsync(ppm => ppm.IdTeamPostPone != idTeam && ppm.IdMatch == idMatch);
        }
    }
}