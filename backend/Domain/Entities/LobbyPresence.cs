using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class LobbyPresence
    {
        [Key]
        public Guid Id { get; set; } = new Guid();

        [ForeignKey("Matches")]
        public Guid MatchId { get; set; }
        public Matches Matches { get; set; }

        [ForeignKey("Teams")]
        public Guid TeamId { get; set; }

        public Teams Teams { get; set; }

        [ForeignKey("Users")]
        public Guid UserId { get; set; }

        public Users Users { get; set; }
        public string ConnectionId { get; set; }
        public DateTime JoinedAt { get; set; }

        public LobbyPresence() { }

        public LobbyPresence(Guid matchId, Guid idUser, Guid teamId, string ConnectionID) {
            this.MatchId = matchId;
            this.UserId = idUser;
            this.TeamId = teamId;
            this.ConnectionId = ConnectionID;
            this.JoinedAt = DateTime.UtcNow;
        }
    }
}
