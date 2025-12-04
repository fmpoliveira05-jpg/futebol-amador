using Application.DTOs.Match;
using Application.Hubs;

namespace Application.Interfaces.Services.Hub
{
    /// <summary>
    /// Contrato de serviço para o Manager do Hub de Finalização de Partida ([FinishMatchHub]).
    /// 
    /// Esta interface define a lógica de negócio para gerir a submissão de resultados de golos,
    /// verificar a coincidência de resultados e controlar o ciclo de vida do lobby de finalização
    /// (armazenado tipicamente em cache).
    /// </summary>
    public interface IManagerFinishMatchService
    {
        /// <summary>
        /// Regista a entrada de um administrador no lobby de finalização e submete o resultado inicial.
        /// </summary>
        /// <remarks>
        /// Este método é o ponto de entrada no processo. Se o resultado do adversário já existir, verifica a coincidência.
        /// </remarks>
        /// <param name="matchId">O ID da partida.</param>
        /// <param name="finishMatch">O DTO com o resultado (golos) submetido.</param>
        /// <param name="userId">O ID do utilizador (admin) que está a submeter.</param>
        /// <param name="connectionId">O ID da conexão SignalR do utilizador.</param>
        /// <returns>Objeto [JoinFinishMatch] com o estado atual da finalização (se já terminou ou se está à espera).</returns>
        public Task<JoinFinishMatch> JoinHubAsync(Guid matchId, ResultMatchDto finishMatch, string userId, string connectionId);

        /// <summary>
        /// Atualiza o resultado de uma equipa no lobby de finalização.
        /// </summary>
        /// <remarks>
        /// Utilizado para corrigir um resultado previamente submetido (em caso de erro) e tentar novamente
        /// sincronizar com o resultado do adversário.
        /// </remarks>
        /// <param name="matchId">O ID da partida.</param>
        /// <param name="finishMatch">O DTO com o resultado corrigido.</param>
        /// <param name="userId">O ID do administrador que edita.</param>
        /// <param name="connectionId">O ID da conexão SignalR.</param>
        /// <returns>Objeto [JoinFinishMatch] atualizado com o novo estado de coincidência.</returns>
        public Task<JoinFinishMatch> UpdateResult(Guid matchId, ResultMatchDto finishMatch, string userId, string connectionId);

        /// <summary>
        /// Remove um administrador do lobby de finalização (saída explícita ou cancelamento).
        /// </summary>
        /// <param name="matchId">O ID da partida.</param>
        /// <param name="teamId">O ID da equipa.</param>
        /// <param name="connectionId">O ID da conexão SignalR.</param>
        /// <returns><c>true</c> se a remoção do estado da cache foi bem-sucedida.</returns>
        public Task<bool> LeaveHubAsync(Guid matchId, Guid teamId, string connectionId);

        /// <summary>
        /// Lida com a desconexão abrupta de um cliente.
        /// </summary>
        /// <remarks>
        /// Recupera o ID da partida e da equipa (se possível) e invoca [LeaveHubAsync] para limpar o estado temporário na cache.
        /// </remarks>
        /// <param name="maybeMatchId">ID da partida (pode ser nulo).</param>
        /// <param name="maybeTeamId">ID da equipa (pode ser nulo).</param>
        /// <param name="connectionId">O ID da conexão que desconectou.</param>
        /// <returns><c>true</c> se o estado foi limpo.</returns>
        public Task<bool> HandleDisconnectAsync(Guid? maybeMatchId, Guid? maybeTeamId, string connectionId);
    }
}