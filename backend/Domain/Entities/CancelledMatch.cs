using Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class CancelledMatch
    {
        [Key]
        public Guid Id { get; set; } = new Guid();

        [Required]
        public Team Team { get; set; }

        [Required]
        [ForeignKey("Team")]
        public Guid IdTeam { get; set; }

        public Matches Match { get; set; }

        [ForeignKey("Match")]
        public Guid IdMatch { get; set; }

        [Required]
        [StringLength(ModelConstants.CancelledMatchConst.MaxDescriptionLength, MinimumLength = ModelConstants.CancelledMatchConst.MinDescriptionLength)]
        public string Description { get; set; }

        [Required]
        public DateTime TimeCancellation { get; set; } = DateTime.UtcNow;

        public CancelledMatch() { }

        public CancelledMatch(Team team, Matches match, string description) { 
            this.Team = team;
            this.IdTeam = team.Id;
            this.Match = match;
            this.IdMatch = match.Id;
            this.Description = description;
        }

    }
}
