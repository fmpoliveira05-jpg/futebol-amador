using Domain.Entities;

namespace Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de Serviço de Domínio para a gestão completa da hierarquia de Níveis de Classificação ([Rank]).
    /// 
    /// Esta interface define os métodos para a obtenção de Ranks, a gestão da sua estrutura hierárquica
    /// (próximo/anterior) e as operações CRUD (Criar, Ler, Atualizar, Eliminar).
    /// </summary>
    public interface IRankService
    {
        /// <summary>
        /// Obtém o Rank que atua como nível de entrada ou padrão na hierarquia.
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
        /// Marca um Rank para ser eliminado da base de dados.
        /// </summary>
        /// <remarks>
        /// Esta operação requer validação prévia para garantir que nenhuma equipa ou outro Rank dependa desta entidade.
        /// </remarks>
        /// <param name="rank">A entidade [Rank] a ser eliminada.</param>
        Task DeleteRank(Rank rank);

        /// <summary>
        /// Elimina um Rank pelo seu identificador único (ID).
        /// </summary>
        /// <remarks>
        /// Este método de conveniência obtém o Rank pelo ID e executa a lógica de deleção.
        /// </remarks>
        /// <param name="rankId">O ID (GUID) do Rank a eliminar.</param>
        Task DeleteRankById(Guid rankId);

        /// <summary>
        /// Atualiza os dados de um Rank existente (ex: nome, pontos por vitória, limite de promoção).
        /// </summary>
        /// <param name="newRank">A entidade [Rank] com os novos valores.</param>
        /// <returns>Uma tarefa assíncrona que retorna a entidade [Rank] atualizada.</returns>
        Task<Rank> UpdateRank(Rank newRank);

        /// <summary>
        /// Adiciona um novo Rank à base de dados.
        /// </summary>
        /// <param name="rank">A entidade [Rank] a ser persistida.</param>
        Task AddRankAsync(Rank rank);

        /// <summary>
        /// Adiciona um novo Rank, inserindo-o na cadeia hierárquica entre um Rank existente e o Rank anterior (implícito).
        /// </summary>
        /// <remarks>
        /// Esta operação é transacional e envolve a atualização das chaves estrangeiras dos Ranks adjacentes.
        /// </remarks>
        /// <param name="rank">O novo Rank a ser inserido.</param>
        /// <param name="nextRank">O Rank que ficará imediatamente acima do novo Rank na hierarquia.</param>
        Task AddRankInOtherRankPlaceAsync(Rank rank, Rank nextRank);

        /// <summary>
        /// Altera o Rank que precede o [rank] fornecido na hierarquia.
        /// </summary>
        /// <remarks>
        /// Utilizado para reorganizar a hierarquia de Ranks.
        /// </remarks>
        /// <param name="rank">O Rank cuja posição anterior será alterada.</param>
        /// <param name="newPreviousRank">O novo Rank anterior na cadeia.</param>
        Task ChangePreviousRank(Rank rank, Rank newPreviousRank);
    }
}
