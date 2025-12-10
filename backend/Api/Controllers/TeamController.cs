using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão completa de equipas: criação, edição, gestão de membros, pesquisas e visualização de perfis de equipa.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TeamController : ControllerBase
    {
        #region Initialization
        private readonly ITeamService TeamService;
        private readonly IMembershipRequestService MemberShipRequestService;
        private readonly IPlayerAuthorizationService PlayerAuthorizationService;
        private readonly INotificationFirebaseService notificationFirebaseService;

        /// <summary>
        /// Construtor do TeamController.
        /// </summary>
        /// <param name="TeamService">Serviço com a lógica de negócio das equipas.</param>
        /// <param name="MemberShipRequestService">Serviço para gerir pedidos de adesão.</param>
        /// <param name="PlayerAuthorizationService">Serviço para validar permissões dos jogadores.</param>
        public TeamController(ITeamService TeamService, IMembershipRequestService MemberShipRequestService, 
           IPlayerAuthorizationService PlayerAuthorizationService, INotificationFirebaseService notificationFirebaseService)
        {
            this.TeamService = TeamService;
            this.MemberShipRequestService = MemberShipRequestService;
            this.PlayerAuthorizationService = PlayerAuthorizationService;
            this.notificationFirebaseService = notificationFirebaseService;
        }

        #endregion

        #region EndPoints

        #region CRUD Team
        /// <summary>
        /// Cria uma nova equipa na plataforma.
        /// </summary>
        /// <remarks>
        /// Esta operação é fundamental para iniciar a participação de um jogador.
        /// 
        /// **Regras de Negócio:**
        /// * O utilizador autenticado que cria a equipa é designado como **Administrador** (dono) inicial.
        /// * Um utilizador só pode pertencer a uma equipa de cada vez (validação de unicidade).
        /// 
        /// **Retorno (201 Created):**
        /// A resposta inclui o DTO completo da equipa criada e, crucialmente,
        /// o Header 'Location' que aponta para o URI de acesso direto ao novo recurso.
        /// </remarks>
        /// <param name="teamDto">
        /// Data Transfer Object (DTO) contendo os dados essenciais para a criação da equipa,
        /// incluindo o nome, descrição, e informações do campo principal.
        /// </param>
        /// <returns>
        /// Retorna o objeto DTO da equipa criada e o código de status 201 Created.
        /// </returns>
        /// <response code="201">
        /// Equipa criada com sucesso. O Header 'Location' aponta para o endpoint GET da equipa (ex: /api/Team/{id}).
        /// </response>
        /// <response code="400">
        /// Dados inválidos (ex: falha na validação do [CreateTeamDto]) ou violação de regras de negócio
        /// (ex: o jogador autenticado já possui uma equipa).
        /// </response>
        /// <response code="401">
        /// O utilizador não está autenticado e, portanto, não pode realizar a criação da equipa.
        /// </response>
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto teamDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var newTeam = await TeamService.CreateTeamAsync(teamDto, userId);

            return CreatedAtAction(nameof(GetTeamById), new { id = newTeam.Id }, newTeam);
        }

        /// <summary>
        /// Obtém os detalhes públicos de uma equipa pelo seu ID.
        /// </summary>
        /// <param name="id">ID da equipa.</param>
        /// <returns>Detalhes da equipa.</returns>
        /// <response code="200">Detalhes da equipa retornados com sucesso.</response>
        /// <response code="404">Equipa não encontrada.</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TeamDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTeamById(Guid id)
        {
            var team = await TeamService.GetTeamByIdAsync(id);

            return Ok(team);
        }

        /// <summary>
        /// Atualiza as informações de uma equipa existente (Nome, Descrição, Logótipo, Campo Principal).
        /// </summary>
        /// <remarks>
        /// Esta é uma operação HTTP PUT idempotente.
        /// 
        /// **Requer Autorização:** Apenas o utilizador que possui permissão de **Administrador**
        /// da equipa especificada pelo <paramref name="teamId"/> pode executar esta ação.
        /// 
        /// **Corpo do Pedido (Request Body):**
        /// O corpo deve conter todos os campos necessários para a atualização, conforme definido
        /// no esquema do [CreateTeamDto] (incluindo as informações aninhadas do campo de jogo).
        /// </remarks>
        /// <param name="teamId">O identificador único (GUID) da equipa a ser atualizada.</param>
        /// <param name="dto">O Data Transfer Object (DTO) contendo os novos dados da equipa.</param>
        /// <response code="200">
        /// Retorna o objeto DTO da equipa com as informações atualizadas, confirmando a operação.
        /// </response>
        /// <response code="400">
        /// Ocorreu um erro de validação (ex: dados incompletos ou fora dos limites de caracteres).
        /// O corpo da resposta (Body) conterá o objeto de erros de validação.
        /// </response>
        /// <response code="403">
        /// O utilizador autenticado não tem a permissão de Administrador necessária para modificar a equipa.
        /// </response>
        /// <response code="404">
        /// A equipa especificada pelo <paramref name="teamId"/> não foi encontrada.
        /// </response>
        [HttpPut("{teamId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Adicionei 404, comum para recursos que não existem.
        public async Task<IActionResult> UpdateTeamInfo(Guid teamId, [FromBody] CreateTeamDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var teamUpdate = await TeamService.UpdateTeamInfoAsync(teamId, dto, userId);
            return Ok(teamUpdate);
        }

        /// <summary>
        /// Elimina uma equipa.
        /// </summary>
        /// <remarks>
        /// Apenas administradores da equipa podem realizar esta ação. 
        /// A equipa não pode ter jogos agendados ou em progresso.
        /// </remarks>
        /// <param name="teamId">ID da equipa a eliminar.</param>
        /// <response code="204">Equipa eliminada com sucesso.</response>
        /// <response code="400">Equipa tem jogos pendentes.</response>
        /// <response code="403">Utilizador não é administrador.</response>
        [HttpDelete("{teamId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteTeam(Guid teamId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await TeamService.DeleteTeamAsync(teamId, userId);
            return NoContent();
        }

        /// <summary>
        /// Obtém o nome e informações básicas de uma equipa adversária.
        /// </summary>
        /// <param name="teamId">ID da equipa.</param>
        /// <returns>Informações básicas da equipa.</returns>
        /// <response code="200">Informações retornadas com sucesso.</response>
        /// <response code="404">Equipa não encontrada.</response>
        [HttpGet("opponent/{teamId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TeamName(Guid teamId)
        {
            var team = await TeamService.getOpponent(teamId);
            return Ok(team);
        }
        #endregion

        #region Search Teams 

        /// <summary>
        /// Lista equipas com base em filtros gerais.
        /// </summary>
        /// <remarks>
        /// Endpoint público para pesquisa de equipas.
        /// </remarks>
        /// <param name="filter">Filtros de pesquisa (Nome, Rank, Cidade, etc.).</param>
        /// <returns>Lista de equipas encontradas.</returns>
        /// <response code="200">Lista retornada com sucesso.</response>
        [HttpGet("listTeams")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<InfoTeamsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListTeam([FromQuery] FilterListTeamDto? filter)
        {
            var list = await TeamService.GetListTeams(filter);

            return Ok(list);
        }

        /// <summary>
        /// Pesquisa equipas para fins de Matchmaking ou consulta específica.
        /// </summary>
        /// <param name="teamId">ID da equipa que está a realizar a pesquisa (opcional para contexto).</param>
        /// <param name="filter">Filtros de pesquisa.</param>
        /// <returns>Lista de equipas.</returns>
        /// <response code="200">Resultados da pesquisa.</response>
        [HttpGet("{teamId}/search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<InfoTeamsDto>), StatusCodes.Status200OK)]
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

        /// <summary>
        /// Obtém a lista de membros de uma equipa, com filtros opcionais.
        /// </summary>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="filters">Filtros (ex: mostrar apenas Admins, filtrar por posição).</param>
        /// <returns>Lista de jogadores da equipa.</returns>
        /// <response code="200">Lista de membros retornada com sucesso.</response>
        /// <response code="404">Equipa não encontrada.</response>
        [HttpGet("{teamId}/members")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<PlayerDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Remove (expulsa) um jogador da equipa.
        /// </summary>
        /// <remarks>
        /// Apenas administradores podem remover membros. 
        /// Um admin só pode remover outro admin se tiver mais antiguidade no cargo.
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="playerIdToRemove">ID do jogador a remover.</param>
        /// <response code="204">Jogador removido com sucesso.</response>
        /// <response code="403">Permissão negada (não é admin ou hierarquia insuficiente).</response>
        [HttpDelete("{teamId}/members/{playerIdToRemove}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemovePlayerFromTeam(Guid teamId, string playerIdToRemove)
        {
            var playerRemovingId = GetCurrentUserId();
            
            await TeamService.RemovePlayerFromTeamAsync(teamId, playerIdToRemove, playerRemovingId);
            await notificationFirebaseService.SendNotificationToUser(playerIdToRemove, null, "Saída da Equipa","Você foi removido da equipa.");

            return NoContent();
        }

        #region Manage Admins

        /// <summary>
        /// Promove um membro da equipa a Administrador.
        /// </summary>
        /// <remarks>
        /// Apenas administradores podem promover outros membros.
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="playerIdToPromote">ID do jogador a promover.</param>
        /// <response code="200">Jogador promovido com sucesso.</response>
        /// <response code="403">Permissão negada.</response>
        [HttpPut("{teamId}/members/promote/{playerIdToPromote}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PromotePlayerToAdmin(Guid teamId, string playerIdToPromote)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            await TeamService.PromotePlayerToAdminAsync(teamId, playerIdToPromote, userId);
            await notificationFirebaseService.SendNotificationToUser(playerIdToPromote, null, "Promoção a Administrador", "Parabéns! Você foi promovido a administrador da sua equipa.");

            return Ok("Jogador promovido a admin.");
        }

        /// <summary>
        /// Rebaixa um Administrador a membro normal.
        /// </summary>
        /// <remarks>
        /// Requer permissões de administrador e hierarquia superior (antiguidade).
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="adminIdToDemote">ID do administrador a rebaixar.</param>
        /// <response code="200">Admin rebaixado com sucesso.</response>
        /// <response code="403">Permissão negada.</response>
        [HttpPut("{teamId}/members/demote/{adminIdToDemote}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DemoteAdminToPlayer(Guid teamId, string adminIdToDemote)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await TeamService.DemoteAdminToPlayerAsync(teamId, adminIdToDemote, userId);

            await notificationFirebaseService.SendNotificationToUser(adminIdToDemote, null, "Despromovido", "Você foi despromovido a jogador de equipa.");
            
            return Ok("Admin rebaixado a jogador.");
        }

        #endregion

        #endregion

        #region Membership Requests Management

        /// <summary>
        /// Obtém a lista de jogadores sem equipa (Free Agents), com filtros opcionais.
        /// </summary>
        /// <remarks>
        /// Útil para encontrar jogadores para recrutar.
        /// </remarks>
        /// <param name="teamId">ID da equipa (contexto).</param>
        /// <param name="filter">Filtros de pesquisa (Posição, Idade, etc.).</param>
        /// <returns>Lista de jogadores disponíveis.</returns>
        /// <response code="200">Lista retornada com sucesso.</response>
        /// <response code="400">Filtros inválidos.</response>
        [HttpGet("{teamId}/playersWithoutTeam")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PlayerWithoutTeamInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPlayersWithouTeam(Guid teamId, [FromQuery] FilterTeamDto filter)
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

        /// <summary>
        /// Obtém os pedidos de adesão recebidos pela equipa (candidaturas de jogadores).
        /// </summary>
        /// <remarks>
        /// Apenas administradores podem ver os pedidos.
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="filters">Filtros opcionais.</param>
        /// <returns>Lista de pedidos de adesão.</returns>
        /// <response code="200">Lista retornada com sucesso.</response>
        /// <response code="403">Utilizador não é administrador.</response>
        [HttpGet("{teamId}/membership-request")]
        [ProducesResponseType(typeof(IEnumerable<MemberShipRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> MembershipRequests(Guid teamId, [FromQuery] FilterMembershipRequestsTeam filters)
        {
            await PlayerAuthorizationService.UserAuthorizationIsAdminTeamById(GetCurrentUserId(), teamId);
            var playerId = GetCurrentUserId();

            var hasFilter = filters.MinDate.HasValue ||
                            filters.MaxDate.HasValue ||
                            !string.IsNullOrEmpty(filters.SenderName);

            IEnumerable<MemberShipRequestDto> membershipRequests;
            if (hasFilter)
            {
                membershipRequests = await MemberShipRequestService.GetMembershipRequestsByTeamWithFilters(teamId, filters, playerId);
            }
            else
            {
                membershipRequests = await MemberShipRequestService.GetMembershipRequestsByTeam(teamId, playerId);
            }

            return Ok(membershipRequests);
        }

        /// <summary>
        /// Aceita a candidatura de um jogador à equipa.
        /// </summary>
        /// <remarks>
        /// O jogador é adicionado à lista de membros. Requer permissão de administrador.
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="requestId">ID do pedido a aceitar.</param>
        /// <response code="200">Pedido aceite com sucesso.</response>
        /// <response code="403">Permissão negada.</response>
        [HttpPost("{teamId}/membership-request/accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AcceptMembershipRequest(Guid teamId, [FromBody] Guid requestId)
        {
            var userId = GetCurrentUserId();
            //await PlayerAuthorizationService.UserAuthorizationIsAdminTeamById(userId, teamId);
            await MemberShipRequestService.AcceptMembershipRequestTeam(teamId, requestId, userId);
            return Ok();
        }

        /// <summary>
        /// Rejeita a candidatura de um jogador.
        /// </summary>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="requestId">ID do pedido a rejeitar.</param>
        /// <response code="200">Pedido rejeitado com sucesso.</response>
        /// <response code="403">Permissão negada.</response>
        [HttpDelete("{teamId}/membership-request/{requestId}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RejectMembershipRequest(Guid teamId, Guid requestId)
        {
            var userId = GetCurrentUserId();
            //await PlayerAuthorizationService.UserAuthorizationIsAdminTeamById(userId, teamId);
            await MemberShipRequestService.RejectMembershipRequestTeam(teamId, requestId, userId);
            return Ok();
        }

        /// <summary>
        /// Envia um convite da equipa para um jogador se juntar a ela.
        /// </summary>
        /// <remarks>
        /// Apenas administradores podem enviar convites.
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="playerId">ID do jogador a convidar.</param>
        /// <returns>Detalhes do convite criado.</returns>
        /// <response code="200">Convite enviado com sucesso.</response>
        /// <response code="400">Jogador já tem equipa ou convite duplicado.</response>
        /// <response code="403">Permissão negada.</response>
        [HttpPost("{teamId}/membership-requests/send")]
        [ProducesResponseType(typeof(MemberShipRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SendMembershipRequest(Guid teamId, [FromBody] string playerId)
        {
            var senderId = GetCurrentUserId();
            //await PlayerAuthorizationService.UserAuthorizationIsAdminTeamById(senderId, teamId);
            var dto = await MemberShipRequestService.SendMembershipRequestTeam(teamId, playerId, senderId);

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