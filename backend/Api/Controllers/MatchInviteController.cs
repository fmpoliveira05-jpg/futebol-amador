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
        private readonly IPlayerAuthorizationService AuthorizationService;

        public MatchInviteController(IMatchInviteService matchInviteService, IPlayerAuthorizationService authorizationService)
        {
            this.matchInviteService = matchInviteService;
            this.AuthorizationService = authorizationService;
        }

        #endregion

        #region EndPoints

        #region MatchInvites
        [HttpPost("match-invites")]
        public async Task<IActionResult> SendMatchInvite(Guid idTeam, [FromBody] SendMatchInviteDto dto)
        {
            await AuthorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);

            var sendInvite = await matchInviteService.SendMatchInvite(idTeam, dto);

            return Ok(sendInvite);
        }

        [HttpPost("AcceptMatchInvite")]
        public async Task<IActionResult> AcceptMatchInvite(Guid idTeam, [FromBody] Guid idMatchInvite)
        {
            await AuthorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);

            var match = await matchInviteService.AcceptMatchInvite(idTeam, idMatchInvite);
            
            return Ok(match);
        }

        [HttpDelete("RefuseMatchInvite")]
        public async Task<IActionResult> RefuseMatchInvite(Guid idTeam, [FromBody] Guid idMatchInvite)
        {
            await AuthorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);
            await matchInviteService.RefuseMatchInvites(idTeam, idMatchInvite);
            return Ok();
        }

        [HttpPut("Negociate")]
        public async Task<IActionResult> NegociateMatchInvite(Guid idTeam, [FromBody] SendMatchInviteDto dto)
        {
            await AuthorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);
            var matchInvite = await matchInviteService.NegociateMatchInvite(idTeam, dto);

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