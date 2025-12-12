
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Membership
{
    public class InviteTeamRequest
    {
        [Required]
        public Guid TeamId { get; set; }
    }
}
