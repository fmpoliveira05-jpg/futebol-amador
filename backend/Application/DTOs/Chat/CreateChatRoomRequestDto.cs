namespace Application.DTOs.Chat
{
    public class CreateChatRoomRequestDto
    {
        public string RoomName { get; set; }
        public List<Guid> TeamIds { get; set; }
    }
}
