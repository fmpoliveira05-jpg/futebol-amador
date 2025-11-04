using Application.DTOs.Filters;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/Team/{idTeam:guid}/[controller]")]
    [ApiController]
    public class PostPoneMatchController: ControllerBase
    {
        #region Initialization
        private readonly IMatchService matchController;

        public PostPoneMatchController(IMatchService matchController)
        {
            this.matchController = matchController;
        }
        #endregion

        #region PostPoneMatch
        [HttpGet]
        public async Task<IActionResult> GetListPostPoneMatchTeam(Guid idTeam, [FromQuery] FilterPostPoneMatchDto filter)
        {
            try
            {
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
                    listPostPone = await matchController.GetListPostPoneMatchTeamWithFilters(userId, idTeam, filter);
                }
                else
                {
                    listPostPone = await matchController.GetListPostPoneMatchTeam(userId, idTeam);
                }

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

        [HttpPost("AcceptPostponeMatch")]
        public async Task<IActionResult> AcceptPostponeMatch(Guid idTeam, [FromBody] AcceptRefusePostPoneDto dto)
        {
            try
            {
                var match = await matchController.AcceptPostPoneMatch(GetCurrentUserId(), idTeam, dto);

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

        [HttpDelete("RejectPostponeMatch")]
        public async Task<IActionResult> RejectPostponeMatch(Guid idTeam, [FromBody] AcceptRefusePostPoneDto dto)
        {
            try
            {
                await matchController.RejectPostPoneMatch(GetCurrentUserId(), idTeam, dto);

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
