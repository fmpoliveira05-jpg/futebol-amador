using Application.DTOs.Membership;
using Application.Interfaces.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/{idPlayer:guid}")]
    [ApiController]
    public class MembershipRequestsController : ControllerBase
    {
        private readonly IMembershipRequestService membershipRequestService;

        public MembershipRequestsController(IMembershipRequestService membershipRequestService)
        {
            this.membershipRequestService = membershipRequestService;
        }

        // GET: api/{idPlayer}/MembershipRequests/received-from-teams
        [HttpGet("received-from-teams")]
        public async Task<IActionResult> GetRequestsReceivedFromTeams(Guid idPlayer)
        {
            if (idPlayer == Guid.Empty)
                return BadRequest("O id do jogador não pode ser vazio");

            try
            {
                var results = await membershipRequestService.GetRequestsReceivedByPlayerFromTeams(idPlayer);
                return Ok(results);
            }
            catch (ArgumentNullException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado no servidor.", details = ex.Message });
            }
        }

        // GET: api/{idPlayer}/MembershipRequests/sent
        [HttpGet("sent")]
        public async Task<IActionResult> GetRequestsSentByPlayer(Guid idPlayer)
        {
            if (idPlayer == Guid.Empty)
                return BadRequest("O id do jogador não pode ser vazio");

            try
            {
                var results = await membershipRequestService.GetRequestsSentByPlayer(idPlayer);
                return Ok(results);
            }
            catch (ArgumentNullException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado no servidor.", details = ex.Message });
            }
        }

        // POST: api/{idPlayer}/MembershipRequests
        [HttpPost]
        public async Task<IActionResult> SendRequest(Guid idPlayer, [FromBody] SendMembershipRequestDTO dto)
        {
            if (idPlayer == Guid.Empty)
                return BadRequest("O id do jogador não pode ser vazio");

            if (dto == null)
                return BadRequest("DTO inválido");

            if (dto.IdPlayer == Guid.Empty)
                return BadRequest("O id do jogador no DTO não pode ser nulo");

            if (dto.IdTeam == Guid.Empty)
                return BadRequest("O id da equipa no DTO não pode ser nulo");

            if (dto.IdPlayer != idPlayer)
                return BadRequest("O id do jogador no DTO não bate com o id do url");

            try
            {
                await membershipRequestService.SendMembershipRequest(dto);
                return Ok(dto);
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

        // POST: api/{idTeam}/MembershipRequests/accept
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptRequest(Guid idTeam, [FromBody] Guid idMembershipRequest)
        {
            if (idTeam == Guid.Empty)
                return BadRequest("O id da equipa não pode ser vazio");

            if (idMembershipRequest == Guid.Empty)
                return BadRequest("O id do pedido não pode ser vazio");

            try
            {
                var player = await membershipRequestService.AcceptMembershipRequest(idTeam, idMembershipRequest);
                return Ok(player);
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

        // DELETE: api/{idTeam}/MembershipRequests/refuse
        [HttpDelete("MembershipRequests/refuse")]
        public async Task<IActionResult> RefuseRequest(Guid idTeam, [FromBody] Guid idMembershipRequest)
        {
            if (idTeam == Guid.Empty)
                return BadRequest("O id da equipa não pode ser vazio");

            if (idMembershipRequest == Guid.Empty)
                return BadRequest("O id do pedido não pode ser vazio");

            try
            {
                await membershipRequestService.RefuseMembershipRequest(idTeam, idMembershipRequest);
                return Ok();
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
    }
}