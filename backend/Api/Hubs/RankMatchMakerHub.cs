using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Hub;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators.Hub;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    /// <summary>
    /// Hub SignalR responsável pelo sistema de Matchmaking (Procura de Jogo) para partidas competitivas (Ranked).
    /// 
    /// Funciona como um Lobby de espera onde as equipas entram para procurar adversários.
    /// O emparelhamento real é gerido por um serviço de background, mas este Hub gere as conexões,
    /// grupos e notificações em tempo real para os clientes.
    /// </summary>
    [Authorize]
    public class RankMatchMakerHub : Hub<IRankMatchMakerHub>
    {
        private readonly IManagerRankMatchMakerService service;
        private readonly IGeralHubValidator geralValidator;

        /// <summary>
        /// Construtor do RankMatchMakerHub.
        /// </summary>
        /// <param name="service">Serviço de gestão do estado do Matchmaking (quem está à procura).</param>
        /// <param name="geralValidator">Validador para regras gerais de Hub.</param>
        public RankMatchMakerHub(IManagerRankMatchMakerService service, IGeralHubValidator geralValidator)
        {
            this.service = service;
            this.geralValidator = geralValidator;
        }

        /// <summary>
        /// Método invocado por um cliente para iniciar a procura de jogo (Entrar no Lobby).
        /// 
        /// Fluxo:
        /// 1. O cliente envia os critérios de procura ([StartSearchDto]).
        /// 2. O serviço tenta encontrar um par imediato ou adiciona a equipa à fila de espera.
        /// 3. Se encontrar par, notifica ambas as equipas e remove-as do lobby.
        /// 4. Se não encontrar, o cliente fica no grupo à espera de notificações do Background Service.
        /// </summary>
        /// <param name="startSearch">DTO com os dados da equipa e preferências de horário.</param>
        /// <exception cref="HubException">Se os dados de entrada forem inválidos ou a equipa já estiver em jogo.</exception>
        public async Task JoinRankMatchMaker(StartSearchDto startSearch)
        {
            var connectionId = Context.ConnectionId;
            var userId = Context.User.Identity.Name;
            EntryRankMatchMakerHub result;
            string groupName = "";
            var idTeam = startSearch.IdTeam;
            var hoursGame = startSearch.HoursGame;

            //Ver se tenho mais alguma exception
            try
            {
                result = await service.JoinRankMatchMaker(userId, idTeam, hoursGame, connectionId);
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }

            Context.Items[ModelConstants.RankMatchMakerHubConst.ContentTeamId] = idTeam;

            if (result.ConnectionId == connectionId)
            {
                groupName = GetGroupName(idTeam);
            }
            else
            {
                groupName = GetGroupName(result.Team.IdTeam);
            }

            await Groups.AddToGroupAsync(connectionId, groupName);

            if (result.ConnectionId != connectionId)
            {
                //Notificar os dois teams, ver no startMatch
                await CleanHub(groupName, result.ConnectionId, connectionId);
            }
        }

        /// <summary>
        /// Método invocado explicitamente pelo cliente para cancelar a procura de jogo (Sair do Lobby).
        /// </summary>
        /// <exception cref="HubException">Se ocorrer erro ao processar a saída.</exception>
        public async Task LeaveRankMatchMaker()
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
        /// Garante que a equipa é removida da fila de espera para não ser emparelhada "fantasma".
        /// </summary>
        /// <param name="exception">A exceção que causou a desconexão, se houver.</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Guid? idTeam = null;

            if (Context.Items.TryGetValue(ModelConstants.RankMatchMakerHubConst.ContentTeamId, out var t) && t is Guid tg)
            {
                idTeam = tg;
            }

            await service.HandleDisconnectAsync(idTeam, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Lógica auxiliar encapsulada para remover o utilizador do Hub e do Grupo SignalR.
        /// </summary>
        /// <returns><c>true</c> se a remoção foi bem-sucedida; <c>false</c> caso contrário.</returns>
        private async Task<bool> HandleLeaveHub()
        {
            if (Context.Items.TryGetValue(ModelConstants.RankMatchMakerHubConst.ContentTeamId, out var teamIdObj))
            {
                var teamId = (Guid)teamIdObj;
                var connectionId = Context.ConnectionId;

                var removed = await service.LeaveRankMatchMakerAsync(teamId, connectionId);

                if (removed)
                {
                    var groupName = GetGroupName(teamId);
                    await Groups.RemoveFromGroupAsync(connectionId, groupName);

                    Context.Items.Remove(ModelConstants.RankMatchMakerHubConst.ContentTeamId);
                }

                return removed;
            }

            return false;
        }

        /// <summary>
        /// Gera o nome padronizado para o grupo de matchmaking de uma equipa.
        /// </summary>
        /// <param name="idTeam">O ID da equipa.</param>
        /// <returns>Uma string no formato "RankMatchhub-{Guid}".</returns>
        private static string GetGroupName(Guid idTeam)
        {
            return ModelConstants.RankMatchMakerHubConst.PrefixGroupName + idTeam;
        }

        /// <summary>
        /// Remove ambos os participantes do grupo SignalR e limpa o contexto.
        /// Chamado quando um emparelhamento é bem-sucedido.
        /// </summary>
        /// <param name="groupName">O nome do grupo.</param>
        /// <param name="fisrtAdminConnectionId">ID da conexão da primeira equipa (Host).</param>
        /// <param name="SecondAdminConnectionId">ID da conexão da segunda equipa (Challenger).</param>
        private async Task CleanHub(string groupName, string? fisrtAdminConnectionId, string SecondAdminConnectionId)
        {
            if (!string.IsNullOrEmpty(fisrtAdminConnectionId))
            {
                await Groups.RemoveFromGroupAsync(fisrtAdminConnectionId, groupName);
            }

            await Groups.RemoveFromGroupAsync(SecondAdminConnectionId, groupName);

            Context.Items.Remove(ModelConstants.RankMatchMakerHubConst.ContentTeamId);
        }
    }
}