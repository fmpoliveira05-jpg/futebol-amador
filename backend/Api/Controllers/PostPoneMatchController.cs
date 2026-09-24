using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de pedidos de adiamento de partidas (Postponements).
    /// Permite consultar, aceitar ou rejeitar propostas de alteração de datas de jogos.
    /// </summary>
    [Authorize]
    [Route("api/Team/{idTeam:guid}/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class PostPoneMatchController : ControllerBase
    {
        #region Initialization
        private readonly IMatchService matchController;
        private readonly IPlayerAuthorizationService authorizationService;

        /// <summary>
        /// Construtor do PostPoneMatchController.
        /// </summary>
        /// <param name="matchController">Serviço responsável pela lógica de negócio das partidas e adiamentos.</param>
        /// <param name="authorizationService">Serviço responsável por validar se o utilizador tem permissões (ex: se é Admin da equipa).</param>
        public PostPoneMatchController(IMatchService matchController, IPlayerAuthorizationService authorizationService)
        {
            this.matchController = matchController;
            this.authorizationService = authorizationService;
        }
        #endregion

        #region EndPoints

        #region PostPoneMatch
        /// <summary>
        /// Obtém a lista de pedidos de adiamento de jogos (enviados ou recebidos) da equipa.
        /// </summary>
        /// <remarks>
        /// Apenas administradores da equipa podem consultar esta lista.
        /// Permite filtrar por adversário, data do jogo, data do adiamento, etc.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa.</param>
        /// <param name="filter">Filtros de pesquisa opcionais.</param>
        /// <returns>Lista de informações sobre os adiamentos.</returns>
        /// <response code="200">Lista retornada com sucesso.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        /// <response code="403">Utilizador não é administrador da equipa.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<InfoPostPoneMatch>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetListPostPoneMatchTeam(Guid idTeam, [FromQuery] FilterPostPoneMatchDto filter)
        {
            await authorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);
            IEnumerable<InfoPostPoneMatch> listPostPone;

            var isFilter = !string.IsNullOrEmpty(filter.NameOpponent) ||
                            filter.IsHome.HasValue ||
                            filter.MinDateGame.HasValue ||
                            filter.MaxDateGame.HasValue ||
                            filter.MinDatePostPoneGame.HasValue ||
                            filter.MaxDatePostPoneGame.HasValue;

            var userId = GetCurrentUserId();
            if (isFilter)
            {
                listPostPone = await matchController.GetListPostPoneMatchTeamWithFilters(idTeam, filter);
            }
            else
            {
                listPostPone = await matchController.GetListPostPoneMatchTeam(idTeam);
            }

            return Ok(listPostPone);
        }

        /// <summary>
        /// Aceita um pedido de adiamento de uma partida.
        /// </summary>
        /// <remarks>
        /// Ao aceitar, a data do jogo é atualizada para a nova data proposta e o estado do jogo volta a ser 'Agendado' (Scheduled).
        /// Apenas a equipa que recebeu o pedido pode aceitá-lo. Requer permissões de administrador.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa que aceita o adiamento.</param>
        /// <param name="dto">Dados para aceitação (ID do jogo, ID do oponente, Estado).</param>
        /// <returns>Os detalhes da partida atualizada com a nova data.</returns>
        /// <response code="200">Adiamento aceite e partida reagendada com sucesso.</response>
        /// <response code="400">Dados inválidos (ex: conflito de horários, pedido não encontrado).</response>
        /// <response code="403">Utilizador não é administrador ou não tem permissão para aceitar.</response>
        [HttpPost("AcceptPostponeMatch")]
        [ProducesResponseType(typeof(MatchDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AcceptPostponeMatch(Guid idTeam, [FromBody] AcceptRefusePostPoneDto dto)
        {
            await authorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);
            var match = await matchController.AcceptPostPoneMatch(idTeam, dto);

            return Ok(match);
        }

        /// <summary>
        /// Rejeita um pedido de adiamento de uma partida.
        /// </summary>
        /// <remarks>
        /// <b>Atenção:</b> Ao rejeitar o adiamento, a partida é <b>CANCELADA</b>.
        /// Apenas a equipa que recebeu o pedido pode rejeitá-lo. Requer permissões de administrador.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa que rejeita o adiamento.</param>
        /// <param name="dto">Dados para rejeição (ID do jogo, ID do oponente, Estado).</param>
        /// <response code="200">Pedido rejeitado e partida cancelada com sucesso.</response>
        /// <response code="400">Dados inválidos ou pedido não encontrado.</response>
        /// <response code="403">Utilizador não é administrador.</response>
        [HttpDelete("RejectPostponeMatch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RejectPostponeMatch(Guid idTeam, [FromBody] AcceptRefusePostPoneDto dto)
        {
            await authorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);
            await matchController.RejectPostPoneMatch(idTeam, dto);

            return Ok();
        }

        #endregion

        #endregion

        #region private Methods
        private string GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                throw new UnauthorizedAccessException("User ID not found in claims.");
            }

            return userId;
        }
        #endregion
    }
}
