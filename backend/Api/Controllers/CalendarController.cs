using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub.ClienteService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão do calendário, partidas e eventos em tempo real de uma equipa.
    /// </summary>
    [Authorize]
    [Route("api/[controller]/{idTeam:guid}")]
    [ApiController]
    [Produces("application/json")]
    public class CalendarController : ControllerBase
    {
        #region Inicializer
        private readonly IMatchService matchController;
        private readonly IPlayerAuthorizationService authorizationService;

        /// <summary>
        /// Construtor do CalendarController.
        /// </summary>
        /// <param name="matchController">Serviço responsável pela lógica de negócio das partidas e gestão do calendário.</param>
        /// <param name="authorizationService">Serviço responsável pela validação de permissões (ex: verificar se o utilizador pertence à equipa).</param>
        public CalendarController(IMatchService matchController, IPlayerAuthorizationService authorizationService)        {
            this.matchController = matchController;
            this.authorizationService = authorizationService;
        }
        #endregion

        #region Calendar
        /// <summary>
        /// Obtém o calendário de jogos de uma equipa com opção de filtros.
        /// </summary>
        /// <remarks>
        /// Apenas membros da equipa podem aceder a esta informação.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa cujo calendário se pretende consultar.</param>
        /// <param name="filters">Filtros opcionais para pesquisar jogos (por data, adversário, estado, etc.).</param>
        /// <returns>Uma lista de objetos <see cref="InfoMatchCalendar"/> representando os jogos agendados ou realizados.</returns>
        /// <response code="200">Retorna a lista de jogos com sucesso.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        /// <response code="403">Utilizador não é membro da equipa especificada.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<InfoMatchCalendar>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CalendarTeam(Guid idTeam, [FromQuery] FilterCalendarDto filters)
        {
            await authorizationService.UserAuthorizationIsMemberTeamById(GetCurrentUserId(), idTeam);
            IEnumerable<InfoMatchCalendar> matches;
            var hasFilter = filters.IsRealized != null ||
                                filters.IsRanqued != null ||
                                filters.IsHome != null ||
                                filters.MinDate.HasValue ||
                                filters.MaxDate.HasValue ||
                                !string.IsNullOrEmpty(filters.NameOpponent);

            

            if (hasFilter)
            {
                matches = await matchController.GetCalendarWithFilters(idTeam, filters);
            }
            else 
            {
                matches = await matchController.GetCalendar(idTeam);
            }
                    
            return Ok(matches);  
        }

        /// <summary>
        /// Obtém os detalhes de uma partida específica.
        /// </summary>
        /// <remarks>
        /// Este endpoint é público (AllowAnonymous), permitindo que qualquer pessoa veja os detalhes básicos de um jogo.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa (para contexto).</param>
        /// <param name="idMatch">O ID da partida a consultar.</param>
        /// <returns>Um objeto <see cref="InfoMatch"/> com os detalhes da partida.</returns>
        /// <response code="200">Detalhes da partida retornados com sucesso.</response>
        /// <response code="404">Partida não encontrada.</response>
        [HttpGet("{idMatch}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(InfoMatch), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMatchTeam(Guid idTeam, Guid idMatch)
        {
            var match = await matchController.GetMatchById(idTeam, idMatch);
            return Ok(match);
        }

        #endregion

        #region PostPoneMatch
        /// <summary>
        /// Solicita ou confirma o adiamento de uma partida.
        /// </summary>
        /// <remarks>
        /// Apenas administradores da equipa podem adiar jogos.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa que está a solicitar o adiamento.</param>
        /// <param name="dto">Dados do adiamento (ID do jogo, nova data proposta).</param>
        /// <returns>Objeto com a informação do adiamento criado/atualizado.</returns>
        /// <response code="200">Adiamento processado com sucesso.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        /// <response code="403">Utilizador não é administrador da equipa.</response>
        [HttpPut("PostponeMatch")]
        [ProducesResponseType(typeof(InfoPostPoneMatch), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PostponeMatch(Guid idTeam, [FromBody] PostPoneMatchDto dto)
        {
            await authorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);
            var matchPostPone = await matchController.PostPoneMatch(idTeam, dto);

            return Ok(matchPostPone);  
        }

        #endregion

        #region CancelMatch
        /// <summary>
        /// Cancela uma partida agendada.
        /// </summary>
        /// <remarks>
        /// Apenas administradores da equipa podem cancelar jogos. Requer uma justificação.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa que está a cancelar o jogo.</param>
        /// <param name="idMatch">O ID da partida a cancelar.</param>
        /// <param name="description">Motivo do cancelamento (enviado no corpo do pedido como string).</param>
        /// <response code="200">Partida cancelada com sucesso.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        /// <response code="403">Utilizador não é administrador da equipa.</response>
        [HttpDelete("CancelMatch/{idMatch}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CancelMatch(Guid idTeam, Guid idMatch, [FromBody] string description)
        {
            await authorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);
            await matchController.CancelMatch(idTeam, idMatch, description);

            return Ok();
        }

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