using Domain.Constants;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Player
{
    public class PlayerDto
    {
        public Guid PlayerId { get; set; }

        public string PlayerName { get; set; }

        [Range(ModelConstants.PlayerConst.MinHeight, ModelConstants.PlayerConst.MaxHeight, ErrorMessage = "Um jogador deve ter entre {0} e {1} centimetors")]
        public int Height { get; set; }

        public Guid? idTeam { get; set; }

        public Position Position { get; set; }

        public bool? IsAdmin { get; set; }
    }
}
