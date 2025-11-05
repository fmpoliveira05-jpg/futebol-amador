using Application.DTOs.Pitch;
using Application.DTOs.Team;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Match
{
    public class InfoMatchCalendar
    {
        [Required]
        public Guid IdMatch { get; set; }

        [Required]
        public MatchStatus MatchStatus { get; set; }

        [Required]
        public DateTime GameDate { get; set; }

        public string? Result { get; set; }

        [Required]
        public MatchResult MatchResult { get; set; }

        [Required]
        public TeamDto Team { get; set; } = null!;

        [Required]
        public TeamDto Opponent { get; set; } = null!;

        [Required]
        public PitchDto pitchGame { get; set; } = null!;
    }
}
