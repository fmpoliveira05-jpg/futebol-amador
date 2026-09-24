using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.RankMatchMaker
{
    public class EntryRankMatchMakerHub
    {
        [Required]
        public string ConnectionId { get; set; } = null!;

        [Required]
        public InfoTeamRankMatchMakerDto Team { get; set; } = null!;
    }
}
