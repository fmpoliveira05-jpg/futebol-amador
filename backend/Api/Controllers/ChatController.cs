using Application.DTOs.Chat; 
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de salas de chat e comunicação em tempo real.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ChatController : ControllerBase
    {
        private readonly IChatRoomService ChatService;

        /// <summary>
        /// Construtor do ChatController.
        /// </summary>
        /// <param name="chatService">Serviço responsável pela gestão e lógica de negócio das salas de chat.</param>
        public ChatController(IChatRoomService chatService)
        {
            ChatService = chatService;
        }

        /// <summary>
        /// Cria uma nova sala de chat privada ou de grupo.
        /// </summary>
        /// <remarks>
        /// Permite criar uma sala de chat especificando o nome e os membros que farão parte dela.
        /// O criador da sala é adicionado automaticamente à lista de membros.
        /// </remarks>
        /// <param name="request">Objeto contendo o nome da sala e os IDs dos membros a adicionar.</param>
        /// <returns>Um objeto JSON contendo o ID da sala recém-criada.</returns>
        /// <response code="200">Sala criada com sucesso.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        /// <response code="400">Dados da sala inválidos.</response>
        [HttpPost("create-room")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // Retorna { roomId = "..." }
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRoomDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var roomId = await ChatService.CreateRoomAsync(request, userId);

            return Ok(new { roomId = roomId });
        }

        /// <summary>
        /// Obtém todas as salas de chat em que o utilizador atual participa.
        /// </summary>
        /// <remarks>
        /// Retorna uma lista de salas de chat onde o ID do utilizador consta na lista de membros.
        /// </remarks>
        /// <returns>Uma lista de objetos <see cref="ChatRoomDto"/> representando as salas do utilizador.</returns>
        /// <response code="200">Lista de salas retornada com sucesso.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        [HttpGet("my-rooms")]
        [ProducesResponseType(typeof(List<ChatRoomDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyRooms()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var rooms = await ChatService.GetMyRoomsAsync(userId);
            return Ok(rooms);
        }
    }
}