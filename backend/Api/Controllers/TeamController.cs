namespace Api.Controllers;
using Application.DTOs.Team;

using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.Interfaces.Services;

[ApiController]
[Route("api/[controller]")]
public class TeamController : ControllerBase
{
    private readonly ITeamService TeamService;

    public TeamController(ITeamService teamService)
    {
        TeamService = teamService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeamById(Guid id)
    {
        var creatorIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //if (string.IsNullOrEmpty(creatorIdString) || !Guid.TryParse(creatorIdString, out Guid creatorUserId))
        //{
          //  return Unauthorized("Token de utilizador inválido ou em falta.");
        //}
        var team = await TeamService.GetTeamByIdAsync(id);
        return Ok(team);
    }

    [HttpPost]
    public async Task<IActionResult> CriarEquipa([FromBody] CreateTeamDto teamDto)
    {
        var creatorIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //if (string.IsNullOrEmpty(creatorIdString) || !Guid.TryParse(creatorIdString, out Guid creatorUserId))
        //{
            //return Unauthorized("Token de utilizador inválido ou em falta.");
        //}
        //TODO: Adicionar verificação se o player já tem equipa
        var newTeamId = await TeamService.CreateTeamAsync(teamDto);

        return CreatedAtAction(nameof(GetTeamById), new { id = newTeamId }, new { id = newTeamId });
    }
}
