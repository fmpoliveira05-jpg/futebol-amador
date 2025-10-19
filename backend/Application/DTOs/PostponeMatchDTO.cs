using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class PostponeMatchDTO
    {
        [Required(ErrorMessage = "O id da partida tem de estar preenchido")]
        public Guid IdMatch { get; set; }

        [Required(ErrorMessage = "Tem de ser especificada a nova data")]
        public DateTime MatchDate { get; set; }

        [Required(ErrorMessage = "O id da equipa tem de estar preenchido")]
        public Guid IdTeam { get; set; }

        [Required(ErrorMessage = "O id do opponente tem de estar preenchido")]
        public Guid IdOpponent { get; set; }
    }
}
