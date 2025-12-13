using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Membership
{
    public class RequestMemberShip
    {
        [Required]
        public Guid RequestId { get; set; }
    }
}
