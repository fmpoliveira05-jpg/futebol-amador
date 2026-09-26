using Application.DTOs.Competition;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Ligas e épocas: consulta pública da classificação e do calendário; gestão pelo super administrador;
    /// inscrição de equipas pelos seus administradores.
    /// </summary>
    [ApiController]
    [Route("api/leagues")]
    [Produces("application/json")]
    public class LeaguesController : ControllerBase
    {
        private readonly ILeagueService leagues;

        public LeaguesController(ILeagueService leagues)
        {
            this.leagues = leagues;
        }

        private string? UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        /// <summary>Todas as ligas, do escalão mais alto para o mais baixo, com a época atual.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<LeagueDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLeagues() => Ok(await leagues.GetLeaguesAsync());

        /// <summary>Classificação de uma liga (época atual, ou a indicada).</summary>
        [HttpGet("{leagueId:guid}/standings")]
        [ProducesResponseType(typeof(StandingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStandings(Guid leagueId, [FromQuery] Guid? seasonId) =>
            Ok(await leagues.GetStandingsAsync(leagueId, seasonId));

        /// <summary>Calendário da época, por jornadas.</summary>
        [HttpGet("seasons/{seasonId:guid}/fixtures")]
        [ProducesResponseType(typeof(List<FixtureRoundDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFixtures(Guid seasonId) => Ok(await leagues.GetFixturesAsync(seasonId));

        /// <summary>Cria uma liga (super administrador).</summary>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(LeagueDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateLeague([FromBody] CreateLeagueDto dto)
        {
            var league = await leagues.CreateLeagueAsync(UserId, dto);
            return Created($"/api/leagues/{league.Id}/standings", league);
        }

        /// <summary>Abre as inscrições de uma nova época (super administrador).</summary>
        [Authorize]
        [HttpPost("{leagueId:guid}/seasons")]
        [ProducesResponseType(typeof(SeasonDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateSeason(Guid leagueId, [FromBody] CreateSeasonDto dto)
        {
            var season = await leagues.CreateSeasonAsync(UserId, leagueId, dto);
            return Created($"/api/leagues/seasons/{season.Id}/fixtures", season);
        }

        /// <summary>Sorteia o calendário e começa a época (super administrador).</summary>
        [Authorize]
        [HttpPost("seasons/{seasonId:guid}/start")]
        [ProducesResponseType(typeof(SeasonDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> StartSeason(Guid seasonId, [FromBody] StartSeasonDto? dto) =>
            Ok(await leagues.StartSeasonAsync(UserId, seasonId, dto ?? new StartSeasonDto()));

        /// <summary>Fecha a época: troféu, subidas e descidas (super administrador).</summary>
        [Authorize]
        [HttpPost("seasons/{seasonId:guid}/close")]
        [ProducesResponseType(typeof(SeasonDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CloseSeason(Guid seasonId) => Ok(await leagues.CloseSeasonAsync(UserId, seasonId));

        /// <summary>Inscreve a equipa nas inscrições abertas da liga (administrador da equipa).</summary>
        [Authorize]
        [HttpPost("{leagueId:guid}/register/{teamId:guid}")]
        [ProducesResponseType(typeof(SeasonDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Register(Guid leagueId, Guid teamId) =>
            Ok(await leagues.RegisterTeamAsync(UserId, leagueId, teamId));
    }
}
