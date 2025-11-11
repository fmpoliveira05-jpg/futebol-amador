using Application.DTOs;
using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.SuperAdmin;
using Application.DTOs.Team;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlayerController : ControllerBase
    {
        #region Inicializar
        private readonly IPlayerService playerService;
        private readonly IPlayerAuthorizationValidator playerAuthorizationValidator;
        private readonly IMembershipRequestService membershipRequestService;
        private readonly IAuthService authService;
        public PlayerController(IPlayerService playerService, IPlayerAuthorizationValidator playerAuthorizationValidator, IMembershipRequestService membershipRequestService, IAuthService authService)
        {
            this.playerService = playerService;
            this.playerAuthorizationValidator = playerAuthorizationValidator;
            this.membershipRequestService = membershipRequestService;
            this.authService = authService;
        }
        #endregion

        #region EndPoints

        #region CRUD Player
        [HttpPost]
        [Route("create-profile")]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerDto playerDto)
        {

            var newPlayerId = await playerService.CreatePlayerAsync(playerDto);
            var createPlayerResult = await authService.LoginAsync(playerDto.Email, playerDto.Password);
            return CreatedAtAction(
                    nameof(GetPlayer),
                    new { playerId = newPlayerId },
                    createPlayerResult
                    );
        }

        [HttpDelete("{playerId:required}")]
        public async Task<IActionResult> DeletePlayer(string playerId) 
        {
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), playerId);
            await playerService.DeletePlayerAsync(playerId);

            return NoContent();
        }

        [HttpGet("{playerId:required}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlayer(string playerId)
        {
            var playerDetails = await playerService.GetPlayerByIdAsync(playerId);

            return Ok(playerDetails);
        }

        [HttpGet()]
        [Route("get-my-profile")]
        public async Task<IActionResult> GetFullProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var playerDetails = await playerService.GetPlayerByIdAsync(userId);

            return Ok(playerDetails);
        }

        [HttpPut("{playerId:required}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(string playerId, [FromBody] UpdatePlayerDto dto)
        {
            //playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), playerId);
            var userId= User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            */

            await playerService.UpdatePlayerAsync(dto.playerId, dto);

            return Ok("Player information updated succesfully.");
        }

        #endregion

        #region MemberShipRequest

        [HttpGet("listTeamsToMemberShipRequest")]
        public async Task<IActionResult> ListTeams([FromQuery] FilterListTeamDto filter)
        {
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
                list = await playerService.GetTeamListWithFilters(filter);
            }
            else
            {
                list = await playerService.GetListTeams();
            }

            return Ok(list);
        }


        [HttpGet("{playerId}/membership-requests")]
        public async Task<IActionResult> GetMembershipRequests(string playerId, [FromQuery] FilterMembershipRequestsPlayer filters)
        {
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), playerId);

            var hasFilter = filters.MinDate.HasValue ||
                            filters.MaxDate.HasValue ||
                            !string.IsNullOrEmpty(filters.SenderName);

            IEnumerable<MemberShipRequestDto> requests;
            if (hasFilter)
            {
                requests = await membershipRequestService.GetMembershipRequestsAsyncPlayerWithFilters(playerId, filters);
            }
            else
            {
                requests = await membershipRequestService.GetMembershipRequestsAsyncPlayer(playerId);
            }

            return Ok(requests);
        }

        [HttpPost("{playerId:required}/membership-requests/accept")]
        public async Task<IActionResult> AcceptMembershipRequest(string playerId, [FromBody] Guid requestId)
        {
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), playerId);

            var dto = await membershipRequestService.AcceptMembershipRequestAsyncPlayer(playerId, requestId);
            return Ok(dto);
        }

        [HttpDelete("{playerId:required}/membership-requests/reject/{requestId:guid}")]
        public async Task<IActionResult> RejectMembershipRequest(string playerId, Guid requestId)
        {
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), playerId);

            var dto = await membershipRequestService.RejectMembershipRequestAsyncPlayer(playerId, requestId);
            return Ok(dto);
        }

        [HttpPost("{playerId:required}/membership-requests/send")]
        public async Task<IActionResult> SendMembershipRequest(string playerId, [FromBody] Guid teamId)
        {
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), playerId);

            var dto = await membershipRequestService.SendMembershipRequestAsyncPlayer(playerId, teamId);
            return Ok(dto);
        }

        #endregion

        #region Teams Operations

        [HttpPut("{playerId:required}/leave-team")]
        public async Task<IActionResult> LeaveTeam(string playerId)
        {
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), playerId);

            string teamName = await playerService.LeaveTeam(playerId);

            return Ok("Player succesfully left the team" + teamName + ".");
        }

        #endregion

        #endregion

        #region Private Methods
        private string GetCurrentUserId()
        {
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
