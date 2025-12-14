using Application.DTOs.Match;
using Application.Hubs;
using Application.Interfaces.Hub;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators.Hub;
using Domain.Constants;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    /// <summary>
    /// Hub SignalR responsável pela coordenação da finalização de uma partida (Finish Match).
    /// 
    /// Este hub gere o processo de submissão de resultados finais por parte dos administradores das equipas.
    /// O objetivo é garantir que ambas as equipas concordam com o resultado (número de golos) antes de oficializar o fim do jogo.
    /// </summary>
    public class FinishMatchHub : Hub<IFinishMatchHub>
    {
        private readonly IManagerFinishMatchService managerFinishMatchService;
        private readonly IHubFinshMatchValidator validator;
        private readonly IGeralHubValidator geralValidator;
        private readonly ILogger<FinishMatchHub> _logger; 

        /// <summary>
        /// Construtor do FinishMatchHub.
        /// </summary>
        /// <param name="managerFinishMatchService">Serviço que gere o estado dos resultados submetidos.</param>
        /// <param name="validator">Validador específico para regras de finalização de jogo.</param>
        /// <param name="geralValidator">Validador para regras gerais de Hub.</param>
        public FinishMatchHub(IManagerFinishMatchService managerFinishMatchService,
            IHubFinshMatchValidator validator, IGeralHubValidator geralValidator, ILogger<FinishMatchHub> logger)
        {
            this.managerFinishMatchService = managerFinishMatchService;
            this.validator = validator;
            this.geralValidator = geralValidator;
            this._logger = logger;
        }

        /// <summary>
        /// Método invocado por um cliente (Admin) para submeter o resultado final do jogo.
        /// 
        /// Fluxo:
        /// 1. O admin envia o resultado ([ResultMatchDto]).
        /// 2. O serviço armazena o resultado e verifica se a outra equipa já submeteu.
        /// 3. Se ambos submeteram e os resultados coincidem ([result.IsCoincides]), o jogo é finalizado e o grupo é limpo.
        /// </summary>
        /// <param name="finishMatch">DTO contendo os golos e IDs da partida/equipa.</param>
        /// <exception cref="HubException">Se os dados forem inválidos ou o estado do jogo não permitir finalização.</exception>
        public async Task JoinFinishMatch(ResultMatchDto finishMatch)
        {
            var connectionId = Context.ConnectionId;
            var userId = Context.UserIdentifier;
            var idMatch = finishMatch.IdMatch;
            var groupName = GetGroupName(idMatch);
            JoinFinishMatch result;

            _logger.LogInformation($"[Hub Join] Recebido pedido para Match {idMatch} da conexão {connectionId}");

            try
            {
                result = await managerFinishMatchService.JoinHubAsync(idMatch, finishMatch, userId, connectionId);
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }

            await Groups.AddToGroupAsync(connectionId, groupName);

            Context.Items[ModelConstants.FinishMatchHubConst.ContentMatchId] = idMatch;
            Context.Items[ModelConstants.FinishMatchHubConst.ContentTeamId] = result.IdTeam;

            if (result.IsCoincides.HasValue && result.IsCoincides.Value == true)
            {
                _logger.LogInformation($"[Hub Join] Coincidência confirmada! Enviando 'ReceiveFinishMatch' para o grupo {groupName}.");
                await Clients.Group(groupName).ReceiveFinishMatch(true);
                await CleanHub(groupName, result.FirstAdminConnectionId, connectionId);
            }
            else
            {
                _logger.LogInformation($"[Hub Join] Aguardando oponente ou correção. (IsCoincides: {result.IsCoincides})");
            }
        }

        /// <summary>
        /// Método invocado por um cliente para editar um resultado previamente submetido (correção de erro).
        /// </summary>
        /// <param name="finishMatch">O novo DTO de resultado corrigido.</param>
        /// <exception cref="HubException">Se a edição não for permitida (ex: jogo já fechado).</exception>
        public async Task EditResult(ResultMatchDto finishMatch)
        {
            var connectionId = Context.ConnectionId;
            var userId = Context.UserIdentifier;
            var idMatch = finishMatch.IdMatch;
            var groupName = GetGroupName(idMatch);
            JoinFinishMatch result;

            try
            {
                result = await managerFinishMatchService.UpdateResult(idMatch, finishMatch, userId, connectionId);
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }

            try
            {
                validator.ValidateIsCoincide(result.IsCoincides);
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
            }

            if (result.IsCoincides.HasValue && result.IsCoincides.Value == true)
            {
                await Clients.Group(groupName).ReceiveFinishMatch(true);
                await CleanHub(groupName, result.FirstAdminConnectionId, connectionId);
            }
        }

        /// <summary>
        /// Método invocado explicitamente pelo cliente para sair do ecrã de finalização sem concluir.
        /// </summary>
        /// <exception cref="HubException">Se ocorrer erro ao processar a saída.</exception>
        public async Task LeaveFinishMatch()
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
        /// Garante que os dados temporários de finalização são limpos ou marcados como pendentes.
        /// </summary>
        /// <param name="exception">A exceção que causou a desconexão, se houver.</param>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Guid? matchId = null;
            Guid? teamId = null;

            //Limpeza do Context
            if (Context.Items.TryGetValue(ModelConstants.FinishMatchHubConst.ContentMatchId, out var m) && m is Guid g)
            {
                matchId = g;
            }

            if (Context.Items.TryGetValue(ModelConstants.FinishMatchHubConst.ContentTeamId, out var t) && t is Guid tg)
            {
                teamId = tg;
            }

            await managerFinishMatchService.HandleDisconnectAsync(matchId, teamId, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        #region Private Methods
        /// <summary>
        /// Lógica auxiliar para remover o utilizador do Hub e do Grupo SignalR.
        /// </summary>
        /// <returns><c>true</c> se a remoção foi bem-sucedida; <c>false</c> caso contrário.</returns>
        private async Task<bool> HandleLeaveHub()
        {
            if (Context.Items.TryGetValue(ModelConstants.FinishMatchHubConst.ContentMatchId, out var matchIdObj) &&
                Context.Items.TryGetValue(ModelConstants.FinishMatchHubConst.ContentTeamId, out var teamIdObj))
            {
                var matchId = (Guid)matchIdObj;
                var teamId = (Guid)teamIdObj;
                var connectionId = Context.ConnectionId;

                //Remoção do admin do hub
                var removed = await managerFinishMatchService.LeaveHubAsync(matchId, teamId, connectionId);

                if (removed)
                {
                    var groupName = GetGroupName(matchId);
                    await Groups.RemoveFromGroupAsync(connectionId, groupName);

                    // limpar context items
                    Context.Items.Remove(ModelConstants.FinishMatchHubConst.ContentMatchId);
                    Context.Items.Remove(ModelConstants.FinishMatchHubConst.ContentTeamId);
                }

                return removed;
            }

            return false;
        }

        /// <summary>
        /// Gera o nome padronizado para o grupo de finalização de partida.
        /// </summary>
        /// <param name="idMatch">O ID da partida.</param>
        /// <returns>Uma string no formato "hubFinishMatch-{Guid}".</returns>
        private static string GetGroupName(Guid idMatch)
        {
            return ModelConstants.FinishMatchHubConst.PrefixGroupName + idMatch;
        }

        /// <summary>
        /// Remove ambos os administradores do grupo e limpa o contexto, sinalizando o fim do processo.
        /// </summary>
        /// <param name="groupName">O nome do grupo.</param>
        /// <param name="fisrtAdminConnectionId">ID da conexão do primeiro admin.</param>
        /// <param name="SecondAdminConnectionId">ID da conexão do segundo admin.</param>
        private async Task CleanHub(string groupName, string? fisrtAdminConnectionId, string SecondAdminConnectionId)
        {
            if (!string.IsNullOrEmpty(fisrtAdminConnectionId))
            {
                await Groups.RemoveFromGroupAsync(fisrtAdminConnectionId, groupName);
            }

            await Groups.RemoveFromGroupAsync(SecondAdminConnectionId, groupName);

            Context.Items.Remove(ModelConstants.FinishMatchHubConst.ContentMatchId);
            Context.Items.Remove(ModelConstants.FinishMatchHubConst.ContentTeamId);
        }
        #endregion
    }
}