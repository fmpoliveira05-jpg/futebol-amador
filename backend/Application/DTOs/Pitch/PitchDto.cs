using Domain.Constants;
using System.ComponentModel.DataAnnotations;
namespace Application.DTOs.Pitch
{
    public class PitchDto
    {
        [Required]
        [MinLength(ModelConstants.PitchConst.MinNameLength), MaxLength(ModelConstants.PitchConst.MaxNameLength)]
        public string Name { get; set; } = null!;

        [Required]
        [MinLength(ModelConstants.GeneralConst.MinAddressLength), MaxLength(ModelConstants.GeneralConst.MaxAddressLength)]
        public string Address { get; set; } = null!;
    }
}
