using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Player
{
    public class InfoPlayerDto
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Só a zona (localidade) da morada — ver <see cref="Zona.DaMorada"/>. A morada completa
        /// nunca sai nas listas de jogadores.
        /// </summary>
        public string Address { get; set; } = null!;

        [Required]
        [Range(ModelConstants.UserConst.MinAge, ModelConstants.UserConst.MaxAge)]
        public int Age { get; set; } 

        [Required]
        public Position Position { get; set; }

        [Required]
        public int Heigth { get; set; }

        [Required]
        public bool HaveTeam { get; set; }
    }
}
