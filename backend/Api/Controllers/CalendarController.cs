using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub.ClienteService;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

/*
 Criar urls para o FinishMatch
 */
namespace Api.Controllers
{
    //[Authorize]
    [Route("api/[controller]/{idTeam:guid}")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
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

        #region Calendar
        [HttpGet]
        public async Task<IActionResult> CalendarTeam(Guid idTeam, [FromQuery] FilterCalendarDto filters)
        {
            try
            {
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
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NullReferenceException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado no servidor.", details = ex.Message });
            }
        }

        #endregion

        #region PostPoneMatch
        [HttpPut("PostponeMatch")]
        public async Task<IActionResult> PostponeMatch(Guid idTeam, [FromBody] PostPoneMatchDto dto)
        {
            try
            {
                var matchPostPone = await matchController.PostPoneMatch(idTeam, dto);

                return Ok(matchPostPone);
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NullReferenceException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado no servidor.", details = ex.Message });
            }
        }

        #endregion

        #region CancelMatch
        [HttpDelete("CancelMatch/{idMatch}")]
        public async Task<IActionResult> CancelMatch(Guid idTeam, Guid idMatch, [FromBody] string description)
        {
            if (idTeam == Guid.Empty)
            {
                return BadRequest("O id da equipa não pode estar vazio");
            }

            if(idMatch == Guid.Empty)
            {
                return BadRequest("O id da partida está vazio");
            }

            if (description == "")
            {
                return BadRequest("O cancelamento precisa de uma descrição");
            }

            try
            {
                await matchController.CancelMatch(idTeam, idMatch, description);

                return Ok();
            }
            catch (EmptyCollectionException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado no servidor.", details = ex.Message });
            }
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
    }
}