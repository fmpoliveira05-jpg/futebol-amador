using Application.DTOs.Pitch;
using Application.Validators.Atributos;
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

        /// <summary>Emblema: URL HTTPS do Cloudinary ou imagem embebida (ver <see cref="EmblemaPermitidoAttribute"/>).</summary>
        [EmblemaPermitido]
        public string? Icon { get; set; }

        [Required(ErrorMessage = "É necessario fornecer o campo principal da equipa.")]
        public PitchDto HomePitch { get; set; } = null!;

    }
}
