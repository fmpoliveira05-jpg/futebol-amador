using Application.Interfaces.Hub;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators.Hub;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

/*
 Criar um servce com a logica do loby
 Criar validator para as validações
Posso mandar throws desta forma: "throw new HubException("A partida não foi encontrada.");"
 */

namespace Api.Hubs
{
    //Descomentar isto quando houver aut para so pessoas autenticadas acederem
    //[Authorize]
    public class StartMatchHub: Hub<IStartMatchHub>
    {
        private readonly IManageStarMatchService startMatchManager; //Service com a logica
        private readonly IMatchRepository matchRepository; //Atualizar match
        private readonly IUnityOfWork unityOfWork;
        private readonly IStartMatchHubValidator startMatchHubValidator;
        private readonly IMemoryCache cache;
        

        // Injeção do service de domínio
        public StartMatchHub(IManageStarMatchService startMatchManager, 
            IMatchRepository matchRepository, IUnityOfWork unityOfWork,
            IStartMatchHubValidator startMatchHubValidator, IMemoryCache memoryCache)
        {
            this.startMatchManager = startMatchManager;
            this.matchRepository = matchRepository;
            this.unityOfWork = unityOfWork;
            this.startMatchHubValidator = startMatchHubValidator;
            this.cache = memoryCache;
        }

        //Dá uma duração de 10 minutos ao lobbie
        private MemoryCacheEntryOptions GetCacheOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        }

        /**
         Como o hub não fecha tentar encontrar forma de caso outra pessoa entra dar a mensagem
         */
        public async Task JoinStartMatch(Guid idMatch)
        {
            var groupName = $"match-{idMatch}";
            var connectionId = Context.ConnectionId;
            var userId = Guid.Parse(Context.User.Identity.Name);
            var lobbyCacheKey = $"lobby-{idMatch}";
            Guid teamId = Guid.Empty;
            Matches? match;
            ConcurrentDictionary<Guid, string> lobby;
            int lobbyCount = 0;

            try
            {
                match = await matchRepository.GetMatchWithListPlayerById(idMatch);
                var teamMatchAdmin = match?.Teams.FirstOrDefault(ts =>
                    ts.Team.Members.Any(p => p.Id == userId && p.IsAdmin == true)
                )?.Team;

                if (teamMatchAdmin != null)
                {
                    teamId = teamMatchAdmin.Id;
                }

                if (!cache.TryGetValue(lobbyCacheKey, out lobby))
                {
                    lobby = new ConcurrentDictionary<Guid, string>();
                }

                startMatchHubValidator.ValidateJoinMatch(match, teamMatchAdmin, teamId, lobby);
            }
            catch (ArgumentNullException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }

            lobbyCount = lobby.Count;
            if (lobbyCount == 0)
            {
                await Groups.AddToGroupAsync(connectionId, groupName);
                lobby.TryAdd(teamId, connectionId);

                cache.Set(lobbyCacheKey, lobby, GetCacheOptions());

                Context.Items["LobbyMatchId"] = idMatch;
                Context.Items["LobbyTeamId"] = teamId;
            }
            else if (lobbyCount == 1)
            {
                var firstAdminEntry = lobby.First();
                var firstAdminTeamId = firstAdminEntry.Key;
                var firstAdminConnectionId = firstAdminEntry.Value;

                // Atualiza a partida (Lógica de Negócio)
                match.MatchStatus = MatchStatus.IN_PROGRESS;
                match.TimeStart = DateTime.UtcNow;
                await unityOfWork.SaveChangesAsync();
                cache.Remove(lobbyCacheKey);

                await Groups.RemoveFromGroupAsync(firstAdminConnectionId, groupName);
            }

            return;
        }

        public async Task LeaveStartMatch()
        {
            try
            {
                bool success = await HandleLeaveLobby();
                startMatchHubValidator.ValidateLeaveMatch(success);
            }
            catch (ArgumentNullException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }

            return;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await HandleLeaveLobby();
            await base.OnDisconnectedAsync(exception);
        }

        /*
         * Contém a lógica de limpeza que é partilhada
         * Retorna 'true' se limpou algo, 'false' se não encontrou nada
         * 
         * Ver se tem sentido um validator!!!!!
        */
        private async Task<bool> HandleLeaveLobby()
        {
            if (Context.Items.TryGetValue("LobbyMatchId", out var matchIdObj) &&
                Context.Items.TryGetValue("LobbyTeamId", out var teamIdObj))
            {
                var matchId = (Guid)matchIdObj;
                var teamId = (Guid)teamIdObj;
                var connectionId = Context.ConnectionId;
                var lobbyCacheKey = $"lobby-{matchId}";

                if (cache.TryGetValue(lobbyCacheKey, out ConcurrentDictionary<Guid, string> lobby))
                {
                    if (lobby.TryRemove(teamId, out _))
                    {
                        var groupName = $"match-{matchId}";
                        await Groups.RemoveFromGroupAsync(connectionId, groupName);

                        if (lobby.IsEmpty)
                        {
                            cache.Remove(lobbyCacheKey);
                        }
                        else
                        {
                            cache.Set(lobbyCacheKey, lobby, GetCacheOptions());
                        }
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

