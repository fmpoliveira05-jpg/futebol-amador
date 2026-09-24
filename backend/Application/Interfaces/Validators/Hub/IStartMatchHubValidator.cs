using Domain.Entities;
using System;
using System.Collections.Concurrent;

namespace Application.Interfaces.Validators.Hub
{
    /// <summary>
    /// Contrato de Validador de Regras de Negócio para o Hub SignalR de Início de Partida ([StartMatchHub]).
    /// 
    /// Define as regras síncronas que verificam a validade dos IDs, a existência de entidades e o estado
    /// do lobby antes de permitir a entrada de um administrador para iniciar um jogo.
    /// </summary>
    public interface IStartMatchHubValidator
    {
        /// <summary>
        /// Valida se os IDs essenciais para a entrada no Hub (Match, Utilizador, Conexão) são válidos (não nulos/vazios).
        /// </summary>
        /// <param name="matchId">O ID da partida.</param>
        /// <param name="userId">O ID do utilizador autenticado.</param>
        /// <param name="connectionId">O ID da conexão SignalR.</param>
        /// <exception cref="System.ArgumentException">Lançada se qualquer um dos IDs for inválido.</exception>
        public void ValidateVariableJoinMatch(Guid matchId, string userId, string connectionId);

        /// <summary>
        /// Valida se a entidade [Matches] foi encontrada na base de dados.
        /// </summary>
        /// <param name="match">A entidade [Matches] a ser verificada.</param>
        /// <exception cref="System.ArgumentException">Lançada se a partida for nula.</exception>
        public void ValidateMatchJoinMatch(Matches match);

        /// <summary>
        /// Valida as regras de negócio complexas antes de permitir a entrada no lobby de início de partida.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: 
        /// <list type="bullet">
        ///     <item>A equipa deve cumprir os requisitos mínimos (ex: número de membros).</item>
        ///     <item>A equipa já não pode ter um admin a iniciar a partida (prevenção de duplicação).</item>
        ///     <item>O lobby deve ter no máximo 2 admins (um de cada equipa) para iniciar.</item>
        /// </list>
        /// </remarks>
        /// <param name="teamMatchAdmin">As estatísticas da equipa (carregadas com dados de membros).</param>
        /// <param name="idTeam">O ID da equipa que está a entrar.</param>
        /// <param name="hub">O dicionário ([ConcurrentDictionary]) que representa o estado atual do lobby.</param>
        /// <exception cref="System.InvalidOperationException">Se a equipa não cumprir os requisitos, já estiver no lobby ou se o lobby estiver cheio.</exception>
        public void ValidateJoinMatch(TeamStatistics teamMatchAdmin, Guid idTeam, ConcurrentDictionary<Guid, string> hub);
    }
}