using Application.DTOs.Filters;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/Team/{idTeam:guid}/[controller]")]
    [ApiController]
    public class PostPoneMatchController: ControllerBase
    {
        private readonly IMatchService matchController;

        public PostPoneMatchController(IMatchService matchController)
        {
            this.matchController = matchController;
        }

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

        [HttpDelete("RejectPostponeMatch")]
        public async Task<IActionResult> RejectPostponeMatch(Guid idTeam, [FromBody] AcceptRefusePostPoneDto dto)
        {
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

        #endregion
    }
}
