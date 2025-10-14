using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    internal class Chat
    {
        [Key]
        [MinLength = 32, MaxLength = 36]
        public string Id { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
