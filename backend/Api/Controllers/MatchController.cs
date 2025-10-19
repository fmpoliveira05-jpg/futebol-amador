using Application.DTOs;
using Application.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/{idTeam:guid}/[controller]")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        private readonly MatchService matchController;

        public MatchController(MatchService matchController)
        {
            this.matchController = matchController;
        }

        private List<string> validatePostPoneMatch (Guid idTeam, PostponeMatchDTO dto)
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

        [HttpPut("/PostponeMatch")]
        public async Task<IActionResult> PostponeMatch(Guid idTeam, [FromBody] PostponeMatchDTO dto)
        {
            var listErrors = validatePostPoneMatch(idTeam, dto);
            if (listErrors.Count > 0) { 
                return BadRequest(listErrors); 
            }

            try
            {
                var match = await matchController.PostPoneMatch(dto);

                return Ok(match);
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

        [HttpPost("/AcceptPostponeMatch")]
        public async Task<IActionResult> AcceptPostponeMatch(Guid idTeam, [FromBody] AcceptRefusePostPoneDTO dto)
        {
            var listErrors = validateAnswerPostPoneMatch(idTeam, dto);
            if (listErrors.Count > 0)
            {
                return BadRequest(listErrors);
            }

            try
            {
                var match = await matchController.AcceptPostPoneMatch(dto);

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

        [HttpDelete("/AcceptPostponeMatch")]
        public async Task<IActionResult> RejectPostponeMatch(Guid idTeam, [FromBody] AcceptRefusePostPoneDTO dto)
        {
            var listErrors = validateAnswerPostPoneMatch(idTeam, dto);
            if (listErrors.Count > 0)
            {
                return BadRequest(listErrors);
            }

            try
            {
                var match = await matchController.RejectPostPoneMatch(dto);

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
    }
}
