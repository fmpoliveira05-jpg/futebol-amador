using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class CancelledMatch
    {
        [Key]
        public Guid Id { get; set; } = new Guid();

        [Required]
        public Teams Team { get; set; }


        [ForeignKey("Team")]
        public Guid IdTeam { get; set; }
        
        [Required]
        public Matches Match { get; set; }

        [ForeignKey("Match")]
        public Guid IdMatch { get; set; }

        [MinLength(1), MaxLength(250)]
        public string Description { get; set; }

        public DateTime TimeCancellation { get; set; } = DateTime.UtcNow;

        public CancelledMatch() { }

        public CancelledMatch(Teams team, Matches match, string description) { 
            this.Team = team;
            this.IdTeam = team.Id;
            this.Match = match;
            this.IdMatch = match.Id;
            this.Description = description;
        }

    }
}
