using Application.DTOs;
using Application.Interfaces.Hub;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Api.Hubs.SupporteEntities;

/*
 Falta aplicar logica está aqui apenas a estrutura estou a adiantar o service
 Criar validator para as validações
Posso mandar throws desta forma: "throw new HubException("A partida não foi encontrada.");"
 */
namespace Api.Hubs
{
    public class FinishMatchHub: Hub<IFinishMatchHub>
    {
        private readonly IManageStarMatchService startMatchManager; //Service com a logica
        private readonly IMatchRepository matchRepository; //Atualizar match
        private readonly IUnityOfWork unityOfWork;
        private readonly IMemoryCache cache;

        // Injeção do service de domínio
        public FinishMatchHub(IManageStarMatchService startMatchManager,
            IMatchRepository matchRepository, IMemoryCache memoryCache,
            IUnityOfWork unityOfWork)
        {
            this.startMatchManager = startMatchManager;
            this.matchRepository = matchRepository;
            this.cache = memoryCache;
            this.unityOfWork = unityOfWork;
        }

        //Dá uma duração de 10 minutos ao lobbie
        private MemoryCacheEntryOptions GetCacheOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
        }

        /**
         Como o hub não fecha tentar encontrar forma de caso outra pessoa entra dar a mensagem
         */
        public async Task JoinMatch(Guid idMatch, ResultMatchDto resultDto)
        {
            var groupName = $"matchFinish-{idMatch}";
            var connectionId = Context.ConnectionId;
            var userId = Guid.Parse(Context.User.Identity.Name);
            var match = await matchRepository.GetMatchWithListPlayerById(idMatch);

            if (match == null)
            {
                //throw
                return;
            }

            if (match.MatchStatus != MatchStatus.IN_PROGRESS)
            {
                //throw
                return;
            }

            //Validar se o player está em alguma das equipas (Talvez seja melhor dar mais)
            var teamMatchAdmin = match.Teams.FirstOrDefault(ts =>
                ts.Team.Members.Any(p => p.Id == userId && p.IsAdmin == true)
            );

            if (teamMatchAdmin == null)
            {
                //Throw
                return;
            }

            if (teamMatchAdmin.IdTeam != resultDto.IdTeam)
            {
                //throw 
                return;
            }

            var teamId = teamMatchAdmin.IdTeam;
            var lobbyCacheKey = $"lobbyFinishMatch-{idMatch}";

            if (!cache.TryGetValue(lobbyCacheKey, out FinishMatchLobbyState lobby))
            {
                lobby = new FinishMatchLobbyState();
            }

            var lobbyAdmins = lobby.Admins;
            if (lobbyAdmins.ContainsKey(teamId))
            {
                //await _notifier.SendErrorToCallerAsync(connectionId, "Já existe um admin desta equipa no lobby.");
                return;
            }

            if (lobbyAdmins.Count == 0)
            {
                await Groups.AddToGroupAsync(connectionId, groupName);
                var adminLobbyInfo = new AdminLobbyInfo(connectionId, userId, teamId, resultDto);
                lobbyAdmins.TryAdd(teamId, adminLobbyInfo);

                cache.Set(lobbyCacheKey, lobby, GetCacheOptions());

                Context.Items["LobbyMatchId"] = idMatch;
                Context.Items["LobbyTeamId"] = teamId;
            }
            else if (lobbyAdmins.Count == 1)
            {
                var firstAdminEntry = lobbyAdmins.First();
                var firstAdminTeamId = firstAdminEntry.Key;
                var firstAdminConnectionId = firstAdminEntry.Value.ConnectionId;
                var firstAdminConnectionResult = firstAdminEntry.Value.Result;

                if(firstAdminConnectionResult.IdTeam != resultDto.IdOpponent ||
                    firstAdminConnectionResult.IdOpponent != resultDto.IdTeam)
                {
                    //As teams não coincidem
                    return;
                }
                
                if (firstAdminConnectionResult.MyTeamGoals != resultDto.OpponentGoals ||
                    firstAdminConnectionResult.OpponentGoals != resultDto.MyTeamGoals)
                {
                    //Throw para os goals não coincidem
                    return;
                }

                // Atualiza a partida (Lógica de Negócio)
                match.MatchStatus = MatchStatus.DONE;
                await unityOfWork.SaveChangesAsync();
                cache.Remove(lobbyCacheKey);

                await Groups.RemoveFromGroupAsync(firstAdminConnectionId, groupName);
            }

            return;
        }

        public async Task LeaveMatch()
        {
            var connectionId = Context.ConnectionId;

            bool success = await HandleLeaveLobby();

            if (!success)
            {
                //Throw
                return;
                //await _notifier.SendMessageToCallerAsync(connectionId, "Saíste do lobby com sucesso.");
            }

            //Mensagem se necessário
            return;
            //await _notifier.SendErrorToCallerAsync(connectionId, "Não foi possível sair (não estavas em nenhum lobby).");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await HandleLeaveLobby();
            await base.OnDisconnectedAsync(exception);
        }

        /*
         * Contém a lógica de limpeza que é partilhada
         * Retorna 'true' se limpou algo, 'false' se não encontrou nada
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

                if (cache.TryGetValue(lobbyCacheKey, out FinishMatchLobbyState lobby))
                {
                    if (lobby.Admins.TryRemove(teamId, out _))
                    {
                        var groupName = $"lobbyFinishMatch-{matchId}";
                        await Groups.RemoveFromGroupAsync(connectionId, groupName);

                        if (lobby.Admins.IsEmpty)
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
