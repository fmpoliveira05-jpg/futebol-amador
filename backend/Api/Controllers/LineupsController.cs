using Microsoft.AspNetCore.OutputCaching;
using Api.Operacao;
using Application.DTOs.Competition;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>Onze inicial e suplentes de cada equipa num jogo.</summary>
    [ApiController]
    [Route("api/lineups")]
    [Produces("application/json")]
    public class LineupsController : ControllerBase
    {
        private readonly IMatchDetailsService details;

        public LineupsController(IMatchDetailsService details)
        {
            this.details = details;
        }

        private string? UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        /// <summary>Táticas disponíveis, com as coordenadas de cada posição no campo.</summary>
        [OutputCache(PolicyName = CachePublica.Politica)]
        [HttpGet("formations")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<FormationDto>), StatusCodes.Status200OK)]
        public IActionResult Formations() => Ok(details.GetFormations());

        /// <summary>Onze de uma equipa (o do adversário só depois do prazo).</summary>
        [HttpGet("{matchId:guid}/{teamId:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LineupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get(Guid matchId, Guid teamId) => Ok(await details.GetLineupAsync(UserId, matchId, teamId));

        /// <summary>Define ou altera o onze (administrador da equipa, até 2 horas antes do jogo).</summary>
        [Authorize]
        [HttpPut("{matchId:guid}/{teamId:guid}")]
        [ProducesResponseType(typeof(LineupDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Save(Guid matchId, Guid teamId, [FromBody] SaveLineupDto dto) =>
            Ok(await details.SaveLineupAsync(UserId, matchId, teamId, dto));
    }
}
