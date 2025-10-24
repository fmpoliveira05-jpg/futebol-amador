using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class ResultMatchDto
    {
        [Required]
        public Guid IdMatch {  get; set; }

        [Required]
        public Guid IdTeam { get; set; }

        [Required]
        public int MyTeamGoals { get; set; }

        [Required]
        public Guid IdOpponent { get; set; }
        
        [Required]
        public int OpponentGoals { get; set; }
    }
}
