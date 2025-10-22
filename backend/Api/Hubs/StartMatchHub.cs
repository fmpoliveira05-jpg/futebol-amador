using Application.Interfaces.Hub;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

/**
 Meter try catchs nas linhas que chamam funcionalidades do service
 
 Se calhar para resolver o problema do Lobby tenho de a classe atual ser userLoby 
 e criar uma classe Loby com o id dele, talvez a connectionString e também a Lista de playersLoby
 

namespace Api.Hubs
{
    //Descomentar isto quando houver aut para so pessoas autenticadas acederem
    //[Authorize]
    public class StartMatchHub: Hub<IStartMatchHub>
    {
        private readonly IManageStarMatchService startMatchManager; //Service com a logica
        private readonly IMatchRepository matchRepository; //Atualizar match
        private readonly IUnityOfWork unityOfWork;
        private static readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, string>> lobbies =
            new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, string>>();
   
        // Injeção do service de domínio
        public StartMatchHub(IManageStarMatchService startMatchManager, 
            IMatchRepository matchRepository,
            IUnityOfWork unityOfWork)
        {
            this.startMatchManager = startMatchManager;
            this.matchRepository = matchRepository;
        }

        /**
         Como o hub não fecha tentar encontrar forma de caso outra pessoa entra dar a mensagem
         
        public async Task JoinMatch(Guid idMatch)
        {
            var groupName = $"match-{idMatch}";
            var connectionId = Context.ConnectionId;
            var userId = Guid.Parse(Context.User.Identity.Name);

            var match = await matchRepository.GetMatchWithListPlayerById(idMatch);

            if (match == null)
            {
                //Meter throw ou mensagem
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

            var teamId = teamMatchAdmin.IdTeam;

            var lobby = lobbies.GetOrAdd(idMatch, new ConcurrentDictionary<Guid, string>());

            if (lobby.ContainsKey(teamId))
            {
                //await _notifier.SendErrorToCallerAsync(connectionId, "Já existe um admin desta equipa no lobby.");
                return;
            }

            if (lobby.Count == 0)
            {
                await Groups.AddToGroupAsync(connectionId, groupName);

                lobby.TryAdd(teamId, connectionId);
                //Ver como faço isto
                Context.Items["LobbyMatchId"] = idMatch; // O que guardar
                Context.Items["LobbyTeamId"] = teamId;   // O que guardar 

                //await _notifier.SendMessageToCallerAsync(connectionId, "Entraste no lobby. A aguardar oponente...");
            }
            else if (lobby.Count == 1)
            {

                await Groups.AddToGroupAsync(connectionId, groupName);

                // 4.2. Pega nos dados do primeiro admin
                var firstAdminEntry = lobby.First();
                var firstAdminTeamId = firstAdminEntry.Key;
                var firstAdminConnectionId = firstAdminEntry.Value;

                // 4.3. Atualiza a partida (Lógica de Negócio)
                match.MatchStatus = MatchStatus.IN_PROGRESS;
                match.TimeStart = DateTime.UtcNow;
                await unityOfWork.SaveChangesAsync(); // Salva a partida

                // 4.4. Limpa o lobby em memória (remove o 1º admin)
                lobby.TryRemove(firstAdminTeamId, out _);
                // (Opcional: limpa o lobby se estiver vazio)
                if (lobby.IsEmpty) lobbies.TryRemove(idMatch, out _);

                // 4.5. Limpa os Grupos SignalR
                await Groups.RemoveFromGroupAsync(firstAdminConnectionId, groupName);
                await Groups.RemoveFromGroupAsync(connectionId, groupName);

                //await _notifier.NotifyMatchStarted(groupName, idMatch);
            }
            ///Só manter se depois houver notificações
            //else
            //{
                //await _notifier.SendErrorToCallerAsync(connectionId, "Este lobby está cheio ou bloqueado.");
            //}

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
        
        private async Task<bool> HandleLeaveLobby()
        {
            if (Context.Items.TryGetValue("LobbyMatchId", out var matchIdObj) &&
                            Context.Items.TryGetValue("LobbyTeamId", out var teamIdObj))
            {
                var matchId = (Guid)matchIdObj;
                var teamId = (Guid)teamIdObj;
                var connectionId = Context.ConnectionId;

                // 2. Encontra o lobby em memória
                if (lobbies.TryGetValue(matchId, out var lobby))
                {
                    // 3. Remove a equipa do lobby em memória
                    if (lobby.TryRemove(teamId, out _))
                    {
                        // 4. Limpa o Grupo SignalR
                        var groupName = $"match-{matchId}";
                        await Groups.RemoveFromGroupAsync(connectionId, groupName);

                        // (Opcional: limpa o lobby se estiver vazio)
                        if (lobby.IsEmpty) lobbies.TryRemove(matchId, out _);

                        // ... Notificar oponente que saíste ...
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

*/