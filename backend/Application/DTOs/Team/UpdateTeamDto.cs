using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Team
{
    public class UpdateTeamDto
    {
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string? Name { get; set; }

        [MaxLength(ModelConstants.TeamConst.MaxDescriptionLength)]
        public string? Description { get; set; }

        public byte[]? icon { get; set; }

        [MinLength(ModelConstants.PitchConst.MinNameLength), MaxLength(ModelConstants.PitchConst.MaxNameLength)]
        public string? PitchName{ get; set; }

        [MinLength(ModelConstants.GeneralConst.MinAddressLength), MaxLength(ModelConstants.GeneralConst.MaxAddressLength)]
        public string? PitchLocation { get; set; }
    }
}
