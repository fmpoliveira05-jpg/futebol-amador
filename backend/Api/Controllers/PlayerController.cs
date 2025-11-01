using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Application.DTOs.Team;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService playerService;

        public PlayerController(IPlayerService playerService)
        {
            this.playerService = playerService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerDTO playerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newPlayerId = await playerService.CreatePlayerAsync(playerDto);

            return CreatedAtAction(
                    nameof(GetPlayer),
                    new { playerId = newPlayerId },
                    playerDto);
        }

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

        [HttpDelete("{playerId:guid}")]
        public async Task<IActionResult> DeletePlayer(Guid playerId) 
        {
            await playerService.DeletePlayerAsync(playerId);

            return NoContent();
        }

        [HttpGet("{playerId:guid}")]
        public async Task<IActionResult> GetPlayer(Guid playerId)
        {
            var playerDetails = await playerService.GetPlayerByIdAsync(playerId);

            return Ok(playerDetails);
        }

        [HttpPut("{playerId:guid}")]
        public async Task<IActionResult> UpdateUser(Guid playerId, [FromBody] UpdatePlayerDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await playerService.UpdatePlayerAsync(playerId, dto);

            return Ok("Player information updated succesfully.");
        }

        [HttpPut("{playerId:guid}/leave-team")]
        public async Task<IActionResult> LeaveTeam(Guid playerId)
        {
            string teamName = await playerService.LeaveTeam(playerId);

            return Ok("Player succesfully left the team" + teamName + ".");
        }

        [HttpGet("{playerId:guid}/membership-requests")]
        public async Task<IActionResult> GetMembershipRequests(Guid playerId, [FromQuery] FilterMembershipRequestsPlayer filters)
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

        [HttpPost("{playerId:guid}/membership-requests/{requestId:guid}/accept")]
        public async Task<IActionResult> AcceptMembershipRequest(Guid playerId, Guid requestId)
        {
            await playerService.AcceptMembershipRequestAsync(playerId, requestId);
            return Ok("Pedido de adesão de equipa aceite pelo jogador.");
        }

        [HttpPost("{playerId:guid}/membership-requests/{requestId:guid}/reject")]
        public async Task<IActionResult> RejectMembershipRequest(Guid playerId, Guid requestId)
        {
            await playerService.RejectMembershipRequestAsync(playerId, requestId);
            return Ok("Pedido de adesão de equipa rejeitado pelo jogador.");
        }

        [HttpPost("{playerId:guid}/membership-requests/send/{teamId:guid}")]
        public async Task<IActionResult> SendMembershipRequest(Guid playerId, Guid teamId)
        {
            try
            {
                await playerService.SendMembershipRequestAsync(playerId, teamId);
                return Ok("Pedido de adesão enviado com sucesso.");
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
    }
}
