using Application.Interfaces.Services; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.DTOs.Chat; // Ou onde quer que o seu DTO esteja
namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Exige que o utilizador esteja autenticado
public class ChatController : ControllerBase
{
    private readonly IChatRoomService ChatService;

    public ChatController(IChatRoomService chatService)
    {
        ChatService = chatService;
    }

    [HttpPost("create-room")]
    public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRoomDto request)
    {
        // 2. Obter o ID do utilizador
        //Validar bem o que este passo faz!
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var roomId = await ChatService.CreateRoomAsync(request, userId);

        return Ok(new { roomId = roomId });
    }

    [HttpGet("my-rooms")]
    public async Task<IActionResult> GetMyRooms()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var rooms = await ChatService.GetMyRoomsAsync(userId);
        return Ok(rooms);
    }
}