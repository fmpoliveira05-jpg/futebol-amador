using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.RankMatchMaker
{
    public class EntryRankMatchMakerHub
    {
        [Required]
        public string ConnectionId { get; set; }

        [Required]
        public InfoTeamRankMatchMakerDto Team { get; set; }

        [Required]
        public DateTime EnteredAt { get; set; } = DateTime.UtcNow;
    }
}
