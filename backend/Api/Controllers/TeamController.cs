namespace Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs;
[ApiController]
[Route("api/[controller]")]
public class TeamController : ControllerBase
{
    private readonly TeamService _equipaService;

    public TeamController(TeamService equipaService)
    {
        _equipaService = equipaService;
    }

    [HttpPost]
    public IActionResult CriarEquipa(CreateTeamDto dto)
    {
        var result = "test";
//        var result = _equipaService.CriarEquipa(dto);
        return Ok(result);
    }
}
