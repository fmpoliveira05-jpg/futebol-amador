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

        [Required]
        public MatchResult MatchResult { get; set; }

        [Required]
        public bool IsCompetitive { get; set; }

        [Required]
        public TeamStatisticsDto Team { get; set; } = null!;

        [Required]
        public TeamStatisticsDto Opponent { get; set; } = null!;

        [Required]
        public PitchDto PitchGame { get; set; } = null!;

        [Required]
        public bool IsHome { get; set; }
    }
}
