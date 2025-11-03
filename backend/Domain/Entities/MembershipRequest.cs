using Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/***
 * Entidade que representa um pedido de adesão de um jogador a uma equipa ou convite de uma equipa a um jogador
 */
namespace Domain.Entities
{
    public class MembershipRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Player Player { get; set; }

        [Required]
        [ForeignKey("Player")]
        [MaxLength(ModelConstants.UserConst.MaxIdLength)]
        public string IdPlayer { get; set; } //FK

        public Team Team { get; set; }

        [Required]
        [ForeignKey("Team")]
        public Guid IdTeam { get; set; } //FK

        [Required]
        public DateTime InviteDate { get; set; }

        [Required]
        public bool IsPlayerSender { get; set; } // true - Player, false - Team

        public MembershipRequest() { }

        public MembershipRequest(Player player, Team team, bool sender)
        {
            Player = player;
            IdPlayer = player.Id;
            Team = team;
            IdTeam = team.Id;
            InviteDate = DateTime.Now;
            IsPlayerSender = sender;
        }

        public override string ToString()
        {
            return $"[MembershipRequests: Id={Id}, Player={Player}, idPlayer={IdPlayer}, Team={Team}, idTeam={IdTeam}, inviteDate={InviteDate}, sender={IsPlayerSender}]";
        }
    }
}
