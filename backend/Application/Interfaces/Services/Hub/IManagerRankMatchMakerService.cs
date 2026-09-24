using Application.DTOs.RankMatchMaker;

namespace Application.Interfaces.Services.Hub
{
    /// <summary>
    /// Contrato de serviço para o Manager do motor de Matchmaking Ranqueado.
    /// 
    /// Esta interface define a lógica de negócio para gerir a fila de espera do lobby de procura de jogos,
    /// validar a elegibilidade das equipas e coordenar o emparelhamento (chamado pelo serviço de background).
    /// </summary>
    public interface IManagerRankMatchMakerService
    {
        /// <summary>
        /// Regista a entrada de um administrador no lobby de Matchmaking.
        /// </summary>
        /// <remarks>
        /// O serviço deve validar a elegibilidade do jogador/equipa e tentar encontrar um par imediato ou adicionar a equipa à fila de espera em cache.
        /// </remarks>
        /// <param name="idPlayer">O ID do jogador (admin) que inicia a procura.</param>
        /// <param name="idTeam">O ID da equipa.</param>
        /// <param name="hoursGame">O horário preferencial do jogo.</param>
        /// <param name="connectionId">O ID da conexão SignalR.</param>
        /// <returns>Objeto [EntryRankMatchMakerHub] com os dados da equipa que entrou na fila.</returns>
        public Task<EntryRankMatchMakerHub> JoinRankMatchMaker(string idPlayer, Guid idTeam, TimeOnly hoursGame, string connectionId);

        /// <summary>
        /// Remove uma equipa do lobby de Matchmaking (saída explícita ou cancelamento da procura).
        /// </summary>
        /// <param name="teamId">O ID da equipa a ser removida.</param>
        /// <param name="connectionId">O ID da conexão SignalR.</param>
        /// <returns><c>true</c> se a remoção do estado da cache foi bem-sucedida.</returns>
        public Task<bool> LeaveRankMatchMakerAsync(Guid teamId, string connectionId);

        /// <summary>
        /// Lida com a desconexão abrupta de um cliente, removendo a equipa do estado do lobby.
        /// </summary>
        /// <remarks>
        /// Este método é tipicamente chamado pelo Hub na função <c>OnDisconnectedAsync</c> para limpar o estado da cache.
        /// </remarks>
        /// <param name="maybeTeamId">ID da equipa (opcional, recuperado do contexto de conexão).</param>
        /// <param name="connectionId">O ID da conexão que desconectou.</param>
        /// <returns><c>true</c> se o estado foi limpo.</returns>
        public Task<bool> HandleDisconnectAsync(Guid? maybeTeamId, string connectionId);

        /// <summary>
        /// Executa o algoritmo de emparelhamento em lote para todas as equipas em espera.
        /// </summary>
        /// <remarks>
        /// Este método é invocado periodicamente pelo serviço de background. Deve aplicar os [CriteriaMatchMaker] dinâmicos.
        /// </remarks>
        /// <param name="criteria">Os critérios de tolerância (Idade Média, Pontos) para o emparelhamento.</param>
        /// <returns>Dicionário de pares formados, onde a chave e o valor representam as equipas emparelhadas.</returns>
        public Task<Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub>> MatchMaker(CriteriaMatchMaker criteria);
    }
}