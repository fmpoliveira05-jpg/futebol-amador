using Application.DTOs.Chat;
namespace Application.Interfaces.Services
{
    public interface IChatRoomService
    {
        Task<string> CreateRoomAsync(CreateChatRoomDto request, string createdByUserId);
        Task<string> CreateMatchRoomAsync(CreateChatRoomRequestDto request, string createdByUserId);
        Task<List<ChatRoomDto>> GetMyRoomsAsync(string userId);

    }
}
