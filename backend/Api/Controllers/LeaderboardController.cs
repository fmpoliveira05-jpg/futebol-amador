using Microsoft.AspNetCore.OutputCaching;
using Api.Operacao;
using Application.DTOs.Competition;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Classificação de uma liga (por omissão, a do escalão mais alto, na época atual).
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeagueService leagueService;

        public LeaderboardController(ILeagueService leagueService)
        {
            this.leagueService = leagueService;
        }

        /// <summary>Classificação com PD, V, E, D, GM, GS, DG, P e a forma dos últimos 5 jogos.</summary>
        /// <param name="leagueId">Liga (opcional).</param>
        /// <response code="200">Classificação.</response>
        /// <response code="204">Ainda não há ligas.</response>
        [OutputCache(PolicyName = CachePublica.Politica)]
        [HttpGet]
        [ProducesResponseType(typeof(StandingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetLeaderboard([FromQuery] Guid? leagueId)
        {
            var standings = await leagueService.GetStandingsAsync(leagueId, null);
            return standings == null ? NoContent() : Ok(standings);
        }
    }
}
