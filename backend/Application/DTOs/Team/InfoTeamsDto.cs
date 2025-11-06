using Application.DTOs.Rank;
using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Team
{
    public class InfoTeamsDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string Name { get; set; } = null!;

        [MaxLength(ModelConstants.TeamConst.MaxDescriptionLength)]
        public string? Description { get; set; }

        [Required]
        [MinLength(ModelConstants.PitchConst.MinNameLength), MaxLength(ModelConstants.PitchConst.MaxNameLength)]
        public string Address { get; set; } = null!;

        [Required]
        public InfoRankDto Rank { get; set; } = null!;

        [Required]
        [Range(ModelConstants.TeamConst.MinNumberPoints, ModelConstants.TeamConst.MaxNumberPoints)]
        public int CurrentPoints { get; set; }

        [Required]
        [Range(ModelConstants.TeamConst.MinAverageAge, ModelConstants.TeamConst.MaxAverageAge)]
        public float AverageAge { get; set; }

        [Required]
        [Range(ModelConstants.TeamConst.MinMembers, ModelConstants.TeamConst.MaxMembers)]
        public int PlayerCount { get; set; }
    }
}
