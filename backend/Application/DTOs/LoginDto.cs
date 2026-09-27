using Application.Validators.Atributos;
using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class LoginDto
    {
        [Required]
        [MaxLength(ModelConstants.UserConst.MaxEmailLength)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(ModelConstants.PasswordConst.MaxLength)]
        public string Password { get; set; } = null!;

        /// <summary>Campo-armadilha (honeypot) do formulário web: tem de vir vazio.</summary>
        [CampoArmadilha]
        public string? Website { get; set; }
    }
}
