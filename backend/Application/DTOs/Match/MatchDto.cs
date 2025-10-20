using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Match
{
    public class MatchDto
    {
        [Required]
        public Guid IdMatch { get; set; }
        [Required]
        public DateTime GameDate { get; set; }
        [Required]
        public string NameTeam { get; set; }
        [Required]
        public string NameOpponent { get; set; }
        [Required]
        public string NamePitch { get; set; }
    }
}
