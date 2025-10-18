using Domain.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
DTOs são Objetos usados para comunicação entre API e frontends, evitando expor 
entidades do domínio diretamente
 * */
namespace Application.DTOs.Team
{
    public class CreateTeamDto
    {    
        [MaxLength(ModelConstants.Team.MaxNameLength), Required(ErrorMessage = "O nome do time é obrigatorio.")]
        public string Name { get; set; }

        [MaxLength(ModelConstants.Team.MaxDescriptionLength)]
        public string Description { get; set; }

        //IMPORTANTE: definir depois o tamanho maximo da imagem
        public byte[]? icon { get; set; }

        [Required(ErrorMessage = "É necessario fornecer o campo principal da equipa.")]
        public PitchDto HomePitch { get; set; }

        public CreateTeamDto()
        {
        }
        public CreateTeamDto(string name, string description, byte[]? icon, PitchDto homePitch)
        {
            Name = name;
            Description = description;
            this.icon = icon;
            HomePitch = homePitch;
        }
    }
}
