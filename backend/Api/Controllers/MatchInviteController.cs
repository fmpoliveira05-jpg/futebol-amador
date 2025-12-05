using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.MatchInvites;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de convites para jogos amigáveis entre equipas.
    /// Permite enviar, aceitar, recusar e negociar convites de partida.
    /// </summary>
    [Authorize]
    [Route("api/[controller]/{idTeam:guid}")]
    [ApiController]
    [Produces("application/json")]
    public class MatchInviteController : ControllerBase
    {
        #region Initialization
        private readonly IMatchInviteService matchInviteService;
        private readonly IPlayerAuthorizationService AuthorizationService;

        /// <summary>
        /// Construtor do MatchInviteController.
        /// </summary>
        /// <param name="matchInviteService">Serviço responsável pela lógica de envio, aceitação e negociação de convites de partida.</param>
        /// <param name="authorizationService">Serviço responsável por validar se o utilizador tem permissões adequadas (ex: se é Admin da equipa).</param>
        public MatchInviteController(IMatchInviteService matchInviteService, IPlayerAuthorizationService authorizationService)
        {
            this.matchInviteService = matchInviteService;
            this.AuthorizationService = authorizationService;
        }

        #endregion

        #region EndPoints

        #region MatchInvites
        /// <summary>
        /// Envia um convite de partida amigável para outra equipa.
        /// </summary>
        /// <remarks>
        /// Apenas administradores da equipa remetente podem enviar convites.
        /// O sistema valida se já existe um convite pendente ou um jogo marcado para horário próximo.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa que envia o convite.</param>
        /// <param name="dto">Dados do convite (equipa destinatária, data, campo).</param>
        /// <returns>O convite criado com os detalhes confirmados.</returns>
        /// <response code="200">Convite enviado com sucesso.</response>
        /// <response code="400">Dados inválidos (ex: mesma equipa, data passada, conflito de horário).</response>
        /// <response code="401">Utilizador não autenticado.</response>
        /// <response code="403">Utilizador não é administrador da equipa.</response>
        [HttpPost("match-invites")]
        [ProducesResponseType(typeof(InfoMatchInviteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SendMatchInvite(Guid idTeam, [FromBody] SendMatchInviteDto dto)
        {
            await AuthorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);

            var sendInvite = await matchInviteService.SendMatchInvite(idTeam, dto);

            return Ok(sendInvite);
        }

        /// <summary>
        /// Aceita um convite de partida recebido.
        /// </summary>
        /// <remarks>
        /// Transforma o convite numa partida agendada (Scheduled Match).
        /// Apenas administradores da equipa recetora podem aceitar.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa que aceita o convite.</param>
        /// <param name="idMatchInvite">O ID do convite a aceitar (enviado no corpo).</param>
        /// <returns>Os detalhes da partida recém-criada.</returns>
        /// <response code="200">Convite aceite e partida agendada com sucesso.</response>
        /// <response code="400">Convite inválido ou expirado.</response>
        /// <response code="403">Utilizador não é administrador da equipa.</response>
        [HttpPost("AcceptMatchInvite")]
        [ProducesResponseType(typeof(MatchDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AcceptMatchInvite(Guid idTeam, [FromBody] Guid idMatchInvite)
        {
            await AuthorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);

            var match = await matchInviteService.AcceptMatchInvite(idTeam, idMatchInvite);
            
            return Ok(match);
        }

        /// <summary>
        /// Recusa e elimina um convite de partida recebido.
        /// </summary>
        /// <param name="idTeam">O ID da equipa que recusa o convite.</param>
        /// <param name="idMatchInvite">O ID do convite a recusar.</param>
        /// <response code="200">Convite recusado e removido com sucesso.</response>
        /// <response code="403">Utilizador não é administrador da equipa.</response>
        [HttpDelete("RefuseMatchInvite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RefuseMatchInvite(Guid idTeam, [FromBody] Guid idMatchInvite)
        {
            await AuthorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);
            await matchInviteService.RefuseMatchInvites(idTeam, idMatchInvite);
            return Ok();
        }

        /// <summary>
        /// Envia uma contra-proposta para negociar um convite existente (nova data ou campo).
        /// </summary>
        /// <remarks>
        /// Atualiza os detalhes do convite e inverte os papéis de remetente/destinatário.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa que está a negociar.</param>
        /// <param name="dto">Os novos detalhes propostos para o convite.</param>
        /// <returns>O convite atualizado.</returns>
        /// <response code="200">Negociação enviada com sucesso.</response>
        /// <response code="400">Dados inválidos ou sem alterações em relação à proposta anterior.</response>
        /// <response code="403">Utilizador não é administrador da equipa.</response>
        [HttpPut("Negociate")]
        [ProducesResponseType(typeof(InfoMatchInviteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> NegociateMatchInvite(Guid idTeam, [FromBody] SendMatchInviteDto dto)
        {
            await AuthorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);
            var matchInvite = await matchInviteService.NegociateMatchInvite(idTeam, dto);

            return Ok(matchInvite);
        }

        /// <summary>
        /// Obtém a lista de convites de partida recebidos pela equipa.
        /// </summary>
        /// <remarks>
        /// Permite filtrar os convites por nome do remetente ou intervalo de datas.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa.</param>
        /// <param name="filter">Critérios de filtragem opcionais.</param>
        /// <returns>Lista de convites recebidos.</returns>
        /// <response code="200">Lista retornada com sucesso.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<InfoMatchInviteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllMatchInvitesTeam(Guid idTeam, [FromQuery] FilterMatchInvitesDto filter)
        {
            IEnumerable<InfoMatchInviteDto> matchesInvite;

            bool hasFilter = !string.IsNullOrEmpty(filter.SenderName) ||
                                filter.MinDate.HasValue ||
                                filter.MaxDate.HasValue;

            if (hasFilter)
            {
                matchesInvite = await matchInviteService.GetAllMatchInvitesTeamWithFilters(idTeam, filter);
            }
            else
            {
                matchesInvite = await matchInviteService.GetAllMatchInvitesTeam(idTeam);
            }

            return Ok(matchesInvite);
        }
        #endregion

        #endregion

        #region Private Methods
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