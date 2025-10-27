using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Chat
{
    public class CreateChatRoomDto
    {
        public string RoomName { get; set; }
        public List<Guid> ParticipantIds { get; set; }
        public string ChatRoomId { get; set; }
    }
}
