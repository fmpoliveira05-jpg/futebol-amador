namespace Application.DTOs.Chat
{
    public class CreateChatRoomDto
    {
        public string? roomId { get; set; }
        public string RoomName { get; set; }
        public List<string> MemberIds { get; set; }
    }
}
