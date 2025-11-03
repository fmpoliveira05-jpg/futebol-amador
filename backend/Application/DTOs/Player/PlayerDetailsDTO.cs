using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.PlayerDTOs
{
    public class PlayerDetailsDto
    {
        [Required]
        public string PlayerId { get; set; }

        [Required]
        [MinLength(ModelConstants.UserConst.MinNameLength), MaxLength(ModelConstants.UserConst.MaxNameLength)]
        public string Name { get; set; } = null!;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        [MinLength(ModelConstants.GeneralConst.MinAddressLength), MaxLength(ModelConstants.GeneralConst.MaxAddressLength)]
        public string Address { get; set; } = null!;

        [Required]
        public Position Position { get; set; }

        [Required]
        [Range(ModelConstants.PlayerConst.MinHeight, ModelConstants.PlayerConst.MaxHeight)]
        public int Height { get; set; }

        public Guid? IdTeam { get; set; }

        public bool? IsAdmin { get; set; }
    }
}