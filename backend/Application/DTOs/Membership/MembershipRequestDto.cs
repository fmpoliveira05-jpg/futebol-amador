using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.MemberShip
{
    public class MemberShipRequestDto
    {
        [Required]
        public Guid RequestId { get; set; }

        [Required]
        [MinLength(ModelConstants.UserConst.MinNameLength), MaxLength(ModelConstants.UserConst.MaxNameLength)]
        public string PlayerName { get; set; } = null!;

        [Required]
        public string PlayerId { get; set; }

        [Required]
        public Guid TeamId { get; set; }

        [Required]
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string TeamName { get; set; } = null!;

        [Required]
        public DateTime RequestDate { get; set; }

        [Required]
        public bool IsPlayerSender { get; set; }

    }
}
