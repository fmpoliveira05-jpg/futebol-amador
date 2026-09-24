using Application.DTOs.Match;
using Application.Hubs;
using Application.Interfaces.Validators.Hub;
using Domain.Entities;
using System.Collections.Concurrent;

namespace Application.Validators.Hubs
{
    /// <summary>
    /// Validador de Regras de Negócio específico para o processo de Finalização de Partida (Finish Match Hub).
    /// 
    /// Esta classe é crítica para garantir que os resultados submetidos por ambos os administradores
    /// das equipas são válidos e que o processo de submissão ocorre dentro das regras de tempo e contexto.
    /// </summary>
    public class FinishMatchValidator : IFinishMatchValidator
    {
        /// <summary>
        /// Valida todos os IDs e dados essenciais para a entrada no Hub de Finalização.
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>Validade dos IDs (Match, User, Connection, Team, Opponent).</item>
        ///     <item>Golos (não podem ser negativos).</item>
        /// </list>
        /// </remarks>
        /// <param name="matchId">O ID da partida.</param>
        /// <param name="finishMatch">O DTO de resultado com os golos.</param>
        /// <param name="userId">O ID do utilizador autenticado.</param>
        /// <param name="connectionId">O ID da conexão SignalR.</param>
        /// <exception cref="ArgumentException">Se os IDs forem inválidos ou faltarem.</exception>
        /// <exception cref="InvalidOperationException">Se o número de golos for negativo.</exception>
        public void ValidateVariableJoinMatch(Guid matchId, ResultMatchDto finishMatch, string userId, string connectionId)
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

            if (finishMatch == null)
            {
                throw new ArgumentException("O resultado da partida não pode estar a vazio ou nulo");
            }

            if (finishMatch.NumGoalsOpponent < 0 || finishMatch.NumGoalsTeam < 0)
            {
                throw new InvalidOperationException("O número de golos das equipas não pode ser negativo");
            }

            if (finishMatch.IdTeam == Guid.Empty)
            {
                throw new ArgumentException("O id da sua equipa é obrigatório");
            }

            if (finishMatch.IdOpponent == Guid.Empty)
            {
                throw new ArgumentException("O id da equipa adversária é obrigatório");
            }
        }

        /// <summary>
        /// Valida se o jogo pode ser finalizado com base no tempo decorrido.
        /// </summary>
        /// <remarks>
        /// Regra verificada: Devem ter passado pelo menos 90 minutos desde o início do jogo ([match.TimeStart]).
        /// </remarks>
        /// <param name="match">A partida (carregada com TimeStart).</param>
        /// <exception cref="ArgumentException">Lançada se a partida for nula.</exception>
        /// <exception cref="InvalidOperationException">Se não tiverem passado 90 minutos.</exception>
        public void ValidateMatchJoinMatch(Matches match)
        {
            if (match == null)
            {
                throw new ArgumentException("A match não existe ou não foi encontrada");
            }

            var timeMatch = (DateTime.UtcNow - match.TimeStart.Value).TotalMinutes;
            if (timeMatch < 90)
            {
                throw new InvalidOperationException($"Ainda não passaram 90 minutos (Tempo Atual: {timeMatch} minutos / Tempo restante: {90 - timeMatch} minutos)");
            }
        }

        /// <summary>
        /// Valida as regras de negócio para entrar no Hub de Finalização.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Pertinência à equipa, prevenção de duplicação, e limite de admins no Hub.
        /// </remarks>
        /// <param name="teamMatchAdmin">As estatísticas da equipa (carregadas com dados de membros).</param>
        /// <param name="teamId">O ID da equipa que está a entrar.</param>
        /// <param name="hub">O dicionário ([ConcurrentDictionary]) que representa o estado atual do Hub.</param>
        /// <exception cref="ArgumentException">Se a equipa do admin for nula ou IDs não coincidirem.</exception>
        /// <exception cref="InvalidOperationException">Se a equipa já estiver no Hub ou o Hub estiver cheio.</exception>
        public void ValidateJoinMatch(TeamStatistics teamMatchAdmin, Guid teamId, ConcurrentDictionary<Guid, EntryHubFinishMatch> hub)
        {
            if (teamMatchAdmin == null)
            {
                throw new ArgumentException("A equipa do admin não foi encontrada");
            }

            if (teamMatchAdmin.IdTeam != teamId)
            {
                throw new InvalidOperationException("O id da team é diferente do da team que está a entrar no hub");
            }

            if (hub.ContainsKey(teamId))
            {
                throw new InvalidOperationException("Já existe um admin desta equipa a iniciar a partida");
            }

            if (hub.Count >= 2)
            {
                throw new InvalidOperationException("Apenas do 2 admins (um de cada equipa) pode aceder a esta funcionalidade");
            }
        }

        /// <summary>
        /// Valida as regras de negócio para atualizar (editar) um resultado submetido.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: O administrador que edita deve estar no Hub e a equipa deve existir.
        /// </remarks>
        /// <param name="teamMatchAdmin">As estatísticas da equipa.</param>
        /// <param name="teamId">O ID da equipa.</param>
        /// <param name="hub">O dicionário do lobby (deve conter a equipa).</param>
        /// <exception cref="ArgumentException">Se a equipa do admin for nula.</exception>
        /// <exception cref="InvalidOperationException">Se o admin não estiver no Hub.</exception>
        public void ValidateUpdateResult(TeamStatistics teamMatchAdmin, Guid teamId, ConcurrentDictionary<Guid, EntryHubFinishMatch> hub)
        {
            if (teamMatchAdmin == null)
            {
                throw new ArgumentException("A equipa do admin não foi encontrada");
            }

            if (teamMatchAdmin.IdTeam != teamId)
            {
                throw new InvalidOperationException("O id da team é diferente do da team que está a entrar no hub");
            }

            if (!hub.ContainsKey(teamId))
            {
                throw new InvalidOperationException("Não existe nenhum administrador dessa equipa no hub dessa match, logo não dá para alterar resultado nenhum");
            }

            if (hub.Count == 0)
            {
                throw new InvalidOperationException("O hub está vazio.");
            }
        }

        /// <summary>
        /// Valida se os resultados submetidos por ambas as equipas são consistentes em termos de adversários.
        /// </summary>
        /// <remarks>
        /// Verifica se o IdOpponent do primeiro resultado é o IdTeam do segundo, e vice-versa.
        /// </remarks>
        /// <param name="firstResult">O resultado submetido pela primeira equipa.</param>
        /// <param name="secondResult">O resultado submetido pela segunda equipa.</param>
        /// <exception cref="InvalidOperationException">Se houver inconsistência nos IDs dos adversários.</exception>
        public void ValidateMatchResultTwoTeams(ResultMatchDto firstResult, ResultMatchDto secondResult)
        {
            if (firstResult.IdOpponent != secondResult.IdTeam ||
                firstResult.IdTeam != secondResult.IdOpponent)
            {
                throw new InvalidOperationException("Um das equipas introduziu um adversário diferente da outra");
            }
        }

        /// <summary>
        /// Valida se a entidade de estatísticas do adversário existe.
        /// </summary>
        /// <param name="opponent">A entidade [TeamStatistics] do adversário.</param>
        /// <exception cref="ArgumentException">Lançada se o adversário for nulo.</exception>
        public void ValidateOpponentTeam(TeamStatistics opponent)
        {
            if (opponent == null)
            {
                throw new ArgumentException("O adversário não existe");
            }
        }
    }
}