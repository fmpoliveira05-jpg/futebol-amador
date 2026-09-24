using Application.DTOs.RankMatchMaker;

namespace Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de Serviço de Domínio para a lógica algorítmica de Matchmaking (Emparelhamento).
    /// 
    /// Esta interface define os métodos de lógica pura, responsáveis por receber listas de equipas
    /// e aplicar os critérios de emparelhamento (tolerância de Rank, Idade, Pontos), sem acesso direto à base de dados.
    /// </summary>
    public interface IMatchMakerService
    {
        /// <summary>
        /// Tenta encontrar um adversário instantâneo para uma equipa que acabou de entrar no Lobby.
        /// </summary>
        /// <remarks>
        /// Esta lógica é utilizada no momento em que uma equipa se regista no lobby para verificar
        /// se há um adversário perfeitamente compatível disponível imediatamente (busca "greedy").
        /// </remarks>
        /// <param name="finder">O DTO da equipa que iniciou a procura.</param>
        /// <param name="teamsInSearch">A coleção de equipas atualmente à espera na fila.</param>
        /// <param name="gameDate">A data de jogo para a qual se procura (para conciliar com as equipas em espera).</param>
        /// <returns>O ID (GUID) da equipa adversária encontrada, ou <c>null</c> se não houver compatibilidade imediata.</returns>
        public Guid? LogicMatchMakerJoinHub(InfoTeamRankMatchMakerDto finder, IEnumerable<InfoTeamRankMatchMakerDto> teamsInSearch, DateTime gameDate);

        /// <summary>
        /// Executa o algoritmo de emparelhamento em lote (Batch Processing) para todas as equipas em espera.
        /// </summary>
        /// <remarks>
        /// Este método implementa uma técnica otimizada (como o Sliding Window) para encontrar todos os pares compatíveis 
        /// (dentro dos limites de tolerância) na lista de espera. É tipicamente invocado por um serviço de background.
        /// </remarks>
        /// <param name="teamsInSearch">A lista completa de todas as equipas no Lobby.</param>
        /// <param name="criteria">Os critérios de tolerância dinâmicos (Idade Média, Pontos) para o emparelhamento.</param>
        /// <returns>Um dicionário contendo os pares formados, onde a Chave e o Valor são as entradas das equipas emparelhadas.</returns>
        public Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub>? LogicMatchMaker(IEnumerable<EntryRankMatchMakerHub> teamsInSearch, CriteriaMatchMaker criteria);
    }
}