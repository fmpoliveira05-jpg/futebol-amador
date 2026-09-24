using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>
    /// Contrato de Repositório para operações de acesso a dados da entidade [Rank] (Nível de Classificação).
    /// 
    /// Define os métodos de consulta e persistência para gerir a estrutura hierárquica dos Ranks
    /// e os seus atributos de pontuação.
    /// </summary>
    public interface IRankRepository
    {
        /// <summary>
        /// Obtém o Rank que atua como nível de entrada ou padrão na hierarquia (o Rank inicial).
        /// </summary>
        /// <returns>A entidade [Rank] padrão.</returns>
        Task<Rank> GetDefaultRankAsync();

        /// <summary>
        /// Obtém o próximo Rank na hierarquia (o Rank para o qual a equipa será promovida).
        /// </summary>
        /// <param name="currentRank">O Rank atual de referência.</param>
        /// <returns>A entidade [Rank] imediatamente superior ou null se for o Rank máximo.</returns>
        Task<Rank> GetNextRankAsync(Rank currentRank);

        /// <summary>
        /// Obtém o Rank anterior na hierarquia (o Rank para o qual a equipa será despromovida).
        /// </summary>
        /// <param name="currentRank">O Rank atual de referência.</param>
        /// <returns>A entidade [Rank] imediatamente inferior ou null se for o Rank mínimo.</returns>
        Task<Rank> GetPreviousRankAsync(Rank currentRank);

        /// <summary>
        /// Obtém uma lista de todos os níveis de Rank existentes no sistema.
        /// </summary>
        /// <returns>Uma lista de todas as entidades [Rank].</returns>
        Task<List<Rank>> GetAllRanksAsync();

        /// <summary>
        /// Obtém um Rank pelo seu identificador único (ID).
        /// </summary>
        /// <param name="rankId">O ID (GUID) do Rank.</param>
        /// <returns>A entidade [Rank] correspondente.</returns>
        Task<Rank> GetRankByIdAsync(Guid rankId);

        /// <summary>
        /// Obtém um Rank pelo seu nome (ex: "Ouro", "Prata").
        /// </summary>
        /// <param name="rankName">O nome do Rank.</param>
        /// <returns>A entidade [Rank] correspondente.</returns>
        Task<Rank> GetRankByNameAsync(string rankName);

        /// <summary>
        /// Adiciona um novo Rank à base de dados.
        /// </summary>
        /// <param name="rank">A entidade [Rank] a ser persistida.</param>
        Task AddRankAsync(Rank rank);

        /// <summary>
        /// Adiciona um novo Rank, inserindo-o na cadeia hierárquica entre um Rank existente e o Rank anterior (implícito).
        /// </summary>
        /// <remarks>
        /// Esta operação é transacional e envolve a atualização das chaves estrangeiras [IdNextRank] e [IdPreviousRank] no Rank adjacente.
        /// </remarks>
        /// <param name="rank">O novo Rank a ser inserido.</param>
        /// <param name="nextRank">O Rank que ficará imediatamente acima do novo Rank na hierarquia.</param>
        Task AddRankInOtherRankPlaceAsync(Rank rank, Rank nextRank);

        /// <summary>
        /// Marca um Rank para ser atualizado.
        /// </summary>
        /// <param name="newRank">A entidade [Rank] com os novos valores (ex: novos limites de pontuação).</param>
        void UpdateRank(Rank newRank);

        /// <summary>
        /// Marca um Rank para ser removido da base de dados.
        /// </summary>
        /// <param name="rank">A entidade [Rank] a ser removida.</param>
        void DeleteRank(Rank rank);
    }
}