using Application.DTOs;
using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Hub;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    //[Authorize]
    [Route("api/{idTeam:guid}/[controller]")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        private readonly IMatchService matchController;
        private readonly IStartMatchHub hub;

        public MatchController(IMatchService matchController)
        {
            this.matchController = matchController;
        }

        [HttpGet]
        public async Task<IActionResult> CalendarTeam(Guid idTeam, [FromQuery] FilterCalendar filters)
        {
            if (idTeam == Guid.Empty)
            {
                return BadRequest("O id da equipa é obrigatorio");
            }

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

        [HttpPut("/PostponeMatch")]
        public async Task<IActionResult> PostponeMatch(Guid idTeam, [FromBody] PostponeMatchDTO dto)
        {
            var listErrors = validatePostPoneMatch(idTeam, dto);
            if (listErrors.Count > 0) { 
                return BadRequest(listErrors); 
            }

            try
            {
                var matchPostPone = await matchController.PostPoneMatch(dto);

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

        [HttpPost("/AcceptPostponeMatch")]
        public async Task<IActionResult> AcceptPostponeMatch(Guid idTeam, [FromBody] AcceptRefusePostPoneDTO dto)
        {
            var listErrors = validateAnswerPostPoneMatch(idTeam, dto);
            if (listErrors.Count > 0)
            {
                return BadRequest(listErrors);
            }

            if (dto.StatusPostPone != StatusPostPone.ACCEPT)
            {
                return BadRequest("Para poder aceitar um convite ele precisa de estar aceite");
            }

            try
            {
                var match = await matchController.AcceptPostPoneMatch(idTeam, dto);

                return Ok(match);
            }
            catch (MatchException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (NotFindException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado no servidor.", details = ex.Message });
            }
        }

        [HttpDelete("/RejectPostponeMatch")]
        public async Task<IActionResult> RejectPostponeMatch(Guid idTeam, [FromBody] AcceptRefusePostPoneDTO dto)
        {
            var listErrors = validateAnswerPostPoneMatch(idTeam, dto);
            if (listErrors.Count > 0)
            {
                return BadRequest(listErrors);
            }

            if (dto.StatusPostPone != StatusPostPone.REJECT)
            {
                return BadRequest("Para poder rejeitar um convite ele precisa de estar como rejeitado");
            }

            try
            {
                await matchController.RejectPostPoneMatch(idTeam, dto);

                return Ok();
            }
            catch (MatchException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (NotFindException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado no servidor.", details = ex.Message });
            }
        }

        [HttpGet("/PostPoneMatchs")]
        public async Task<IActionResult> GetListPostPoneMatchTeam(Guid idTeam)
        {
            if (idTeam == Guid.Empty)
            {
                return BadRequest("O id da equipa não pode estar vazio");
            } 

            try
            {
                var listPostPone = await matchController.GetListPostPoneMatchTeam(idTeam);

                return Ok(listPostPone);
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

        //Falta Testar
        [HttpDelete("/CancelMatch/{idMatch}")]
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

        //IHUbContext<Hub> para poder chamar o metodo do Hub
        //Vou ter de utilizar socket (SignalR) para o iniciar partida.
        //Posso utilizar um backgroundService para que caso a partida não seja aceite em 5 minutos o soocket é desligado
        [HttpPost("/StartMatch")]
        public async Task<IActionResult> StartMatch([FromBody] Guid idMatch)
        {
            if (idMatch == Guid.Empty)
            {
                return BadRequest("O id de admin não pode estar vazio");
            }

            hub.JoinStartMatch(idMatch);
            return Ok();
        }

        [HttpPost("/LeaveStartMatch")]
        public async Task<IActionResult> LeaveStartMatch([FromBody] Guid idMatch)
        {
            if (idMatch == Guid.Empty)
            {
                return BadRequest("O id de admin não pode estar vazio");
            }

            hub.LeaveStartMatch();
            return Ok();
        }

        //Falta apenas chamar hub
        [HttpPost("/FinishMatch")]
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

            await matchController.FinishMatch(idTeam, result);
            
            //Chamar Hub finishmatch e utilizar o join
            return Ok();
        }

        [HttpPut("/UpdateFinishMatch")]
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

            await matchController.FinishMatch(idTeam, result); //Chamo na mesma o Finish que tem as mesmas validações

            //Chamar Hub finishmatch e utilizar um metodo para atualizar se calhar
            return Ok();
        }

        [HttpPost("/LeaveFinishMatch")]
        public async Task<IActionResult> LeaveFinishMatch(Guid idTeam, [FromBody] Guid idMatch)
        {
            if (idTeam == Guid.Empty)
            {
                return BadRequest("O id da equipa está vazio");
            }

            if (idMatch == Guid.Empty)
            {
                return BadRequest("O id da equipa está vazio");
            }

            await matchController.LeaveFinishMatch(idTeam, idMatch);

            //Chamar Hub finishMatch e chamar o leave
            return Ok();
        }

        private List<string> validatePostPoneMatch(Guid idTeam, PostponeMatchDTO dto)
        {
            List<string> errors = new List<string>();
            if (idTeam == Guid.Empty)
            {
                errors.Add("O id da Team está vazio");
            }

            if (dto.IdMatch == Guid.Empty)
            {
                errors.Add("O id da match não pode estar vazio");
            }

            if (dto.IdTeam == Guid.Empty)
            {
                errors.Add("O id da equipa não pode estar vazio");
            }

            if (dto.IdTeam != idTeam)
            {
                errors.Add("O id da equipa que quer adiar é diferente da que está no url");
            }

            if (dto.IdOpponent == Guid.Empty)
            {
                errors.Add("O id do opponete não pode estar vazio");
            }

            return errors;
        }

        private List<string> validateAnswerPostPoneMatch(Guid idTeam, AcceptRefusePostPoneDTO dto)
        {
            List<string> errors = new List<string>();
            if (idTeam == Guid.Empty)
            {
                errors.Add("O id da Team está vazio");
            }

            if (dto.IdMatch == Guid.Empty)
            {
                errors.Add("O id da match não pode estar vazio");
            }

            if (dto.IdTeam == Guid.Empty)
            {
                errors.Add("O id da equipa não pode estar vazio");
            }

            if (dto.IdOpponent == Guid.Empty)
            {
                errors.Add("O id do opponete não pode estar vazio");
            }

            return errors;
        }
    }
}