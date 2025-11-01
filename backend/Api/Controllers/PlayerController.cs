using Application.DTOs.Filters;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

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
    }
}
