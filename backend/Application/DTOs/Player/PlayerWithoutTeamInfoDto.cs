using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Player
{
    public class PlayerWithoutTeamInfoDto
    {
        [Required]
        public string PlayerId { get; set; } = null!;

        [Required]
        [MinLength(ModelConstants.UserConst.MinNameLength), MaxLength(ModelConstants.UserConst.MaxNameLength)]
        public string Name { get; set; } = null!;

        [Required]
        [Range(ModelConstants.UserConst.MinAge, ModelConstants.UserConst.MaxAge)]
        public int Age { get; set; }

        [Required]
        [MinLength(ModelConstants.GeneralConst.MinAddressLength), MaxLength(ModelConstants.GeneralConst.MaxAddressLength)]
        public string Address { get; set; } = null!;

        [Required]
        public Position Position { get; set; }

        [Required]
        [Range(ModelConstants.PlayerConst.MinHeight, ModelConstants.PlayerConst.MaxHeight)]
        public int Height { get; set; }
    }
}
