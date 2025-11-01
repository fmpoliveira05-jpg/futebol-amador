using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.PostPoneGame
{
    public class InfoPostPoneMatch
    {
        [Required(ErrorMessage = "O id da partida tem de estar preenchido")]
        public Guid IdMatch { get; set; }

        [Required(ErrorMessage = "A data de adiamento não pode ser nula")]
        public DateTime GameDate { get; set; }

        [Required(ErrorMessage = "A data de adiamento não pode ser nula")]
        public DateTime PostPoneDate { get; set; }

        [Required(ErrorMessage = "O id da equipa tem de estar preenchido")]
        public Guid IdTeam { get; set; }

        [Required(ErrorMessage = "O nome da equipa tem de estar preenchido")]
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string nameTeam { get; set; } = null!;

        [Required(ErrorMessage = "O id do opponente tem de estar preenchido")]
        public Guid IdOpponent { get; set; }

        [Required(ErrorMessage = "O nome do opponente tem de estar preenchido")]
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string nameOpponent { get; set; } = null!;
    }
}