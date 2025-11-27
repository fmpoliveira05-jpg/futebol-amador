using Application.DTOs.Pitch;
using Application.DTOs.Team;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Match
{
    public class InfoMatch
    {
        [Required]
        public Guid IdMatch { get; set; }

        [Required]
        public DateTime GameDate { get; set; }

        [Required]
        public bool IsCompetitive { get; set; }

        [Required]
        public TeamDto Team { get; set; } = null!;


        [Required]
        public TeamDto Opponent { get; set; } = null!;

        [Required]
        public bool IsHome { get; set; }
    }
}
