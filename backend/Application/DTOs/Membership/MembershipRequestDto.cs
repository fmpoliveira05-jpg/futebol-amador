using Application.DTOs.Membership;
using Application.DTOs.Team;
using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.MemberShip
{
    public class MemberShipRequestDto
    {
        [Required]
        public Guid RequestId { get; set; }

        [Required]
        public PlayerDto Player { get; set; } = null!;
        
        [Required]
        public TeamDto Team { get; set; } = null!;

        [Required]
        public DateTime RequestDate { get; set; }

        [Required]
        public bool IsPlayerSender { get; set; }

    }
}
