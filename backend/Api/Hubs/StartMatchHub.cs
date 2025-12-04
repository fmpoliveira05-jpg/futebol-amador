using Application.Hubs;
using Application.Interfaces.Hub;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators.Hub;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    /// <summary>
    /// Hub SignalR responsável por coordenar o início de uma partida (Start Match).
    /// 
    /// Este hub gere o "handshake" entre os administradores das duas equipas envolvidas num jogo.
    /// Funciona como uma sala de espera: o primeiro admin entra e aguarda; quando o segundo entra,
    /// o jogo é marcado como iniciado e ambos são notificados.
    /// </summary>
    [Authorize]
    public class StartMatchHub : Hub<IStartMatchHub>
    {
        private readonly IManagerStartMatchService startMatchManager;
        private readonly IGeralHubValidator geralValidator;

        /// <summary>
        /// Construtor do StartMatchHub.
        /// </summary>
        /// <param name="startMatchManager">Serviço que gere a lógica de estado do lobby (quem está à espera, quem entrou).</param>
        /// <param name="geralValidator">Validador para regras gerais de Hub (ex: validação de saída).</param>
        public StartMatchHub(IManagerStartMatchService startMatchManager, IGeralHubValidator geralValidator)
        {
            this.startMatchManager = startMatchManager;
            this.geralValidator = geralValidator;
        }

        /// <summary>
        /// Método invocado por um cliente (Admin de Equipa) para sinalizar que está pronto para começar o jogo.
        /// 
        /// Fluxo:
        /// 1. O cliente envia o ID do jogo e da sua equipa.
        /// 2. Se for o primeiro a chegar, é adicionado ao grupo e fica à espera.
        /// 3. Se for o segundo, o jogo inicia ([result.MatchStarted]) e ambos recebem a notificação [ReceiveStartMatch].
        /// </summary>
        /// <param name="idMatch">O ID da partida a iniciar.</param>
        /// <param name="idTeam">O ID da equipa do administrador que está a chamar.</param>
        /// <exception cref="HubException">Se os argumentos forem inválidos ou o estado do jogo não permitir o início.</exception>
        public async Task JoinStartMatch(Guid idMatch, Guid idTeam)
        {
            var connectionId = Context.ConnectionId;
            var userId = Context.User.Identity.Name;
            var groupName = GetGroupName(idMatch);
            JoinStartMatchResult result;

            try
            {
                result = await startMatchManager.JoinHubAsync(idMatch, userId, idTeam, connectionId);
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }

            if (result.IsFirstAdmin)
            {
                await Groups.AddToGroupAsync(connectionId, groupName);

                Context.Items["HubMatchId"] = idMatch;
                Context.Items["HubTeamId"] = result.TeamId;
            }
            else if (result.MatchStarted)
            {
                if (!string.IsNullOrEmpty(result.FirstAdminConnectionId))
                {
                    await Groups.RemoveFromGroupAsync(result.FirstAdminConnectionId, groupName);
                }
                await Clients.Group(groupName).ReceiveStartMatch("O jogo começou!");
            }
        }

        /// <summary>
        /// Método invocado explicitamente pelo cliente para sair do processo de início de jogo (Cancelar espera).
        /// </summary>
        /// <exception cref="HubException">Se ocorrer erro ao processar a saída.</exception>
        public async Task LeaveStartMatch()
        {
            try
            {
                bool success = await HandleLeaveHub();
                geralValidator.ValidateLeaveMatch(success);
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }
        }

        /// <summary>
        /// Chamado automaticamente quando a conexão cai ou o cliente fecha a app.
        /// Garante a limpeza do estado no [startMatchManager] para não deixar jogos "pendurados".
        /// </summary>
        /// <param name="exception">A exceção que causou a desconexão, se houver.</param>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //tentar cleanup via Context items
            Guid? matchId = null;
            Guid? teamId = null;

            if (Context.Items.TryGetValue("LobbyMatchId", out var m) && m is Guid g)
            {
                matchId = g;
            }

            if (Context.Items.TryGetValue("LobbyTeamId", out var t) && t is Guid tg)
            {
                teamId = tg;
            }

            await startMatchManager.HandleDisconnectAsync(matchId, teamId, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Lógica auxiliar encapsulada para remover o utilizador do Hub e do Grupo SignalR.
        /// </summary>
        /// <returns><c>true</c> se a remoção foi bem-sucedida e havia dados para limpar; <c>false</c> caso contrário.</returns>
        private async Task<bool> HandleLeaveHub()
        {
            if (Context.Items.TryGetValue("HubMatchId", out var matchIdObj) &&
                Context.Items.TryGetValue("HubTeamId", out var teamIdObj))
            {
                var matchId = (Guid)matchIdObj;
                var teamId = (Guid)teamIdObj;
                var connectionId = Context.ConnectionId;

                //Remoção do admin do hub
                var removed = await startMatchManager.LeaveHubAsync(matchId, teamId, connectionId);

                if (removed)
                {
                    var groupName = GetGroupName(matchId);
                    await Groups.RemoveFromGroupAsync(connectionId, groupName);

                    // limpar context items
                    Context.Items.Remove("HubMatchId");
                    Context.Items.Remove("HubTeamId");
                }

                return removed;
            }

            return false;
        }

        /// <summary>
        /// Gera o nome padronizado para o grupo SignalR de uma partida.
        /// </summary>
        /// <param name="idMatch">O ID da partida.</param>
        /// <returns>Uma string no formato "StartMatchhub-{Guid}".</returns>
        private static string GetGroupName(Guid idMatch)
        {
            return ModelConstants.StartMatchHubConst.PrefixGroupName + idMatch;
        }
    }
}