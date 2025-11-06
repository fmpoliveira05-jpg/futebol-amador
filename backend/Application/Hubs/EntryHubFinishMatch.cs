using System.ComponentModel.DataAnnotations;

namespace Application.Hubs
{
    public class EntryHubFinishMatch
    {
        [Required]
        public string ConnectionId;

        [Required]
        public JoinFinishMatch Result;
    }
}
