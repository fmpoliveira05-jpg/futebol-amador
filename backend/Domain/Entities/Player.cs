using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


/***
 * Entidade que representa um jogador no sistema.
 */
namespace Domain.Entities
{
    public class Player : User
    {
        [Required]
        public Position Position { get; set; }

        [Required]
        [Range(ModelConstants.PlayerConst.MinHeight, ModelConstants.PlayerConst.MaxHeight, ErrorMessage = "Um jogador deve ter entre {0} e {1} centimetors")]
        public int Height { get; set; }

        public Team? Team { get; set; }

        [ForeignKey("Team")]
        public Guid? IdTeam { get; set; } //FK

        [Required]
        public bool IsAdmin { get; set; }

        public DateTime? IsAdminLastChangedAt { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "O número de convites tem de ser pelo menos 0")]
        public ICollection<MembershipRequest> MembershipRequests { get; set; } = new List<MembershipRequest>();

        // EF
        public Player() { }

        public Player(string userId, string name, DateOnly dateOfBirth, string address, string email, string phoneNumber, Position position, int height)
            : base(userId, name, dateOfBirth, address, email, phoneNumber)
        {
            Position = position;
            Height = height;
            IsAdmin = false;
        }

        public override string ToString()
        {
            return base.ToString() + $", Position: {Position}, Height: {Height}cm, Team: {(Team != null ? Team.Name : "No Team")}, IsAdmin: {IsAdmin}";
        }
    }
}
