namespace Application.DTOs.SuperAdmin
{
    public class UpdateSuperAdminDTO
    {
        public string Name { get; set; } = null!;

        public DateOnly DateOfBirth { get; set; }

        public string Address { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Phone { get; set; } = null!;

        /// <summary>
        /// Palavra-passe atual. Só é obrigatória quando o e-mail muda (reautenticação).
        /// </summary>
        public string? CurrentPassword { get; set; }
    }
}
