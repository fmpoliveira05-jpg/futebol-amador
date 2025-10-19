using Application.DTOs;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Services;

namespace Api.Controllers
{
    [Route("api/{idTeam:guid}/[controller]")]
    [ApiController]
    public class MatchInviteController : ControllerBase
    {
        private readonly IMatchInviteService matchInviteService;

        public MatchInviteController(IMatchInviteService matchInviteService)
        {
            this.matchInviteService = matchInviteService;
        }

        /***
         *  Vai faltar AUTs
         */

        [HttpPost("/match-invites")]
        public async Task<IActionResult> SendMatchInvite(Guid idTeam, [FromBody] SendMatchInviteDTO dto)
        {
            if (idTeam == Guid.Empty) {
                return BadRequest("O id da equipa está vazio");
            }

            if (dto.IdSender == Guid.Empty)
            {
                return BadRequest("O id do emissor do convite não pode ser nulo");
            }

            if (dto.IdSender != idTeam)
            {
                return BadRequest("O id da equipa do DTO não bate com a do url");
            }

            if (dto.IdSender == dto.IdReceiver)
            {
                throw new BusinessRuleException("O recetor do convite deve ser diferente do emissor!");
            }

            try
            {
                await matchInviteService.SendMatchInvite(dto);
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado no servidor.", details = ex.Message });
            }

            return Ok(dto);
        }

        private List<string> ValidateMatchInviteIds(Guid idTeam, Guid idMatchInvite)
        {
            var error = new List<string>();
            if (idTeam == Guid.Empty)
            {
                error.Add("O id da equipa não pode estar vazio");
            }

            if (idMatchInvite == Guid.Empty)
            {
                error.Add("O id da partida não pode estar vazio");
            }

            return error;
        }

        [HttpPost("/AcceptMatchInvite")]
        public async Task<IActionResult> AcceptMatchInvite(Guid idTeam, [FromBody] Guid idMatchInvite)
        {
            List<string> validator = ValidateMatchInviteIds(idTeam, idMatchInvite);
            if (validator.Count() > 0)
            {
                return BadRequest(validator);
            }

            //Chamar service
            try
            {
                var match = await matchInviteService.AcceptMatchInvite(idTeam, idMatchInvite);

                return Ok(match);
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado no servidor.", details = ex.Message });
            }
        }

        //DELETE
        // api/.../RefuseMatchInvite/id_invite
        [HttpDelete("/RefuseMatchInvite")]
        public async Task<IActionResult> RefuseMatchInvite(Guid idTeam, [FromBody] Guid idMatchInvite) {
            List<string> validator = ValidateMatchInviteIds(idTeam, idMatchInvite);
            if (validator.Count() > 0)
            {
                return BadRequest(validator);
            }

            try
            {
                await matchInviteService.RefuseMatchInvites(idTeam, idMatchInvite);
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado no servidor.", details = ex.Message });
            }

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> NegociateMatchInvite(Guid idTeam, [FromBody] SendMatchInviteDTO dto)
        {
            if (idTeam == Guid.Empty)
            {
                return BadRequest("O id da equipa não pode ser nulo");
            }

            if (dto.IdSender == Guid.Empty)
            {
                return BadRequest("O id de quem enviou o convite não pode ser nulo");
            }

            if (dto.IdReceiver == Guid.Empty)
            {
                return BadRequest("O id do recetor do convite não pode ser nulo!");
            }

            if (dto.namePitch == null || dto.namePitch == "")
            {
                return BadRequest("O id do campo não pode ser nulo");
            }

            if(idTeam != dto.IdSender)
            {
                return BadRequest("O id da equipa não bate com o id da equipa que mandou o convite");
            }

            try
            {
                var matchInvite = await matchInviteService.NegociateMatchInvite(dto);
            
                return Ok(matchInvite);
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
    }
}
