using Application.DTOs.Competition;
using Application.DTOs.Match;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>Relatório do jogo e registo do resultado pela web (a app usa o hub /FinishMatch).</summary>
    [ApiController]
    [Route("api/matches")]
    [Produces("application/json")]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchDetailsService details;
        private readonly IManagerFinishMatchService finish;

        public MatchesController(IMatchDetailsService details, IManagerFinishMatchService finish)
        {
            this.details = details;
            this.finish = finish;
        }

        /// <summary>Resultado, onzes, marcadores, cartões, substituições e faltas das duas equipas.</summary>
        [HttpGet("{matchId:guid}/report")]
        [ProducesResponseType(typeof(MatchReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Report(Guid matchId) => Ok(await details.GetReportAsync(matchId));

        /// <summary>
        /// Regista o resultado e os eventos da equipa do administrador. Segue as regras do hub: o jogo só
        /// termina quando os resultados das duas equipas coincidem; submeter outra vez corrige o anterior.
        /// </summary>
        [Authorize]
        [HttpPost("{matchId:guid}/result")]
        [ProducesResponseType(typeof(FinishMatchStateDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> SubmitResult(Guid matchId, [FromBody] ResultMatchDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? throw new UnauthorizedAccessException("O pedido não identifica o utilizador.");
            dto.IdMatch = matchId;
            var connection = $"rest:{userId}";

            // Se a equipa já tinha submetido, é uma correção.
            var state = finish.HasSubmitted(matchId, dto.IdTeam)
                ? await finish.UpdateResult(matchId, dto, userId, connection)
                : await finish.JoinHubAsync(matchId, dto, userId, connection);

            var finished = state.IsCoincides == true;
            return Ok(new FinishMatchStateDto
            {
                Submitted = true,
                MatchFinished = finished,
                ResultsCoincide = state.IsCoincides,
                Message = finished
                    ? "Resultado confirmado pelas duas equipas: o jogo terminou."
                    : state.IsCoincides == false
                        ? "O resultado não coincide com o do adversário. Confirma com a outra equipa e corrige."
                        : "Resultado registado. Falta o resultado da outra equipa.",
            });
        }
    }
}
