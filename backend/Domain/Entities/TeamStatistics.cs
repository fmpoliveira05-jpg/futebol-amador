using System.ComponentModel.DataAnnotations;
using Domain.Enums;
/**
 * Entidade que representa uma equipa no sistema.
 * Nota: Ver se está tudo
 *
 */
namespace Domain.Entities
{
    public class TeamStatistics
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Teams Team { get; set; } //FK

        public string IdTeam { get; set; } //FK

        public const int max_goals = 100;

        [Range(0, max_goals, ErrorMessage = "O número de golos deve estar entre 0 e 100")]
        public int num_goals;

        public MatchResult MatchResult { get; set; } 
    }
}
