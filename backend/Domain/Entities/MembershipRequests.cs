
using System.ComponentModel.DataAnnotations;

/***
 * Entidade que representa um pedido de adesão de um jogador a uma equipa ou convite de uma equipa a um jogador
 */
namespace Domain.Entities
{
    public class MembershipRequests
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Player Player { get; set; }
    
        public Guid idPlayer { get; set; } //FK

        public Teams Team { get; set; }

        public Guid idTeam { get; set; } //FK

        public DateTime inviteDate { get; set; }

        public Boolean sender { get; set; } // true - Player, false - Team

        protected MembershipRequests() { }

        public MembershipRequests(Player player, Teams team, Boolean sender)
        {
            Player = player;
            idPlayer = player.Id;
            Team = team;
            idTeam = team.Id;
            inviteDate = DateTime.Now;
            this.sender = sender;
        }

        public override string ToString()
        {
            return $"[MembershipRequests: Id={Id}, Player={Player}, idPlayer={idPlayer}, Team={Team}, idTeam={idTeam}, inviteDate={inviteDate}, sender={sender}]";
        }
    }
}
