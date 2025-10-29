<<<<<<< Updated upstream
﻿namespace Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs;
[ApiController]
[Route("api/[controller]")]
public class TeamController : ControllerBase
=======
﻿using Api.Controllers; // Assume que este é o teu namespace
using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.Team;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
>>>>>>> Stashed changes
{
    private readonly TeamService _equipaService;

    public TeamController(TeamService equipaService)
    {
        _equipaService = equipaService;
    }

<<<<<<< Updated upstream
    [HttpPost]
    public IActionResult CriarEquipa(CreateTeamDto dto)
    {
        var result = "test";
        //var result = _equipaService.CriarEquipa(dto);
        return Ok(result);
=======
        [HttpPost("{teamId:guid}/membership-requests/send/{playerId:guid}")]
        public async Task<IActionResult> SendMembershipRequest(Guid teamId, Guid playerId)
        {
            try
            {
                var adminId = GetCurrentUserId();

                await TeamService.SendMembershipRequestAsync(teamId, playerId, adminId);

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


>>>>>>> Stashed changes
    }
}
