using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub.ClienteService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]/{idTeam:guid}")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        #region Inicializer
        private readonly IMatchService matchController;
        private readonly IStartMatchHubClientService startMatchHubClientService;
        private readonly IFinishMatchHubClientService finishMatchHubClientService;
        
        public CalendarController(IMatchService matchController, IStartMatchHubClientService startMatchHubClientService,
            IFinishMatchHubClientService finishMatchHubClientService)
        {
            this.matchController = matchController;
            this.startMatchHubClientService = startMatchHubClientService;
            this.finishMatchHubClientService = finishMatchHubClientService;
        }
        #endregion

        #region Calendar
        [HttpGet]
        public async Task<IActionResult> CalendarTeam(Guid idTeam, [FromQuery] FilterCalendarDto filters)
        {
            IEnumerable<InfoMatchCalendar> matches;
            var hasFilter = filters.IsRealized != null ||
                                filters.IsRanqued != null ||
                                filters.IsHome != null ||
                                filters.MinDate.HasValue ||
                                filters.MaxDate.HasValue ||
                                !string.IsNullOrEmpty(filters.NameOpponent);

            var userId = GetCurrentUserId();
            if (hasFilter)
            {
                matches = await matchController.GetCalendarWithFilters(userId, idTeam, filters);
            }
            else 
            {
                matches = await matchController.GetCalendar(userId, idTeam);
            }
                    
            return Ok(matches);  
        }

        #endregion

        #region PostPoneMatch
        [HttpPut("PostponeMatch")]
        public async Task<IActionResult> PostponeMatch(Guid idTeam, [FromBody] PostPoneMatchDto dto)
        {
            var matchPostPone = await matchController.PostPoneMatch(GetCurrentUserId(), idTeam, dto);

            return Ok(matchPostPone);  
        }

        #endregion

        #region CancelMatch
        [HttpDelete("CancelMatch/{idMatch}")]
        public async Task<IActionResult> CancelMatch(Guid idTeam, Guid idMatch, [FromBody] string description)
        {
            await matchController.CancelMatch(GetCurrentUserId(), idTeam, idMatch, description);

            return Ok();
        }

        #endregion

        #region StartMatch

        [HttpPost("StartMatch")]
        public async Task<IActionResult> StartMatch(Guid idTeam, [FromBody] Guid idMatch)
        {
            if (idMatch == Guid.Empty)
            {
                return BadRequest("O id de admin não pode estar vazio");
            }

            await startMatchHubClientService.InitializeAsync();
            await startMatchHubClientService.JoinStartMatchAsync(idMatch, idTeam);
            return Ok("Conseguiu entrar no hub!");
        }

        [HttpPost("LeaveStartMatch")]
        public async Task<IActionResult> LeaveStartMatch(Guid idTeam)
        {
            await startMatchHubClientService.InitializeAsync();
            await startMatchHubClientService.LeaveStartMatchAsync();
            return Ok("Saiu do Hub com sucesso!");
        }

        #endregion

        #region FinishMatch
        [HttpPost("FinishMatch")]
        public async Task<IActionResult> FinishMatch(Guid idTeam, [FromBody] ResultMatchDto result)
        {
            if (idTeam == Guid.Empty)
            {
                return BadRequest("O id da equipa está vazio");
            }

            if (result == null)
            {
                return BadRequest("Não foi mandado o resultado da equipa");
            }

            if (idTeam != result.IdTeam)
            {
                return BadRequest("A equipa que submetu o formulário de fim de jogo não é a mesma do url");
            }

            await finishMatchHubClientService.InitializeAsync();
            await finishMatchHubClientService.JoinFinishMatchAsync(result);

            return Ok("Resultado submetido!");
        }

        [HttpPut("UpdateFinishMatch")]
        public async Task<IActionResult> UpdateFinishMatch(Guid idTeam, [FromBody] ResultMatchDto result)
        {
            if (idTeam == Guid.Empty)
            {
                return BadRequest("O id da equipa está vazio");
            }

            if (result == null)
            {
                return BadRequest("Não foi mandado o resultado da equipa");
            }

            if(idTeam != result.IdTeam)
            {
                return BadRequest("A equipa que submetu o formulário de fim de jogo não é a mesma do url");
            }

            await finishMatchHubClientService.InitializeAsync();
            await finishMatchHubClientService.EditResultMatchAsync(result);
  
            return Ok("Resultado alterado com sucesso");
        }

        [HttpPost("LeaveFinishMatch")]
        public async Task<IActionResult> LeaveFinishMatch(Guid idTeam)
        {
            await finishMatchHubClientService.InitializeAsync();
            await finishMatchHubClientService.LeaveFinishMatchAsync();

            return Ok("Saiu do Hub com sucesso!");
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