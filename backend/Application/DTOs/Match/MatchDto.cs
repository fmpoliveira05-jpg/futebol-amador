using Domain.Constants;
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
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string NameTeam { get; set; } = null!;

        [Required]
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string NameOpponent { get; set; } = null!;

        [Required]
        [MinLength(ModelConstants.PitchConst.MinNameLength), MaxLength(ModelConstants.PitchConst.MaxNameLength)]
        public string NamePitch { get; set; } = null!;
    }
}
