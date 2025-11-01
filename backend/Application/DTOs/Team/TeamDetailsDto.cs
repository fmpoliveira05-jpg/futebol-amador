using Application.DTOs.PlayerDTOs;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Team
{
    public class TeamDetailsDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;
        
        public string? Description { get; set; }

        [Required]
        public DateTime FoundationDate { get; set; }
        
        [Required]
        public int TotalPoints { get; set; }

        [Required]
        public string RankName { get; set; } = null!;

        [Required]
        public string PitchDto { get; set; } = null!;

        [Required]
        public List<PlayerDetailsDto> Players { get; set; } = null!;
    }
}
