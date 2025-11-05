using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet]
        [AllowAnonymous] // qualquer utilizador pode ver
        public async Task<IActionResult> GetLeaderboard()
        {
            var leaderboard = await _leaderboardService.GetLeaderboardAsync();
            return Ok(leaderboard);
        }
    }
}