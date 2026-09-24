using Application.DTOs.Team;
using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.PlayerDTOs
{
    public class PlayerDetailsDto
    {
        [Required]
        public string PlayerId { get; set; } = null!;

        [Required]
        [MinLength(ModelConstants.UserConst.MinNameLength), MaxLength(ModelConstants.UserConst.MaxNameLength)]
        public string Name { get; set; } = null!;

        [Required]
        [MinLength(ModelConstants.UserConst.MinEmailLength), MaxLength(ModelConstants.UserConst.MaxEmailLength)]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(ModelConstants.UserConst.SizePhoneNumber), MaxLength(ModelConstants.UserConst.SizePhoneNumber)]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        [Range(ModelConstants.UserConst.MinAge, ModelConstants.UserConst.MaxAge)]
        public int Age { get; set; }

        [Required]
        [MinLength(ModelConstants.GeneralConst.MinAddressLength), MaxLength(ModelConstants.GeneralConst.MaxAddressLength)]
        public string Address { get; set; } = null!;

        [Required]
        public Position Position { get; set; }

        [Required]
        [Range(ModelConstants.PlayerConst.MinHeight, ModelConstants.PlayerConst.MaxHeight)]
        public int Height { get; set; }

        public TeamDto? Team { get; set; }

        public bool? IsAdmin { get; set; }

        /// <summary>
        /// Apaga os dados de contacto (e-mail, telefone, morada e data de nascimento) antes de o
        /// perfil ser mostrado a alguém que não é o próprio jogador nem colega de equipa.
        /// Mantém-se a idade, a posição e a altura, que são o que interessa para formar equipas.
        /// </summary>
        public void OcultarDadosPessoais()
        {
            Email = string.Empty;
            PhoneNumber = string.Empty;
            Address = string.Empty;
            DateOfBirth = default;
        }
    }
}