using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Membership
{
    public class PlayerDto
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        [MinLength(ModelConstants.UserConst.MinNameLength), MaxLength(ModelConstants.UserConst.MaxNameLength)]
        public string Name { get; set; } = null!;
    }
}
