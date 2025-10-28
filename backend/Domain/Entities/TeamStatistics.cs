using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Constants;
using Domain.Enums;

/***
 * Entidade que representa as estatísticas de uma equipa durante uma Partida
 */
namespace Domain.Entities
{
    public class TeamStatistics
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Teams Team { get; set; } //FK

        [ForeignKey("Team")]
        public Guid IdTeam { get; set; } //FK

        [Range(0, ModelConstants.GeneralConst.MaxGoals, ErrorMessage = "O número de golos deve estar entre 0 e 100")]
        public int NumGoals { get; set; } = 0;

        [ForeignKey("Match")]
        public Guid MatchesId { get; set; }

        public Matches Match { get; set; }
        public MatchResult MatchResult { get; set; } = MatchResult.UNPLAYED;

        // EF
        protected TeamStatistics() { }

        public TeamStatistics(Teams team)
        {
            this.Team = team;
        }
    }
}
