using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repositório específico para operações de persistência de dados e consulta da entidade [Rank].
    /// 
    /// Esta classe é responsável por gerir a hierarquia de Ranks (nível anterior e próximo)
    /// e por fornecer métodos para determinar o Rank padrão e a progressão.
    /// </summary>
    public class RankRepository : IRankRepository
    {
        /// <summary>
        /// O contexto da base de dados ([AmateurFootballContext]) injetado.
        /// Utilizado para aceder à tabela de Ranks.
        /// </summary>
        private readonly AmateurFootballContext DbContext;

        /// <summary>
        /// Construtor da classe [RankRepository].
        /// </summary>
        /// <param name="DbContext">O contexto da base de dados (Db Context) injetado via Dependency Injection.</param>
        public RankRepository(AmateurFootballContext DbContext)
        {
            this.DbContext = DbContext;
        }

        /// <summary>
        /// Adiciona um novo Rank à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="rank">A entidade [Rank] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        public async Task AddRankAsync(Rank rank)
        {
            await DbContext.Rank.AddAsync(rank);
        }

        /// <summary>
        /// Adiciona um novo Rank, inserindo-o na cadeia hierárquica entre o Rank anterior (implícito) e o [nextRank].
        /// </summary>
        /// <remarks>
        /// Esta operação é uma transação em duas partes:
        /// 1. Atualiza as FKs do novo Rank (IdNextRank e IdPreviousRank).
        /// 2. Atualiza o [nextRank] para apontar para o novo Rank na sua FK de Rank anterior ([IdPreviousRank]).
        /// </remarks>
        /// <param name="rank">O novo Rank a ser inserido.</param>
        /// <param name="nextRank">O Rank que ficará imediatamente acima do novo Rank na hierarquia.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>).</returns>
        public async Task AddRankInOtherRankPlaceAsync(Rank rank, Rank nextRank)
        {
            rank.IdNextRank = nextRank.Id;
            rank.IdPreviousRank = nextRank.IdPreviousRank;
            nextRank.IdPreviousRank = nextRank.Id;
            await DbContext.Rank.AddAsync(rank);
            DbContext.Rank.Update(nextRank);
        }

        /// <summary>
        /// Marca um Rank para ser removido da base de dados.
        /// </summary>
        /// <param name="rank">A entidade [Rank] a ser removida.</param>
        public void DeleteRank(Rank rank)
        {
            DbContext.Remove(rank);
        }

        /// <summary>
        /// Obtém todos os níveis de Rank existentes no sistema.
        /// </summary>
        /// <returns>Uma lista de todas as entidades [Rank].</returns>
        public async Task<List<Rank>> GetAllRanksAsync()
        {
            return await DbContext.Rank.ToListAsync();
        }

        /// <summary>
        /// Obtém o Rank que atua como nível de entrada ou padrão na hierarquia.
        /// </summary>
        /// <remarks>
        /// Assume-se que o Rank padrão é aquele cujo [IdPreviousRank] é nulo (o Rank mais baixo/inicial).
        /// </remarks>
        /// <returns>A entidade [Rank] padrão.</returns>
        public async Task<Rank> GetDefaultRankAsync()
        {
            return await DbContext.Rank.FirstOrDefaultAsync(r => r.IdPreviousRank == null);
        }

        /// <summary>
        /// Obtém o próximo Rank na hierarquia (o Rank para o qual a equipa será promovida).
        /// </summary>
        /// <param name="currentRank">O Rank atual de referência.</param>
        /// <returns>A entidade [Rank] imediatamente superior ou null se for o Rank máximo.</returns>
        public async Task<Rank> GetNextRankAsync(Rank currentRank)
        {
            return await DbContext.Rank.FirstOrDefaultAsync(r => r.Id == currentRank.IdNextRank);
        }

        /// <summary>
        /// Obtém o Rank anterior na hierarquia (o Rank para o qual a equipa será despromovida).
        /// </summary>
        /// <param name="currentRank">O Rank atual de referência.</param>
        /// <returns>A entidade [Rank] imediatamente inferior ou null se for o Rank mínimo.</returns>
        public async Task<Rank> GetPreviousRankAsync(Rank currentRank)
        {
            return await DbContext.Rank.FirstOrDefaultAsync(r => r.Id == currentRank.IdPreviousRank);
        }

        /// <summary>
        /// Obtém um Rank pelo seu identificador único (ID).
        /// </summary>
        /// <param name="rankId">O ID (GUID) do Rank.</param>
        /// <returns>A entidade [Rank] correspondente.</returns>
        public async Task<Rank> GetRankByIdAsync(Guid rankId)
        {
            return await DbContext.Rank.FirstOrDefaultAsync(r => r.Id == rankId);
        }

        /// <summary>
        /// Obtém um Rank pelo seu nome (ex: "Ouro", "Prata").
        /// </summary>
        /// <param name="rankName">O nome do Rank.</param>
        /// <returns>A entidade [Rank] correspondente.</returns>
        public async Task<Rank> GetRankByNameAsync(string rankName)
        {
            return await DbContext.Rank.FirstOrDefaultAsync(r => r.Name == rankName);
        }

        /// <summary>
        /// Marca uma entidade [Rank] existente para ser atualizada na base de dados.
        /// </summary>
        /// <param name="newRank">A entidade [Rank] com os novos valores (ex: novos limites de pontuação).</param>
        public void UpdateRank(Rank newRank)
        {
            DbContext.Rank.Update(newRank);
        }
    }
}