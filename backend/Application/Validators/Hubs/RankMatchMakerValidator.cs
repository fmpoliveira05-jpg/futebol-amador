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
        public void ValidateVariableJoinRankMatchMaker(Guid idPlayer, Guid idTeam, string connectionId)
        {
            if (idPlayer == Guid.Empty)
            {
                throw new ArgumentException("O id do jogador está vazio");
            }

            if (idTeam == Guid.Empty)
            {
                throw new ArgumentException("O id da equipa está vazio");
            }

            if (string.IsNullOrEmpty(connectionId))
            {
                throw new ArgumentException("A connection string está vazia");
            }
        }

        public void ValidateTeamJoinRankMatchMaker(Teams? team)
        {
            if (team == null)
            {
                throw new ArgumentException("A equipa está nula");
            }
        }

        public void ValidateJoinRankMatchMaker(Teams team, float averageAge, bool? findUser, ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub)
        {
            if(!findUser.HasValue || !findUser.Value)
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

            if (hub.ContainsKey(team.Id))
            {
                throw new InvalidOperationException("Já existe um admin desta equipa a iniciar a partida");
            }
        }

        public void ValidateLeaveRankMatchMaker(Guid idTeam)
        {
            throw new NotImplementedException();
        }
    }
}
