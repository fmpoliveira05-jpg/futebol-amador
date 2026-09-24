using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class LoginResponseDto
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public string Address { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Phone { get; set; } = null!;

        [Required]
        public DateTime CreationDate { get; set; }

        public DateTime? IsAdminLastChanged { get; set; }
        public Position? Position { get; set; }

        public int? Height { get; set; }

        public Guid? IdTeam { get; set; }

        public bool? IsAdmin { get; set; }

        [Required]
        public FirebaseLoginResponseDto FirebaseLoginResponseDto { get; set; } = null!;
    }
}
