using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Services.Hub.ClienteService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador responsável pelo sistema de Matchmaking Competitivo (Ranked).
    /// Gere a entrada e saída da fila de espera para encontrar adversários via SignalR.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class CompetitiveMatchesController : ControllerBase
    {
        private readonly IRankMatchMakerHubClientService rankMatchMakerHubClientService;

        public CompetitiveMatchesController(IRankMatchMakerHubClientService rankMatchMakerHubClientService)
        {
            this.rankMatchMakerHubClientService = rankMatchMakerHubClientService;
        }

        #region Search Competitive Match

        /// <summary>
        /// Inicia a procura de uma partida competitiva (Ranked).
        /// </summary>
        /// <remarks>
        /// Conecta o cliente ao Hub 'RankMatchMaker' e coloca a equipa na fila de espera. 
        /// O sistema tentará encontrar um oponente com nível de habilidade (Rank) semelhante.
        /// </remarks>
        /// <param name="startSearch">Objeto contendo os dados necessários para iniciar a procura (ex: ID da equipa, critérios de rank).</param>
        /// <returns>Uma mensagem de confirmação de entrada na fila.</returns>
        /// <response code="200">Entrou na fila de matchmaking com sucesso.</response>
        /// <response code="400">Dados de pesquisa inválidos.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        [HttpPost("Search")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> StartMatch([FromBody] StartSearchDto startSearch)
        {
            await rankMatchMakerHubClientService.InitializeAsync();
            await rankMatchMakerHubClientService.JoinRankMatchMakerAsync(startSearch);
            return Ok("Conseguiu entrar no hub!");
        }

        /// <summary>
        /// Cancela a procura de uma partida competitiva em curso.
        /// </summary>
        /// <remarks>
        /// Remove a equipa da fila de espera do Hub 'RankMatchMaker', parando o processo de matchmaking.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa que pretende cancelar a procura (enviado no corpo do pedido).</param>
        /// <returns>Uma mensagem de confirmação de saída da fila.</returns>
        /// <response code="200">Saiu da fila de matchmaking com sucesso.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        [HttpPost("CancelSearch")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LeaveStartMatch([FromBody] Guid idTeam)
        {
            await rankMatchMakerHubClientService.InitializeAsync();
            await rankMatchMakerHubClientService.LeaveRankMatchMakerAsync();
            return Ok("Saiu do Hub com sucesso!");
        }

        #endregion
    }
}
