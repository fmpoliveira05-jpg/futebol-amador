using Application.DTOs.Team;
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

        [Required]
        public TeamDto Team { get; set; } = null!;

        [Required]
        public TeamDto Opponent { get; set; } = null!;
    }
}