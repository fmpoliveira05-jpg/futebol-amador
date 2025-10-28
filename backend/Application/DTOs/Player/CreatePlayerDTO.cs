using Domain.Entities;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.PlayerDTOs
{
    public class CreatePlayerDTO
    {
        [Required]
        public string Name {  get; set; }
        
        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Phone {  get; set; }

        [Required]
        public Position Position { get; set; }

        [Required]
        public int Height { get; set; }
    }
}
