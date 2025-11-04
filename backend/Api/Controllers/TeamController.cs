using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService TeamService;

        public TeamController(ITeamService teamService)
        {
            TeamService = teamService;
        }

        #region CRUD Team
        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto teamDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var newTeamId = await TeamService.CreateTeamAsync(teamDto, userId);

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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await TeamService.UpdateTeamInfoAsync(teamId, dto, userId);
            return Ok("Equipa atualizada com sucesso.");
        }


        [HttpDelete("{teamId}")]
        public async Task<IActionResult> DeleteTeam(Guid teamId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await TeamService.DeleteTeamAsync(teamId, userId);
            return NoContent();
        }
        #endregion

        #region Search Teams 
        //Depois adaptar para o teamId, o player é Aut
        [HttpGet("{teamId}/search")] // Responde a GET /api/team
        public async Task<IActionResult> SearchTeams(Guid teamId, [FromQuery] FilterListTeamDto filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var isFilter = !string.IsNullOrEmpty(filter.NameTeam) ||
                           !string.IsNullOrEmpty(filter.NameRank) ||
                           !string.IsNullOrEmpty(filter.City) ||
                           filter.MinNumberPoints.HasValue ||
                           filter.MaxNumberPoints.HasValue ||
                           filter.MinAge.HasValue ||
                           filter.MaxAge.HasValue ||
                           filter.MinNumberPlayers.HasValue ||
                           filter.MaxNumberPlayers.HasValue;

            //Falta mandar o userId para o service
            IEnumerable<InfoTeamsDto> list;
            if (isFilter)
            {
                list = await TeamService.SearchTeamsWithFiltersAsync(teamId, filter);
            }
            else
            {
                list = await TeamService.SearchTeamsAsync(teamId);
            }

            return Ok(list);
        }
        #endregion

        #region Team Members Management

        [HttpGet("{teamId}/members")]
        public async Task<IActionResult> GetTeamPlayersWithFilters(Guid teamId, [FromQuery] FilterTeamPlayers filters)
        {
            IEnumerable<PlayerDetailsDto> players;
            var hasFilters = filters.IsAdmin.HasValue ||
                             !string.IsNullOrEmpty(filters.Name) ||
                             filters.MinAge.HasValue ||
                             filters.MaxAge.HasValue ||
                             filters.Position.HasValue;

            if (hasFilters)
            {
                players = await TeamService.GetTeamPlayersAsyncWithFilters(teamId, filters);
            }
            else
            {
                players = await TeamService.GetTeamPlayersAsync(teamId);
            }

            return Ok(players);
        }

        [HttpDelete("{teamId}/members/{playerIdToRemove}")]
        public async Task<IActionResult> RemovePlayerFromTeam(Guid teamId, string playerIdToRemove)
        {
            var playerRemovingId = GetCurrentUserId();
            await TeamService.RemovePlayerFromTeamAsync(teamId, playerIdToRemove, playerRemovingId);
            return NoContent();
        }

        #region Manage Admins

        [HttpPut("{teamId}/members/promote/{playerIdToPromote}")]
        public async Task<IActionResult> PromotePlayerToAdmin(Guid teamId, string playerIdToPromote)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await TeamService.PromotePlayerToAdminAsync(teamId, playerIdToPromote, userId);
            return Ok("Jogador promovido a admin.");
        }

        [HttpPut("{teamId}/members/demote/{adminIdToDemote}")]
        public async Task<IActionResult> DemoteAdminToPlayer(Guid teamId, string adminIdToDemote)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await TeamService.DemoteAdminToPlayerAsync(teamId, adminIdToDemote, userId);
            return Ok("Admin rebaixado a jogador.");
        }

        #endregion

        #endregion

        #region Membership Requests Management

        //Falta também validar se a team existe mas isso a principio a Aut faz
        //Falta fazer validação de autentificação e autorização aqui ou se for no service, no proprio service
        [HttpGet("{teamId}/playersWithoutTeam")]
        [Authorize]
        public async Task<IActionResult> GetPlayersWithouTeam(Guid teamId, [FromQuery] FilterPlayersWithoutTeamDto filter)
        {
            IEnumerable<PlayerWithoutTeamInfoDto> players;
            var hasFilter = !string.IsNullOrEmpty(filter.PlayerName) ||
                            !string.IsNullOrEmpty(filter.City) ||
                            filter.MinAge.HasValue ||
                            filter.MaxAge.HasValue ||
                            filter.MinHeight.HasValue ||
                            filter.MaxHeight.HasValue ||
                            filter.Position.HasValue;


            try 
            {
                if (hasFilter)
                {
                    players = await TeamService.GetPlayersWithoutTeamWithFilters(filter);
                }
                else
                {
                    players = await TeamService.GetPlayersWithoutTeam();
                }
                return Ok(players);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado no servidor.", details = ex.Message });
            }
        }

        /*
        [HttpGet("{teamId}/membership-request")]
        public async Task<IActionResult> MembershipRequests(Guid teamId, [FromQuery] FilterMembershipRequestsTeam filters)
        {
            if (teamId == Guid.Empty)
            {
                return BadRequest("O id da equipa é obrigatorio");
            }

            var adminUserId = GetCurrentUserId();

            try
            {
                IEnumerable<MemberShipRequestDto> membershipRequests;
                var hasFilter = filters.MinDate.HasValue ||
                    filters.MaxDate.HasValue ||
                    !string.IsNullOrEmpty(filters.SenderName);

                if (hasFilter)
                {
                    membershipRequests = await TeamService.GetMembershipRequestsAsyncWithFilters(teamId, adminUserId, filters);
                }
                else
                {
                    membershipRequests = await TeamService.GetMembershipRequestsAsync(teamId, adminUserId);
                }

                return Ok(membershipRequests);
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NullReferenceException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado no servidor.", details = ex.Message });
            }
        }
        */


        [HttpPost("{teamId}/membership-request/accept")]
        public async Task<IActionResult> AcceptMembershipRequest(Guid teamId, [FromBody] Guid requestId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok();
        }

        [HttpDelete("{teamId}/membership-request/{requestId}/reject")]
        public async Task<IActionResult> RejectMembershipRequest(Guid teamId, Guid requestId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok();
        }

        /*
        [HttpPost("{teamId}/membership-requests/send")]
        public async Task<IActionResult> SendMembershipRequest(Guid teamId, [FromBody] string playerId)
        {
            try
            {
                var adminId = GetCurrentUserId();
                var dto = await TeamService.SendMembershipRequestAsync(teamId, playerId, adminId);

                return Ok(dto);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro inesperado no servidor.", details = ex.Message });
            }
        }
        */
        #endregion

        #region private Methods
        [Authorize]
        private string GetCurrentUserId()
        {
            //validar null
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User ID not found in claims.");
            }

            return userId;
        }
        #endregion
    }
}