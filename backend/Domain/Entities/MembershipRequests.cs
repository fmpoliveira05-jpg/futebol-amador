using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [ForeignKey("Player")]
        public Guid IdPlayer { get; set; } //FK

        public Teams Team { get; set; }

        [ForeignKey("Team")]
        public Guid IdTeam { get; set; } //FK

        // Data do convite/pedido (usar UTC para consistência)
        public DateTime InviteDate { get; set; }

        // true  => enviado pelo Player (player pediu entrar na team)
        // false => enviado pela Team  (team convidou o player)
        public bool Sender { get; set; }

        protected MembershipRequests() { }

        public MembershipRequests(Player player, Teams team, bool sender)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }

            Player = player;
            IdPlayer = player.Id;
            Team = team;
            IdTeam = team.Id;
            InviteDate = DateTime.UtcNow;
            Sender = sender;
        }

        public override string ToString()
        {
            return $"[MembershipRequests: Id={Id}, PlayerId={IdPlayer}, TeamId={IdTeam}, InviteDate={InviteDate:u}, Sender={(Sender ? "Player" : "Team")}]";
        }
    }
}