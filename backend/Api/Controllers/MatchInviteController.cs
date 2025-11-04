using Application.DTOs.Filters;
using Application.DTOs.MatchInvites;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]/{idTeam:guid}")]
    [ApiController]
    public class MatchInviteController : ControllerBase
    {
        #region Initialization
        private readonly IMatchInviteService matchInviteService;

        public MatchInviteController(IMatchInviteService matchInviteService)
        {
            this.matchInviteService = matchInviteService;
        }

        #endregion

        #region EndPoints
        [HttpPost("match-invites")]
        public async Task<IActionResult> SendMatchInvite(Guid idTeam, [FromBody] SendMatchInviteDto dto)
        {
            var sendInvite = await matchInviteService.SendMatchInvite(GetCurrentUserId(), idTeam, dto);

            return Ok(sendInvite);
        }

        [HttpPost("AcceptMatchInvite")]
        public async Task<IActionResult> AcceptMatchInvite(Guid idTeam, [FromBody] Guid idMatchInvite)
        {
            var match = await matchInviteService.AcceptMatchInvite(GetCurrentUserId(), idTeam, idMatchInvite);
            
            return Ok(match);
        }

        [HttpDelete("RefuseMatchInvite")]
        public async Task<IActionResult> RefuseMatchInvite(Guid idTeam, [FromBody] Guid idMatchInvite)
        {

            await matchInviteService.RefuseMatchInvites(GetCurrentUserId(), idTeam, idMatchInvite);
            return Ok();
        }

        [HttpPut("Negociate")]
        public async Task<IActionResult> NegociateMatchInvite(Guid idTeam, [FromBody] SendMatchInviteDto dto)
        {
            var matchInvite = await matchInviteService.NegociateMatchInvite(GetCurrentUserId(), idTeam, dto);

            return Ok(matchInvite);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMatchInvitesTeam(Guid idTeam, [FromQuery] FilterMatchInvitesDto filter)
        {
            IEnumerable<InfoMatchInviteDto> matchesInvite;

            bool hasFilter = !string.IsNullOrEmpty(filter.SenderName) ||
                                filter.MinDate.HasValue ||
                                filter.MaxDate.HasValue;

            if (hasFilter)
            {
                matchesInvite = await matchInviteService.GetAllMatchInvitesTeamWithFilters(GetCurrentUserId(), idTeam, filter);
            }
            else
            {
                matchesInvite = await matchInviteService.GetAllMatchInvitesTeam(GetCurrentUserId(), idTeam);
            }

            return Ok(matchesInvite);
        }
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