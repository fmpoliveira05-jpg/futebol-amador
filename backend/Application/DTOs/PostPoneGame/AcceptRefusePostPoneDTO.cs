using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.PostPoneGame
{
    public class AcceptRefusePostPoneDTO
    {
        [Required(ErrorMessage = "O id da partida tem de estar preenchido")]
        public Guid IdMatch { get; set; }

        [Required(ErrorMessage = "Tem de estar especificado o estado do PostPone")]
        public StatusPostPone StatusPostPone { get; set; }

        [Required(ErrorMessage = "O id da equipa tem de estar preenchido")]
        public Guid IdTeam { get; set; }

        [Required(ErrorMessage = "O id do opponente tem de estar preenchido")]
        public Guid IdOpponent { get; set; }
    }
}
