using Application.Interfaces.Validators.Hub;
using Domain.Entities;
using System.Collections.Concurrent;

namespace Application.Validators.Hubs
{
    public class StartMatchValidator: IStartMatchHubValidator
    {
        public void ValidateJoinMatch(Matches match, Teams teamMatchAdmin, Guid idTeam, ConcurrentDictionary<Guid, string> lobby)
        {
            if (match == null)
            {
                throw new ArgumentNullException("A partida não foi encontrada");
            }

            if (teamMatchAdmin == null)
            {
                throw new ArgumentNullException("A equipa do admin não foi encontrada");
            }

            if (lobby.ContainsKey(idTeam))
            {
                throw new InvalidOperationException("Já existe um admin desta equipa a iniciar a partida");
            }
        }

        public void ValidateLeaveMatch(bool success)
        {
            if (!success)
            {
                throw new InvalidOperationException("Surguiu um problema a sair do lobby");
            }
        }

    }
}
