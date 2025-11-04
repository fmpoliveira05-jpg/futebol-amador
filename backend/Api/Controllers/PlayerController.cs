using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Domain.Exceptions;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlayerController : ControllerBase
    {
        #region Inicializar
        private readonly IPlayerService playerService;

        public PlayerController(IPlayerService playerService)
        {
            this.playerService = playerService;
        }
        #endregion

        #region CRUD Player
        [HttpPost]
        [Route("create-profile")]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerDto playerDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var newPlayerId = await playerService.CreatePlayerAsync(userId,email,playerDto);

            return CreatedAtAction(
                    nameof(GetPlayer),
                    new { playerId = newPlayerId },
                    playerDto
                    );
        }

        [HttpDelete("{playerId:required}")]
        public async Task<IActionResult> DeletePlayer(string playerId) 
        {
            await playerService.DeletePlayerAsync(playerId);

            return NoContent();
        }

        [HttpGet("{playerId:required}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlayer(string playerId)
        {
            var playerDetails = await playerService.GetPlayerByIdAsync(playerId);

            return Ok(playerDetails);
        }

        [HttpGet()]
        [Route("get-my-profile")]
        public async Task<IActionResult> GetFullProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var playerDetails = await playerService.GetPlayerByIdAsync(userId);

            return Ok(playerDetails);
        }

        [HttpPut("{playerId:required}")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdatePlayerDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await playerService.UpdatePlayerAsync(userId, dto);

            return Ok("Player information updated succesfully.");
        }

        #endregion

        #region MemberShipRequest

        [HttpGet("listTeamsToMemberShipRequest")]
        public async Task<IActionResult> ListTeams([FromQuery] FilterListTeamDto filter)
        {
            var isFilter = !string.IsNullOrEmpty(filter.NameTeam) ||
                           !string.IsNullOrEmpty(filter.NameRank) ||
                           !string.IsNullOrEmpty(filter.City) ||
                           filter.MinNumberPoints.HasValue ||
                           filter.MaxNumberPoints.HasValue ||
                           filter.MinAge.HasValue ||
                           filter.MaxAge.HasValue ||
                           filter.MinNumberPlayers.HasValue ||
                           filter.MaxNumberPlayers.HasValue;

            IEnumerable<InfoTeamsDto> list;
            if (isFilter)
            {
                list = await playerService.GetTeamListWithFilters(filter);
            }
            else
            {
                list = await playerService.GetListTeams();
            }

            return Ok(list);
        }

        /*
        [HttpGet("{playerId:guid}/membership-requests")]
        public async Task<IActionResult> GetMembershipRequests(string playerId, [FromQuery] FilterMembershipRequestsPlayer filters)
        {
            try
            {
                var hasFilter = filters.MinDate.HasValue ||
                                filters.MaxDate.HasValue ||
                                !string.IsNullOrEmpty(filters.SenderName);

                IEnumerable<MemberShipRequestDto> requests;
                if (hasFilter)
                {
                    requests = await playerService.GetMembershipRequestsAsyncWithFilters(playerId, filters);
                }
                else
                {
                    requests = await playerService.GetMembershipRequestsAsync(playerId);
                }

                return Ok(requests);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro inesperado no servidor.", details = ex.Message });
            }
        }

        [HttpPost("{playerId:guid}/membership-requests/accept")]
        public async Task<IActionResult> AcceptMembershipRequest(string playerId, [FromBody] Guid requestId)
        {
            var dto = await playerService.AcceptMembershipRequestAsync(playerId, requestId);
            return Ok(dto);
        }

        [HttpDelete("{playerId:guid}/membership-requests/reject/{requestId:guid}")]
        public async Task<IActionResult> RejectMembershipRequest(string playerId, Guid requestId)
        {
            var dto = await playerService.RejectMembershipRequestAsync(playerId, requestId);
            return Ok(dto);
        }

        [HttpPost("{playerId:guid}/membership-requests/send")]
        public async Task<IActionResult> SendMembershipRequest(string playerId, [FromBody] Guid teamId)
        {
            try
            {
                var dto = await playerService.SendMembershipRequestAsync(playerId, teamId);
                return Ok(dto);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro inesperado no servidor.", details = ex.Message });
            }
        }
        */

        #endregion

        #region Teams Operations

        [HttpPut("{playerId:required}/leave-team")]
        public async Task<IActionResult> LeaveTeam(string playerId)
        {
            string teamName = await playerService.LeaveTeam(playerId);

            return Ok("Player succesfully left the team" + teamName + ".");
        }
        #endregion
    }
}
