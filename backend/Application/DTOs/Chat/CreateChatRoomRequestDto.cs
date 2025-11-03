using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Chat
{
    public class CreateChatRoomRequestDto
    {
        public string RoomName { get; set; }
        public List<Guid> TeamIds { get; set; }
    }
}
