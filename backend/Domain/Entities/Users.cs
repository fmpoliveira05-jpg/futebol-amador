using System.ComponentModel.DataAnnotations;

/**
 * Entidade que representa um utilizador no sistema.
 * 
 * !!!Falta criar validação para a dateOfBirthday, para só receber datas de 18 até 70
 * Talvez Trocasr MaxLength por StringLength
 * !!!Falta verificação para a password.
 */
namespace Domain.Entities
{
    public abstract class Users
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string Name { get; set; }

        public DateOnly DateOfBirth { get; set; }

        [MaxLength(250)]
        public string address { get; set; }

        [MaxLength(50)]
        [EmailAddress(ErrorMessage = "O campo Email não está em um formato válido.")] // Adicione este
        public string Email { get; set; }

        [MinLength(8), MaxLength(16)]
        public string Password { get; set; }

        [Range(100000000, 999999999, ErrorMessage = "O número de telefone deve ter 9 dígitos.")]
        public int PhoneNumber { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
