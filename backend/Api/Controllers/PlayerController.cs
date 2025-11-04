using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Services;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService playerService;

        public PlayerController(IPlayerService playerService)
        {
            this.playerService = playerService;
        }

        [HttpPost]
        [Route("create-profile")]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerDTO playerDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var newPlayerId = await playerService.CreatePlayerAsync(userId,email,playerDto);
            /*
            return CreatedAtAction(
                    nameof(GetPlayer),
                    new { playerId = newPlayerId },
                    playerDto);
            */
            return CreatedAtAction(
            "",
            null,
            null);
            /* Demora um bocado
            return Ok(playerDto);
            */
            //Cria o player rapido
            //  return NoContent();
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
        public async Task<IActionResult> UpdateUser([FromBody] UpdatePlayerDTO dto)
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

        [HttpPut("{playerId:required}/leave-team")]
        public async Task<IActionResult> LeaveTeam(string playerId)
        {
            string teamName = await playerService.LeaveTeam(playerId);

            return Ok("Player succesfully left the team" + teamName + ".");
        }
    }
}
