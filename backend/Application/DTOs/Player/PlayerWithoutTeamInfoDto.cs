using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Player
{
    public class PlayerWithoutTeamInfoDto
    {
        [Required]
        public string PlayerId { get; set; } = null!;

        [Required]
        [MinLength(ModelConstants.UserConst.MinNameLength), MaxLength(ModelConstants.UserConst.MaxNameLength)]
        public string Name { get; set; } = null!;

        [Required]
        [Range(ModelConstants.UserConst.MinAge, ModelConstants.UserConst.MaxAge)]
        public int Age { get; set; }

        /// <summary>
        /// Só a zona (localidade) da morada — ver <see cref="Zona.DaMorada"/>. A morada completa
        /// nunca sai nas listas de jogadores.
        /// </summary>
        public string Address { get; set; } = null!;

        [Required]
        public Position Position { get; set; }

        [Required]
        [Range(ModelConstants.PlayerConst.MinHeight, ModelConstants.PlayerConst.MaxHeight)]
        public int Height { get; set; }
    }
}
