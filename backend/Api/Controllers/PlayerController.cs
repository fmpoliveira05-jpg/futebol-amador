using Application.DTOs;
using Application.DTOs.Filters;
using Application.DTOs.Membership;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de perfis de jogadores, atualizações de dados e gestão de pedidos de adesão a equipas.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class PlayerController : ControllerBase
    {
        #region Inicializar
        private readonly IPlayerService playerService;
        private readonly IPlayerAuthorizationValidator playerAuthorizationValidator;
        private readonly IMembershipRequestService membershipRequestService;
        private readonly IAuthService authService;

        /// <summary>
        /// Construtor do PlayerController.
        /// </summary>
        /// <param name="playerService">Serviço de gestão de jogadores.</param>
        /// <param name="playerAuthorizationValidator">Validador de permissões do jogador.</param>
        /// <param name="membershipRequestService">Serviço de gestão de pedidos de adesão.</param>
        /// <param name="authService">Serviço de autenticação.</param>
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
        /// <summary>
        /// Regista um novo jogador na aplicação.
        /// </summary>
        /// <remarks>
        /// Este endpoint é público. Cria o perfil do jogador na base de dados e realiza o login automático no Firebase/AuthService, retornando o token.
        /// </remarks>
        /// <param name="playerDto">Dados de registo do jogador (Nome, Email, Password, etc.).</param>
        /// <returns>Dados de login (Token) e ID do novo jogador.</returns>
        /// <response code="201">Jogador criado com sucesso.</response>
        /// <response code="400">Dados inválidos (ex: email já existente, idade inválida).</response>
        [HttpPost]
        [Route("create-profile")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)] // Retorna LoginResponseDto (ou similar)
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Elimina a conta de um jogador.
        /// </summary>
        /// <remarks>
        /// O utilizador autenticado só pode eliminar a sua própria conta.
        /// </remarks>
        /// <param name="playerId">ID do jogador a eliminar.</param>
        /// <response code="204">Conta eliminada com sucesso.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        /// <response code="403">Utilizador tentou eliminar uma conta que não lhe pertence.</response>
        [HttpDelete("{playerId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeletePlayer(string playerId) 
        {
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), playerId);
            await playerService.DeletePlayerAsync(playerId);

            return NoContent();
        }

        /// <summary>
        /// Lista jogadores com base em filtros.
        /// </summary>
        /// <remarks>
        /// Endpoint público para pesquisar jogadores (ex: para convidar para equipas).
        /// </remarks>
        /// <param name="filter">Filtros de pesquisa (Nome, Posição, etc.).</param>
        /// <returns>Lista de jogadores encontrados.</returns>
        /// <response code="200">Lista retornada com sucesso.</response>
        [HttpGet("listPlayers")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<InfoPlayerDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlayerList([FromQuery] FilterTeamDto? filter)
        {
            var listPlayers = await playerService.ListPlayers(filter);

            return Ok(listPlayers);
        }

        /// <summary>
        /// Obtém os detalhes públicos de um jogador específico.
        /// </summary>
        /// <param name="playerId">ID do jogador.</param>
        /// <returns>Detalhes do jogador.</returns>
        /// <response code="200">Dados do jogador retornados com sucesso.</response>
        /// <response code="404">Jogador não encontrado.</response>
        [HttpGet("details/{playerId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PlayerDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlayer(string playerId)
        {
            var playerDetails = await playerService.GetPlayerByIdAsync(playerId);

            return Ok(playerDetails);
        }

        /// <summary>
        /// Obtém o perfil completo do utilizador autenticado.
        /// </summary>
        /// <remarks>
        /// Identifica o utilizador através do token JWT.
        /// </remarks>
        /// <returns>Detalhes completos do perfil.</returns>
        /// <response code="200">Perfil retornado com sucesso.</response>
        /// <response code="401">Utilizador não autenticado ou ID não encontrado no token.</response>
        [HttpGet()]
        [Route("get-my-profile")]
        [ProducesResponseType(typeof(PlayerDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>
        /// Atualiza os dados do perfil de um jogador.
        /// </summary>
        /// <remarks>
        /// O utilizador autenticado só pode atualizar o seu próprio perfil.
        /// </remarks>
        /// <param name="playerId">ID do jogador a atualizar.</param>
        /// <param name="dto">Novos dados do jogador.</param>
        /// <returns>Dados do jogador atualizados.</returns>
        /// <response code="200">Perfil atualizado com sucesso.</response>
        /// <response code="400">Dados de atualização inválidos.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        /// <response code="403">Utilizador tentou atualizar outro perfil.</response>
        [HttpPut("update/{playerId}")]
        [Authorize]
        [ProducesResponseType(typeof(UpdatePlayerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateUser(string playerId, [FromBody] UpdatePlayerDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(userId, playerId);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            

            var updatedPlayer = await playerService.UpdatePlayerAsync(dto.playerId, dto);

            return Ok(updatedPlayer);
        }

        #endregion

        #region MemberShipRequest

        /// <summary>
        /// Lista equipas disponíveis para envio de pedidos de adesão.
        /// </summary>
        /// <remarks>
        /// Permite filtrar equipas por nome, cidade, rank, etc.
        /// </remarks>
        /// <param name="filter">Filtros de pesquisa de equipas.</param>
        /// <returns>Lista de equipas.</returns>
        /// <response code="200">Lista retornada com sucesso.</response>
        [HttpGet("{playerId}/listTeamsToMemberShipRequest")]
        [ProducesResponseType(typeof(IEnumerable<InfoTeamsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListTeams(string playerId, [FromQuery] FilterListTeamDto filter)
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
                list = await playerService.GetTeamListWithFilters(playerId, filter);
            }
            else
            {
                list = await playerService.GetListTeams(playerId);
            }

            return Ok(list);
        }


        /// <summary>
        /// Obtém os pedidos de adesão recebidos pelo jogador (convites de equipas).
        /// </summary>
        /// <param name="playerId">ID do jogador.</param>
        /// <param name="filters">Filtros opcionais (ex: data, nome da equipa).</param>
        /// <returns>Lista de pedidos de adesão.</returns>
        /// <response code="200">Lista de pedidos retornada.</response>
        /// <response code="403">Utilizador não tem permissão para ver estes pedidos.</response>
        [HttpGet("{playerId}/membership-requests")]
        [ProducesResponseType(typeof(IEnumerable<MemberShipRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        /// <summary>
        /// Aceita um convite de uma equipa para se juntar a ela.
        /// </summary>
        /// <param name="playerId">ID do jogador que aceita.</param>
        /// <param name="requestId">ID do pedido de adesão.</param>
        /// <returns>Detalhes do pedido aceite.</returns>
        /// <response code="200">Pedido aceite com sucesso. Jogador adicionado à equipa.</response>
        /// <response code="403">Permissão negada.</response>
        [HttpPost("{playerId}/membership-requests/accept")]
        [ProducesResponseType(typeof(MemberShipRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AcceptMembershipRequest(string playerId, [FromBody] Guid requestId)
        {
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), playerId);

            var dto = await membershipRequestService.AcceptMembershipRequestAsyncPlayer(playerId, requestId);
            return Ok(dto);
        }

        /// <summary>
        /// Rejeita um convite de uma equipa.
        /// </summary>
        /// <param name="playerId">ID do jogador que rejeita.</param>
        /// <param name="requestId">ID do pedido a rejeitar.</param>
        /// <returns>Detalhes do pedido rejeitado.</returns>
        /// <response code="200">Pedido rejeitado e removido.</response>
        /// <response code="403">Permissão negada.</response>
        [HttpDelete("{playerId}/membership-requests/reject/{requestId:guid}")]
        [ProducesResponseType(typeof(MemberShipRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RejectMembershipRequest(string playerId, Guid requestId)
        {
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), playerId);

            var dto = await membershipRequestService.RejectMembershipRequestAsyncPlayer(playerId, requestId);
            return Ok(dto);
        }

        /// <summary>
        /// Envia um pedido para se juntar a uma equipa (Candidatura).
        /// </summary>
        /// <param name="playerId">ID do jogador que envia a candidatura.</param>
        /// <param name="teamId">ID da equipa à qual se quer juntar.</param>
        /// <returns>Detalhes do pedido criado.</returns>
        /// <response code="200">Pedido enviado com sucesso.</response>
        /// <response code="400">Jogador já tem equipa ou pedido inválido.</response>
        /// <response code="403">Permissão negada.</response>
        [HttpPost("{playerId}/membership-requests/send")]
        [ProducesResponseType(typeof(MemberShipRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SendMembershipRequest(string playerId, [FromBody] InviteTeamRequest request)
        {
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), playerId);

            var dto = await membershipRequestService.SendMembershipRequestAsyncPlayer(playerId, request.TeamId);
            return Ok(dto);
        }

        #endregion

        #region Teams Operations

        /// <summary>
        /// O jogador sai da equipa atual.
        /// </summary>
        /// <remarks>
        /// Se o jogador for o único Admin ou o único membro, podem ocorrer regras adicionais (ex: equipa ser eliminada ou novo admin promovido).
        /// </remarks>
        /// <param name="playerId">ID do jogador que quer sair.</param>
        /// <returns>Informação atualizada do jogador (sem equipa).</returns>
        /// <response code="200">Saiu da equipa com sucesso.</response>
        /// <response code="403">Permissão negada.</response>
        [HttpPut("{playerId}/leave-team")]
        [ProducesResponseType(typeof(InfoPlayerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> LeaveTeam(string playerId)
        {
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), playerId);

            var playerDto = await playerService.LeaveTeam(playerId);

            return Ok(playerDto);
        }

        #endregion

        [HttpPut("device-token")]
        public async Task<IActionResult> UpdateDeviceToken([FromBody] DeviceTokenDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await playerService.UpdateDeviceTokenAsync(userId, dto.Token);

            if (!result) return BadRequest("Erro ao atualizar token");

            return Ok();
        }

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
