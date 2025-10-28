using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class PostPoneMatch
    {
        [Key]
        public Guid Id { get; set; } = new Guid();

        public Teams Team { get; set; }

        [ForeignKey("Team")]
        public Guid IdTeamPostPone { get; set; }

        public Matches Match { get; set; }

        [ForeignKey("Match")]
        public Guid IdMatch { get; set; }

        public DateTime PostPoneDate { get; set; }

        public PostPoneMatch() { }
        public PostPoneMatch(Teams team, Matches match, DateTime postPoneDate) {
            this.Team = team;
            this.IdTeamPostPone = team.Id;
            this.Match = match;
            this.IdMatch = match.Id;
            this.PostPoneDate = postPoneDate;
        }
    }
}
