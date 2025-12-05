namespace Application.DTOs.Chat
{
    public class CreateChatRoomDto
    {
        public string RoomName { get; set; }
        public List<string> MemberIds { get; set; }
    }
}
