using Domain.Enums;
using System.ComponentModel.DataAnnotations;

/*
 * Classe que representa um Match
 * 
 */
namespace Domain.Entities
{
    public class Matches
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public MatchStatus MatchStatus { get; set; }

        [Range(0, 2, ErrorMessage = "O número de equipas que compõem o match deve estar entre 0 e 2")]
        public int TeamsCount { get; set; }

        public ICollection<TeamStatistics> Teams { get; set; }

        public DateTime MatchDate { get; set; }

        public DateTime TimeStart { get; set; }

        public Boolean iscompetive { get; set; } //Se o match é a valer para o rank ou é so amigável

        public Pitch Pitch { get; set; }

        public Guid idPitch { get; set; } //FK
    }
}
