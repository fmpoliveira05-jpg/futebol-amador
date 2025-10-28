using Application.DTOs.Team;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] para autenticação, validar se bloqueia todos os metodos
    public class TeamController : ControllerBase
    {
        private readonly ITeamService TeamService;

        public TeamController(ITeamService teamService)
        {
            TeamService = teamService;
        }

        private Guid GetCurrentUserId()
        {
            /*
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                throw new ValidationException("Token de utilizador inválido ou em falta.");
            }
            return userId;
             */
            return Guid.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto teamDto)
        {
            var creatorUserId = GetCurrentUserId();
            var newTeamId = await TeamService.CreateTeamAsync(teamDto, creatorUserId);

            return CreatedAtAction(nameof(GetTeamById), new { id = newTeamId }, new { id = newTeamId });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeamById(Guid id)
        {
            var team = await TeamService.GetTeamByIdAsync(id);
            return Ok(team);
        }

        [HttpPut("{teamId}")]
        public async Task<IActionResult> UpdateTeamInfo(Guid teamId, [FromBody] UpdateTeamDto dto)
        {
            var currentUserId = GetCurrentUserId();
            await TeamService.UpdateTeamInfoAsync(teamId, dto, currentUserId);
            return Ok("Equipa atualizada com sucesso.");
        }


        [HttpDelete("{teamId}")]
        public async Task<IActionResult> DeleteTeam(Guid teamId)
        {
            var currentUserId = GetCurrentUserId();
            await TeamService.DeleteTeamAsync(teamId, currentUserId);
            return NoContent();
        }

        [HttpGet("{teamId}/members")]
        public async Task<IActionResult> GetTeamPlayers(Guid teamId)
        {
                var players = await TeamService.GetTeamPlayersAsync(teamId);
                return Ok(players);
        }

        [HttpDelete("{teamId}/members/{playerIdToRemove}")]
        public async Task<IActionResult> RemovePlayerFromTeam(Guid teamId, Guid playerIdToRemove)
        {
                var playerRemovingId = GetCurrentUserId();
                await TeamService.RemovePlayerFromTeamAsync(teamId, playerIdToRemove, playerRemovingId);
                return NoContent();
        }

        [HttpPost("{teamId}/members/{playerIdToPromote}/promote")]
        public async Task<IActionResult> PromotePlayerToAdmin(Guid teamId, Guid playerIdToPromote)
        {
                var playerPromotingId = GetCurrentUserId();
                await TeamService.PromotePlayerToAdminAsync(teamId, playerIdToPromote, playerPromotingId);
                return Ok("Jogador promovido a admin.");
        }

        [HttpPost("{teamId}/members/{adminIdToDemote}/demote")]
        public async Task<IActionResult> DemoteAdminToPlayer(Guid teamId, Guid adminIdToDemote)
        {
            var adminDemotingId = GetCurrentUserId();
            await TeamService.DemoteAdminToPlayerAsync(teamId, adminIdToDemote, adminDemotingId);
            return Ok("Admin rebaixado a jogador.");
        }

        [HttpGet("{teamId}/members/requests")]
        public async Task<IActionResult> GetMembershipRequests(Guid teamId)
        {
            var adminUserId = GetCurrentUserId();
            var requests = await TeamService.GetMembershipRequestsAsync(teamId, adminUserId);
            return Ok(requests);
        }


        [HttpPost("{teamId}/members/requests/{requestId}/accept")]
        public async Task<IActionResult> AcceptMembershipRequest(Guid teamId, Guid requestId)
        {

            var adminUserId = GetCurrentUserId();
            await TeamService.AcceptMembershipRequestAsync(teamId, requestId, adminUserId);
            return Ok("Pedido de adesão aceite.");
        }

        [HttpPost("{teamId}/members/requests/{requestId}/reject")]
        public async Task<IActionResult> RejectMembershipRequest(Guid teamId, Guid requestId)
        {
            var adminUserId = GetCurrentUserId();
            await TeamService.RejectMembershipRequestAsync(teamId, requestId, adminUserId);
            return Ok("Pedido de adesão rejeitado.");
        }


        [HttpGet("{teamId}/search")] // Responde a GET /api/team
        public async Task<IActionResult> SearchTeams([FromQuery] TeamSearchFiltersDto filters)
        {

            // var teams = await _teamService.SearchTeamsAsync(filters);
            // return Ok(teams);
            return Ok("Endpoint 'SearchTeams' ainda não implementado no serviço.");
            
        }

    }
}