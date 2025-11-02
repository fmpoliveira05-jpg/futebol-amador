using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Services.Hub.ClienteService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitiveMatchesController : ControllerBase
    {
        private readonly IRankMatchMakerHubClientService rankMatchMakerHubClientService;

        public CompetitiveMatchesController(IRankMatchMakerHubClientService rankMatchMakerHubClientService)
        {
            this.rankMatchMakerHubClientService = rankMatchMakerHubClientService;
        }

        #region Search Competitive Match

        [HttpPost("Search")]
        public async Task<IActionResult> StartMatch([FromBody] StartSearchDto startSearch)
        {
            await rankMatchMakerHubClientService.InitializeAsync();
            await rankMatchMakerHubClientService.JoinRankMatchMakerAsync(startSearch);
            return Ok("Conseguiu entrar no hub!");
        }

        [HttpPost("CancelSearch")]
        public async Task<IActionResult> LeaveStartMatch([FromBody] Guid idTeam)
        {
            await rankMatchMakerHubClientService.InitializeAsync();
            await rankMatchMakerHubClientService.LeaveRankMatchMakerAsync();
            return Ok("Saiu do Hub com sucesso!");
        }

        #endregion
    }
}
