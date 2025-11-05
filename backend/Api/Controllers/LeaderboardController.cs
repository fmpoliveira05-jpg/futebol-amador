using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            this.leaderboardService = leaderboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaderboard()
        {
            var leaderboard = await leaderboardService.GetLeaderboardAsync();
            return Ok(leaderboard);
        }
    }
}