using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Team
{
    public class VitorySequenceTeam
    {
        [Required]
        public TeamDto Opponent { get; set; } = null!;

        [Required]
        public string Result { get; set; } = null!;

        [Required]
        public MatchResult MatchResult { get; set; }
    }
}
