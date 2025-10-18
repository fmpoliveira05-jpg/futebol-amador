using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.PlayerDTOs;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly AmateurFootballContext context;

        public PlayerController(AmateurFootballContext context)
        {
            this.context = context;
        }

        [HttpPost("create")]
        public IActionResult CreatePlayer([FromBody] CreatePlayerDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var player = new Player
            {
                Name = dto.Name,
                DateOfBirth = dto.DateOfBirth,
                Address = dto.Address,
                Email = dto.Email,
                Password = dto.Password,
                Phone = dto.Phone,
                Position = dto.Position,
                Height = dto.Height,
                CreationDate = DateTime.Now
            };

            context.Player.Add(player);
            context.SaveChanges();

            return CreatedAtAction(nameof(GetPlayer), new { playerId = player.Id }, player);
        }

        [HttpDelete("{playerId:guid}")]
        public IActionResult DeletePlayer(Guid playerId) 
        {
            var player = context.Player.Find(playerId);
            
            if ( player == null)
            {
                return NotFound("Player not found");
            }

            context.Player.Remove(player);
            context.SaveChanges();

            return NoContent();
        }

        [HttpGet("{playerId:guid}")]
        public IActionResult GetPlayer(Guid playerId)
        {
            var player = context.Player.Find(playerId);

            if (player == null)
            {
                return NotFound("Player not found");
            }

            return Ok(player);
        }

        [HttpPut("{playerId:guid}")]
        public IActionResult UpdateUser(Guid playerId, [FromBody] UpdatePlayerDTO dto)
        {
            var player = context.Player.Find(playerId);

            if (player == null)
            {
                return NotFound("Player not found");
            }

            player.Name = dto.Name;
            player.DateOfBirth = dto.DateOfBirth;
            player.Email = dto.Email;
            player.Password = dto.Password;
            player.Phone = dto.Phone;
            player.Position = dto.Position;
            player.Height = dto.Height;

            context.SaveChanges();
            return Ok(player);
        }

        [HttpPut("{playerId:guid}/leave-team")]
        public IActionResult LeaveTeam(Guid playerId)
        {
            var player = context.Player.Find(playerId);

            if (player == null)
            {
                return NotFound("Player not found.");
            }

            string teamName = player.Team.Name;

            player.Team = null;
            player.idTeam = null;

            context.SaveChanges();

            return Ok("Player succesfully left the team" + teamName);
        }
    }
}
