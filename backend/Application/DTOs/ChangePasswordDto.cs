using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    /// <summary>
    /// Pedido de alteração da palavra-passe (<c>PUT /api/User/password</c>). As palavras-passe vão
    /// no corpo e nunca no URL, onde ficariam em logs de servidores e proxies.
    /// </summary>
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = null!;

        [Required]
        [MinLength(6), MaxLength(32)]
        public string NewPassword { get; set; } = null!;
    }
}
