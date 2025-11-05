using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Validators.Hub;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using System.Collections.Concurrent;

namespace Application.Validators.Hubs
{
    public class RankMatchMakerValidator : IRankMatchMakerValidator
    {
        public void ValidateVariableJoinRankMatchMaker(string idPlayer, Guid idTeam, TimeOnly hoursGame, string connectionId)
        {
            if (idPlayer == string.Empty)
            {
                throw new ArgumentException("O id do jogador está vazio");
            }

            if (idTeam == Guid.Empty)
            {
                throw new ArgumentException("O id da equipa está vazio");
            }

            if (hoursGame != ModelConstants.HoursValidToCompetitiveMatch.MORNING &&
                hoursGame != ModelConstants.HoursValidToCompetitiveMatch.AFTERNOON &&
                hoursGame != ModelConstants.HoursValidToCompetitiveMatch.NIGHT)
            {
                throw new ArgumentException($"A hora da partida não corrresponde a uma hora válida para uma partida (horas válidas {ModelConstants.HoursValidToCompetitiveMatch.MORNING}, {ModelConstants.HoursValidToCompetitiveMatch.AFTERNOON} ou {ModelConstants.HoursValidToCompetitiveMatch.NIGHT}");
            }

            if (string.IsNullOrEmpty(connectionId))
            {
                throw new ArgumentException("A connection string está vazia");
            }
        }

        public void ValidateHoursToMatch(double differenteHoursNowAndGame, Matches match)
        {
            if (match != null)
            {
                throw new InvalidOperationException("Não pode marcar um jogo para este domingo uma vez que já tem um jogo marcado a pelo menos 12 horas do jogo a marcar");
            }

            if (differenteHoursNowAndGame <= 12)
            {
                throw new InvalidOperationException("Não pode marcar um jogo para este domingo, porque está a menos de 12 horas da hora do jogo");
            }
        }

        public void ValidateTeamJoinRankMatchMaker(Team? team)
        {
            if (team == null)
            {
                throw new ArgumentException("A equipa está nula");
            }
        }

        public void ValidateJoinRankMatchMaker(Team team, float averageAge, string city, bool? findUser, ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub)
        {
            if (!findUser.HasValue || !findUser.Value)
            {
                throw new NotFindException("O administrador que quer procurar uma partida ranqueada não existe");
            }

            if (team.Members?.Count() < 11)
            {
                throw new InvalidOperationException("A equipa não pode jogar partidas rankeadas, porque ainda não tem no mínimo 11 jogadores");
            }

            if (averageAge < ModelConstants.TeamConst.MinAverageAge || averageAge > ModelConstants.TeamConst.MaxAverageAge)
            {
                throw new ArgumentException($"A equipa não possui a idade média permitida ({averageAge})");
            }

            if (string.IsNullOrEmpty(city))
            {
                throw new ArgumentException("A cidade do campo da equipa está ou não foi encontrada na morada");
            }

            if (hub.ContainsKey(team.Id))
            {
                throw new InvalidOperationException("Já existe um admin desta equipa a iniciar a partida");
            }
        }

        public void ValidateLeaveRankMatchMaker(Guid idTeam)
        {
            if (idTeam == Guid.Empty)
            {
                throw new ArgumentException("O id da equipa está vazio");
            }
        }
    }
}
