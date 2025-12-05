using Application.DTOs.Pitch;
using Application.DTOs.PlayerDTOs;
using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Team
{
    public class TeamDetailsDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string Name { get; set; } = null!;

        [MaxLength(ModelConstants.TeamConst.MaxDescriptionLength)]
        public string? Description { get; set; }

        [Required]
        public DateOnly FoundationDate { get; set; }
        
        [Required]
        [Range(ModelConstants.TeamConst.MinNumberPoints, ModelConstants.TeamConst.MaxNumberPoints)]
        public int TotalPoints { get; set; }

        [Required]
        [MinLength(ModelConstants.RankConts.MinNameLength), MaxLength(ModelConstants.RankConts.MaxNameLength)]
        public string RankName { get; set; } = null!;

        [Required]
        public PitchDto PitchDto { get; set; } = null!;

        [Required]
        public List<PlayerDetailsDto> Players { get; set; } = null!;
    }
}
