using Application.Validators.Atributos;
using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.PlayerDTOs
{
    public class CreatePlayerDto
    {
        [Required]
        [MinLength(ModelConstants.UserConst.MinNameLength), MaxLength(ModelConstants.UserConst.MaxNameLength)]
        public string Name { get; set; } = null!;
        
        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        [MinLength(ModelConstants.GeneralConst.MinAddressLength), MaxLength(ModelConstants.GeneralConst.MaxAddressLength)]
        public string Address { get; set; } = null!;

        [Required]
        [MinLength(ModelConstants.UserConst.MinEmailLength), MaxLength(ModelConstants.UserConst.MaxEmailLength)]
        public string Email { get; set; } = null!;

        [Required]
        [PalavraPasseSegura]
        public string Password { get; set; } = null!;

        [Required]
        public string Phone {  get; set; } = null!;

        [Required]
        public Position Position { get; set; }

        [Required]
        [Range(ModelConstants.PlayerConst.MinHeight, ModelConstants.PlayerConst.MaxHeight)]
        public int Height { get; set; }

        /// <summary>
        /// Campo-armadilha (honeypot): o formulário web esconde-o, por isso só um robô o preenche.
        /// Um pedido com este campo preenchido é recusado.
        /// </summary>
        [CampoArmadilha]
        public string? Website { get; set; }
    }
}
