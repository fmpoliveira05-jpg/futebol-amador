using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Services;
using Application.DTOs.MatchInvites;
using Application.DTOs.Filters;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]/{idTeam:guid}")]
    [ApiController]
    public class MatchInviteController : ControllerBase
    {
        private readonly IMatchInviteService matchInviteService;

        public MatchInviteController(IMatchInviteService matchInviteService)
        {
            this.matchInviteService = matchInviteService;
        }

        [HttpPost("match-invites")]
        public async Task<IActionResult> SendMatchInvite(Guid idTeam, [FromBody] SendMatchInviteDto dto)
        {
            try
            {
                var sendInvite = await matchInviteService.SendMatchInvite(idTeam, dto);

                return Ok(sendInvite);
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

        [HttpPost("AcceptMatchInvite")]
        public async Task<IActionResult> AcceptMatchInvite(Guid idTeam, [FromBody] Guid idMatchInvite)
        {
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

        [HttpDelete("RefuseMatchInvite")]
        public async Task<IActionResult> RefuseMatchInvite(Guid idTeam, [FromBody] Guid idMatchInvite)
        {
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
            catch (NullReferenceException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado no servidor.", details = ex.Message });
            }

            return Ok();
        }

        [HttpPut("Negociate")]
        public async Task<IActionResult> NegociateMatchInvite(Guid idTeam, [FromBody] SendMatchInviteDto dto)
        {
            try
            {
                var matchInvite = await matchInviteService.NegociateMatchInvite(idTeam, dto);

                return Ok(matchInvite);
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado no servidor.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMatchInvitesTeam(Guid idTeam, [FromQuery] FilterMatchInvitesDto filter)
        {
            try
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
            catch (NullReferenceException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
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