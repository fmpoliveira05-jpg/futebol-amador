using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Membership
{
    public class SendMembershipRequestDTO
    {
        [Required]
        public Guid IdPlayer { get; set; }
        
        [Required]
        public Guid IdTeam { get; set; }

        public bool? Sender { get; set; }
    }
}
