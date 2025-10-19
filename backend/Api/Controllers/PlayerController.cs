using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly PlayerService playerService;

        public PlayerController(PlayerService playerService)
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

            return CreatedAtAction(nameof(GetPlayer), new { playerId = newPlayerId });
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
