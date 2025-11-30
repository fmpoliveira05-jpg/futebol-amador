using Application.DTOs.Team;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador responsável pela consulta de classificações e rankings (Leaderboard).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService leaderboardService;

        /// <summary>
        /// Construtor do LeaderboardController.
        /// </summary>
        /// <param name="leaderboardService">Serviço responsável pela lógica de obtenção e ordenação das classificações das equipas.</param>
        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            this.leaderboardService = leaderboardService;
        }

        /// <summary>
        /// Obtém a tabela de classificação global das equipas.
        /// </summary>
        /// <remarks>
        /// Retorna o <b>Top 100</b> das equipas com maior pontuação na plataforma.
        /// A lista vem ordenada pela posição (do 1º ao 100º).
        /// Este endpoint é público e não requer autenticação.
        /// </remarks>
        /// <returns>Uma lista de objetos <see cref="TeamLeaderboardDto"/> contendo a posição, nome, pontos e rank da equipa.</returns>
        /// <response code="200">Retorna a lista de classificação com sucesso.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<TeamLeaderboardDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLeaderboard()
        {
            var leaderboard = await leaderboardService.GetLeaderboardAsync();
            return Ok(leaderboard);
        }
    }
}