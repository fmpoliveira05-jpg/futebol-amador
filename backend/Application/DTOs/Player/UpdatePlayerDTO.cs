using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.PlayerDTOs
{
    /// <summary>
    /// Dados editáveis do perfil. Não inclui o id do jogador: o controlador usa o id do token,
    /// para ninguém conseguir editar o perfil de outra pessoa.
    /// </summary>
    public class UpdatePlayerDto
    {
        [Required]
        [MinLength(ModelConstants.UserConst.MinNameLength), MaxLength(ModelConstants.UserConst.MaxNameLength)]
        public string Name { get; set; } = null!;

        public DateOnly DateOfBirth { get; set; }

        [MinLength(ModelConstants.GeneralConst.MinAddressLength), MaxLength(ModelConstants.GeneralConst.MaxAddressLength)]
        public string Address { get; set; } = null!;

        [MinLength(ModelConstants.UserConst.MinEmailLength), MaxLength(ModelConstants.UserConst.MaxEmailLength)]
        public string Email { get; set; } = null!;

        [MinLength(ModelConstants.UserConst.SizePhoneNumber), MaxLength(ModelConstants.UserConst.SizePhoneNumber)]
        public string Phone { get; set; } = null!;

        public Position Position { get; set; }

        [Range(ModelConstants.PlayerConst.MinHeight, ModelConstants.PlayerConst.MaxHeight)]
        public int Height { get; set; }
    }
}
