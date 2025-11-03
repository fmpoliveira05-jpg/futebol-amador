using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Desnecessario
namespace Application.DTOs.Chat
{
    public class ChatRoomDto
    {
        public string RoomId { get; set; }
        public string RoomName { get; set; }
        public List<string> MemberIds { get; set; } // Use string, pois é o que está na DB
    }
}
