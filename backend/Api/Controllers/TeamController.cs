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
    [Authorize]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        #region Initialization
        private readonly ITeamService TeamService;
        private readonly IMembershipRequestService MemberShipRequestService;
        private readonly IPlayerAuthorizationService PlayerAuthorizationService;
        public TeamController(ITeamService teamService, IMembershipRequestService MemberShipRequestService)
        {
            this.TeamService = teamService;
            this.MemberShipRequestService = MemberShipRequestService;
        }

        #endregion

        #region EndPoints

        #region CRUD Team
        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto teamDto)
        {            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var newTeamId = await TeamService.CreateTeamAsync(teamDto, userId);

            return CreatedAtAction(nameof(GetTeamById), new { id = newTeamId }, new { id = newTeamId });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
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

        [HttpGet("{teamId}/search")]
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
            await PlayerAuthorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), teamId);

            IEnumerable<MemberShipRequestDto> membershipRequests;
            var hasFilter = filters.MinDate.HasValue ||
                filters.MaxDate.HasValue ||
                !string.IsNullOrEmpty(filters.SenderName);

            if (hasFilter)
            {
                membershipRequests = await MemberShipRequestService.GetMembershipRequestsByTeamWithFilters(teamId, filters);
            }
            else
            {
                membershipRequests = await MemberShipRequestService.GetMembershipRequestsByTeam(teamId);
            }

            return Ok(membershipRequests);
        }
        */



        [HttpPost("{teamId}/membership-request/accept")]
        public async Task<IActionResult> AcceptMembershipRequest(Guid teamId, [FromBody] Guid requestId)
        {
            await PlayerAuthorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), teamId);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok();
        }

        [HttpDelete("{teamId}/membership-request/{requestId}/reject")]
        public async Task<IActionResult> RejectMembershipRequest(Guid teamId, Guid requestId)
        {
            await PlayerAuthorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), teamId);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok();
        }

        [HttpPost("{teamId}/membership-requests/send")]
        public async Task<IActionResult> SendMembershipRequest(Guid teamId, [FromBody] string playerId)
        {
            await PlayerAuthorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), teamId);
            var dto = await MemberShipRequestService.SendMembershipRequestTeam(teamId, playerId);

            return Ok(dto);
           
        }

        #endregion

        #endregion

        #region private Methods
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