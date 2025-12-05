using Application.Hubs;

namespace Application.Interfaces.Services.Hub
{
    /// <summary>
    /// Contrato de serviço para o Manager do Hub de Início de Partida ([StartMatchHub]).
    /// 
    /// Esta interface define a lógica de negócio para gerir o ciclo de vida do lobby de espera (antes do jogo começar),
    /// coordenando a entrada dos administradores e a transição do estado da partida.
    /// </summary>
    public interface IManagerStartMatchService
    {
        /// <summary>
        /// Regista a entrada de um administrador no lobby de início de partida e tenta iniciar o jogo.
        /// </summary>
        /// <remarks>
        /// Este é o método principal que verifica se o administrador é o primeiro a entrar ou se é o segundo,
        /// acionando a lógica de sincronização para começar o jogo e limpar o lobby.
        /// </remarks>
        /// <param name="matchId">O ID da partida alvo.</param>
        /// <param name="userId">O ID do utilizador autenticado (admin).</param>
        /// <param name="idTeam">O ID da equipa do administrador.</param>
        /// <param name="connectionId">O ID da conexão SignalR do utilizador.</param>
        /// <returns>Objeto [JoinStartMatchResult] com o estado atual da operação (se é o primeiro, se o jogo começou, etc.).</returns>
        public Task<JoinStartMatchResult> JoinHubAsync(Guid matchId, string userId, Guid idTeam, string connectionId);

        /// <summary>
        /// Remove um administrador do lobby de início de partida (saída explícita do utilizador).
        /// </summary>
        /// <remarks>
        /// Se o lobby ficar vazio após a remoção, a entrada de cache associada à partida deve ser eliminada.
        /// </remarks>
        /// <param name="matchId">O ID da partida.</param>
        /// <param name="idTeam">O ID da equipa.</param>
        /// <param name="connectionId">O ID da conexão SignalR.</param>
        /// <returns><c>true</c> se a remoção do estado da cache foi bem-sucedida.</returns>
        public Task<bool> LeaveHubAsync(Guid matchId, Guid idTeam, string connectionId);

        /// <summary>
        /// Lida com a desconexão abrupta de um cliente.
        /// </summary>
        /// <remarks>
        /// Este método é chamado na função <c>OnDisconnectedAsync</c> do Hub para limpar o estado temporário
        /// e evitar que jogos fiquem bloqueados em estado de espera por conexões "fantasmas".
        /// </remarks>
        /// <param name="maybeMatchId">ID da partida (opcional, recuperado do contexto de conexão).</param>
        /// <param name="maybeTeamId">ID da equipa (opcional, recuperado do contexto de conexão).</param>
        /// <param name="connectionId">O ID da conexão que desconectou.</param>
        /// <returns><c>true</c> se o estado foi limpo (o utilizador estava no lobby).</returns>
        public Task<bool> HandleDisconnectAsync(Guid? maybeMatchId, Guid? maybeTeamId, string connectionId);
    }
}