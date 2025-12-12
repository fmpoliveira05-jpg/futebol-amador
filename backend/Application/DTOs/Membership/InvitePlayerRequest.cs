using System.ComponentModel.DataAnnotations;
namespace Application.DTOs.Membership
{
    public class InvitePlayerRequest
    {
        [Required]
        public string PlayerId { get; set; } = null!;
    }
}
