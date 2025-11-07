using Application.DTOs.Filters;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/Team/{idTeam:guid}/[controller]")]
    [ApiController]
    public class PostPoneMatchController : ControllerBase
    {
        #region Initialization
        private readonly IMatchService matchController;
        private readonly IPlayerAuthorizationService authorizationService;
       
        public PostPoneMatchController(IMatchService matchController, IPlayerAuthorizationService authorizationService)
        {
            this.matchController = matchController;
            this.authorizationService = authorizationService;
        }
        #endregion

        #region EndPoints
        
        #region PostPoneMatch
        [HttpGet]
        public async Task<IActionResult> GetListPostPoneMatchTeam(Guid idTeam, [FromQuery] FilterPostPoneMatchDto filter)
        {
            await authorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);
            IEnumerable<InfoPostPoneMatch> listPostPone;

            var isFilter = !string.IsNullOrEmpty(filter.NameOpponent) ||
                            filter.IsHome.HasValue ||
                            filter.MinDateGame.HasValue ||
                            filter.MaxDateGame.HasValue ||
                            filter.MinDatePostPoneGame.HasValue ||
                            filter.MaxDatePostPoneGame.HasValue;

            var userId = GetCurrentUserId();
            if (isFilter)
            {
                listPostPone = await matchController.GetListPostPoneMatchTeamWithFilters(idTeam, filter);
            }
            else
            {
                listPostPone = await matchController.GetListPostPoneMatchTeam(idTeam);
            }

            return Ok(listPostPone);
        }

        [HttpPost("AcceptPostponeMatch")]
        public async Task<IActionResult> AcceptPostponeMatch(Guid idTeam, [FromBody] AcceptRefusePostPoneDto dto)
        {
            await authorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);
            var match = await matchController.AcceptPostPoneMatch(idTeam, dto);

            return Ok(match);
        }

        [HttpDelete("RejectPostponeMatch")]
        public async Task<IActionResult> RejectPostponeMatch(Guid idTeam, [FromBody] AcceptRefusePostPoneDto dto)
        {
            await authorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), idTeam);
            await matchController.RejectPostPoneMatch(idTeam, dto);

            return Ok();
        }

        #endregion

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
