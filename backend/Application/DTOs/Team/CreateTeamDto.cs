using Application.DTOs.Pitch;
using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Team
{
    public class CreateTeamDto
    {
        public Guid? Id { get; set; }

        [Required]
        [Length(ModelConstants.TeamConst.MinNameLength, ModelConstants.TeamConst.MaxNameLength)]
        public string Name { get; set; } = null!;

        [MaxLength(ModelConstants.TeamConst.MaxDescriptionLength)]
        public string? Description { get; set; }

        public string? icon { get; set; }

        [Required(ErrorMessage = "É necessario fornecer o campo principal da equipa.")]
        public PitchDto HomePitch { get; set; } = null!;

    }
}
