using Application.Interfaces.Validators.Hub;
using Domain.Entities;
using System.Collections.Concurrent;

namespace Application.Validators.Hubs
{
    /// <summary>
    /// Validador de Regras de Negócio para o Hub SignalR de Início de Partida ([StartMatchHub]).
    /// 
    /// Responsável por verificar os IDs de entrada, o estado da partida e as regras de limite de membros/admins
    /// antes de permitir a entrada no lobby de sincronização de início de jogo.
    /// </summary>
    public class StartMatchHubValidator : IStartMatchHubValidator
    {
        /// <summary>
        /// Construtor padrão da classe [StartMatchHubValidator].
        /// </summary>
        public StartMatchHubValidator() { }

        /// <summary>
        /// Valida se os IDs essenciais para a entrada no Hub (Match, Utilizador, Conexão) são válidos (não nulos/vazios).
        /// </summary>
        /// <param name="matchId">O ID da partida.</param>
        /// <param name="userId">O ID do utilizador autenticado.</param>
        /// <param name="connectionId">O ID da conexão SignalR.</param>
        /// <exception cref="ArgumentException">Lançada se qualquer um dos IDs for inválido.</exception>
        public void ValidateVariableJoinMatch(Guid matchId, string userId, string connectionId)
        {
            if (matchId == Guid.Empty)
            {
                throw new ArgumentException("O id da partida está null");
            }

            if (userId == string.Empty)
            {
                throw new ArgumentException("O id do utilizador está a null");
            }

            if (string.IsNullOrEmpty(connectionId))
            {
                throw new ArgumentException("A connection string está a null ou vazia");
            }
        }

        /// <summary>
        /// Valida se a entidade [Matches] foi encontrada na base de dados.
        /// </summary>
        /// <param name="match">A entidade [Matches] a ser verificada.</param>
        /// <exception cref="ArgumentException">Lançada se a partida for nula.</exception>
        public void ValidateMatchJoinMatch(Matches match)
        {
            if (match == null)
            {
                throw new ArgumentException("A partida não foi encontrada");
            }
        }

        /// <summary>
        /// Valida as regras de negócio complexas antes de permitir a entrada no lobby de início de partida.
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>A equipa deve ter no mínimo 11 jogadores para iniciar o jogo.</item>
        ///     <item>A equipa já não pode ter um admin a iniciar a partida (prevenção de duplicação).</item>
        ///     <item>O lobby deve ter no máximo 2 admins (um de cada equipa) para iniciar.</item>
        ///     <item>O ID da equipa do admin deve coincidir com o ID que ele está a tentar entrar.</item>
        /// </list>
        /// </remarks>
        /// <param name="teamMatchAdmin">As estatísticas da equipa (carregadas com dados de membros).</param>
        /// <param name="idTeam">O ID da equipa que está a entrar.</param>
        /// <param name="hub">O dicionário ([ConcurrentDictionary]) que representa o estado atual do lobby.</param>
        /// <exception cref="ArgumentException">Se a equipa do admin for nula.</exception>
        /// <exception cref="InvalidOperationException">Se a equipa tiver menos de 11 membros, se a equipa já estiver no lobby ou se o lobby estiver cheio.</exception>
        public void ValidateJoinMatch(TeamStatistics teamMatchAdmin, Guid idTeam, ConcurrentDictionary<Guid, string> hub)
        {
            if (teamMatchAdmin == null)
            {
                throw new ArgumentException("A equipa do admin não foi encontrada");
            }

            if (teamMatchAdmin.IdTeam != idTeam)
            {
                throw new InvalidOperationException("O id da team é diferente do da team que está a entrar no hub");
            }

            if (teamMatchAdmin.Team.Members.Count < 11)
            {
                throw new InvalidOperationException("Só pode dar inicio a partida se a equipa tiver 11 jogadores");
            }

            if (hub.ContainsKey(idTeam))
            {
                throw new InvalidOperationException("Já existe um admin desta equipa a iniciar a partida");
            }

            if (hub.Count >= 2)
            {
                throw new InvalidOperationException("Apenas do 2 admins (um de cada equipa) pode aceder a esta funcionalidade");
            }
        }
    }
}