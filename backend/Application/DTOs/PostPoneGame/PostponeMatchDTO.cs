using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.PostPoneGame
{
    public class PostPoneMatchDto
    {
        [Required(ErrorMessage = "O id da partida tem de estar preenchido")]
        public Guid IdMatch { get; set; }

        [Required(ErrorMessage = "A data de adiamento não pode ser nula")]
        public DateTime PostPoneDate { get; set; }

        [Required(ErrorMessage = "O id da equipa tem de estar preenchido")]
        public Guid IdTeam { get; set; }

        [Required(ErrorMessage = "O id do opponente tem de estar preenchido")]
        public Guid IdOpponent { get; set; }
    }
}
