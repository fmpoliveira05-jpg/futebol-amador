using Application.Interfaces.Validators.Hub;
using Domain.Entities;
using System.Collections.Concurrent;

namespace Application.Validators.Hubs
{
    public class StartMatchHubValidator: IStartMatchHubValidator
    {
        public void ValidateVariableJoinMatch(Guid matchId, Guid userId, string connectionId)
        {
            if (matchId == Guid.Empty)
            {
                throw new ArgumentNullException("O id da partida está null");
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException("O id do utilizador está a null");
            }

            if (string.IsNullOrEmpty(connectionId))
            {
                throw new ArgumentNullException("A connection string está a null ou vazia");
            }
        }

        public void ValidateMatchJoinMatch(Matches match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("A partida não foi encontrada");
            }
        }
        public void ValidateJoinMatch(TeamStatistics teamMatchAdmin, Guid idTeam, ConcurrentDictionary<Guid, string> hub)
        {
            if (teamMatchAdmin == null)
            {
                throw new ArgumentNullException("A equipa do admin não foi encontrada");
            }

            if (teamMatchAdmin.IdTeam != idTeam)
            {
                throw new InvalidOperationException("O id da team é diferente do da team que está a entrar no hub");
            }

            if (hub.ContainsKey(idTeam))
            {
                throw new InvalidOperationException("Já existe um admin desta equipa a iniciar a partida");
            }

            if (hub.Count() >= 2)
            {
                throw new InvalidOperationException("Apenas do 2 admins (um de cada equipa) pode aceder a esta funcionalidade");
            }
        }
    }
}
