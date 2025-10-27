using Application.DTOs;
using Application.Hubs;
using Application.Interfaces.Validators.Hub;
using Domain.Entities;
using System.Collections.Concurrent;

namespace Application.Validators.Hubs
{
    public class FinishMatchValidator : IFinishMatchValidator
    {
        public void ValidateVariableJoinMatch(Guid matchId, ResultMatchDto finishMatch, Guid userId, string connectionId)
        {
            if (matchId == Guid.Empty)
            {
                throw new ArgumentException("O id da partida está null");
            }

            if (userId == Guid.Empty)
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

        public void ValidateMatchJoinMatch(Matches match)
        {
            if (match == null)
            {
                throw new ArgumentException("A match não existe ou não foi encontrada");
            }
        }

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

            if (hub.Count() >= 2) {
                throw new InvalidOperationException("Apenas do 2 admins (um de cada equipa) pode aceder a esta funcionalidade");
            }
        }

        public void ValidateMatchResultTwoTeams(ResultMatchDto firstResult, ResultMatchDto secondResult)
        {
            if (firstResult.IdOpponent != secondResult.IdTeam ||
                firstResult.IdTeam != secondResult.IdOpponent)
            {
                throw new InvalidOperationException("Um das equipas introduziu um adversário diferente da outra");
            }
        }

        public void ValidateOpponentTeam(TeamStatistics opponent)
        {
            if (opponent == null)
            {
                throw new ArgumentException("O adversário não existe");
            }
        }
    }
}