using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

/**
 * Entidade que representa um utilizador no sistema.
 * 
 * !!!Falta criar validação para a dateOfBirthday, para só receber datas de 18 até 70
 * Talvez Trocasr MaxLength por StringLength
 * !!!Falta verificação para a password.
 */
namespace Domain.Entities
{
    internal abstract class Users
    {
        [Key]
        [MinLength = 32, MaxLength = 36]
        public string Id { get; set; }

        [MaxLength = 50]
        public string Name { get; set; }

        public DateOnly DateOfBirth { get; set; }

        [MaxLength = 250]
        public string address { get; set; }

        [MaxLength = 50]
        [EmailAddress(ErrorMessage = "O campo Email não está em um formato válido.")] // Adicione este
        public string Email { get; set; }

        [MinLength = 8, MaxLength = 16]
        public string Password { get; set; }

        [Range(100000000, 999999999, ErrorMessage = "O número de telefone deve ter 9 dígitos.")]
        public int PhoneNumber { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
