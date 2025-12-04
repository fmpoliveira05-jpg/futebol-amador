using Application.DTOs.Match;
using Application.Hubs;
using Domain.Entities;
using System.Collections.Concurrent;

namespace Application.Interfaces.Validators.Hub
{
    /// <summary>
    /// Contrato de Validador de Regras de Negócio específico para o processo de Finalização de Partida ([FinishMatchHub]).
    /// 
    /// Esta interface define as regras síncronas que verificam a validade dos dados (golos) e o contexto do lobby
    /// antes que os resultados sejam considerados para sincronização e persistência.
    /// </summary>
    public interface IFinishMatchValidator
    {
        /// <summary>
        /// Valida se os IDs essenciais e o DTO de resultado são válidos para a entrada no Hub.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Validade de IDs (Match, User, Connection, Team, Opponent) e que os golos não são negativos.
        /// </remarks>
        /// <param name="matchId">O ID da partida.</param>
        /// <param name="finishMatch">O DTO de resultado com os golos submetidos.</param>
        /// <param name="userId">O ID do utilizador autenticado.</param>
        /// <param name="connectionId">O ID da conexão SignalR.</param>
        /// <exception cref="System.ArgumentException">Lançada se os IDs forem inválidos ou faltarem.</exception>
        /// <exception cref="System.InvalidOperationException">Lançada se o número de golos for negativo.</exception>
        public void ValidateVariableJoinMatch(Guid matchId, ResultMatchDto finishMatch, string userId, string connectionId);

        /// <summary>
        /// Valida se a entidade [Matches] existe e pode ser finalizada (ex: se o tempo de jogo mínimo já passou).
        /// </summary>
        /// <param name="match">A partida a ser verificada.</param>
        /// <exception cref="System.ArgumentException">Lançada se a partida for nula.</exception>
        /// <exception cref="System.InvalidOperationException">Lançada se o tempo mínimo de jogo não tiver decorrido (ex: 90 minutos).</exception>
        public void ValidateMatchJoinMatch(Matches match);

        /// <summary>
        /// Valida as regras de negócio antes de permitir a entrada de um administrador no Hub de Finalização.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Se o admin pertence à equipa, prevenção de duplicação, e se o Hub não está já com 2 admins.
        /// </remarks>
        /// <param name="teamMatchAdmin">As estatísticas da equipa (carregadas com dados de membros).</param>
        /// <param name="teamId">O ID da equipa que está a entrar.</param>
        /// <param name="hub">O dicionário ([ConcurrentDictionary]) que representa o estado atual do Hub.</param>
        /// <exception cref="System.InvalidOperationException">Se a equipa já estiver no Hub ou o Hub estiver cheio.</exception>
        public void ValidateJoinMatch(TeamStatistics teamMatchAdmin, Guid teamId, ConcurrentDictionary<Guid, EntryHubFinishMatch> hub);

        /// <summary>
        /// Valida as regras de negócio para atualizar (editar) um resultado submetido.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: O administrador que edita deve estar no Hub e a equipa deve existir no estado temporário.
        /// </remarks>
        /// <param name="teamMatchAdmin">As estatísticas da equipa.</param>
        /// <param name="teamId">O ID da equipa.</param>
        /// <param name="hub">O dicionário do lobby (deve conter a equipa).</param>
        /// <exception cref="System.InvalidOperationException">Se o admin não estiver no Hub ou a equipa for inválida.</exception>
        public void ValidateUpdateResult(TeamStatistics teamMatchAdmin, Guid teamId, ConcurrentDictionary<Guid, EntryHubFinishMatch> hub);

        /// <summary>
        /// Valida se os resultados submetidos por ambas as equipas são consistentes e idênticos.
        /// </summary>
        /// <remarks>
        /// Verifica se: Golo(Equipa A) de A == Golo(Equipa B) de B, e vice-versa.
        /// </remarks>
        /// <param name="firstResult">O resultado submetido pela primeira equipa.</param>
        /// <param name="secondResult">O resultado submetido pela segunda equipa.</param>
        /// <exception cref="System.InvalidOperationException">Se houver inconsistência nos IDs dos adversários.</exception>
        /// <exception cref="System.ArgumentException">Se os golos não coincidirem.</exception>
        public void ValidateMatchResultTwoTeams(ResultMatchDto firstResult, ResultMatchDto secondResult);

        /// <summary>
        /// Valida se a entidade de estatísticas do adversário existe.
        /// </summary>
        /// <param name="opponent">A entidade [TeamStatistics] do adversário.</param>
        /// <exception cref="System.ArgumentException">Lançada se o adversário for nulo.</exception>
        public void ValidateOpponentTeam(TeamStatistics opponent);
    }
}