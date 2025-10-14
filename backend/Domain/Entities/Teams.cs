using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/**
 * Entidade que representa uma equipa no sistema.
 * Nota: Ver se está tudo
 *
 */
namespace Domain.Entities
{
    public class Teams
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public byte[]? Icon { get; set; }

        public Pitch Pitch { get; set; }

        public Guid IdPitch { get; set; } //FK

        public DateTime DataFoundation { get; set; }

        public const int MaxPlayers= 32; //Validar se é mesmo 32

        public const int MaxAdmins = 4;

        [Range(1, MaxPlayers, ErrorMessage = "O número minimo de players é 1 de máximo 32")]
        public int MemberCount { get; set; }

        [Range(1, MaxAdmins, ErrorMessage = "O número minimo de admins é 1 de máximo 4")]
        public int AdminCount { get; set; }

        public ICollection<Player> Members { get; set; } = new List<Player>();


        [Range(18, 70, ErrorMessage = "A idade média deve estar entre os 18 e 70 anos")]
        public float AverageAge { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Um equipa tem 0 ou mais pontos")]
        public int CurrentPoints { get; set; }

        public Rank? Rank { get; set; }

        public Guid IdRank { get; set; } //FK

        [Range(0, int.MaxValue, ErrorMessage = "Um equipa tem 0 ou pedidos de adesão")]
        public int CountMemvberShipsRequests { get; set; }

        public ICollection<MembershipRequests> MembershipRequests { get; set; } = new List<MembershipRequests>();

        [Range(0, int.MaxValue, ErrorMessage = "Um equipa tem 0 ou mais convites de partida")]
        public int CountMatchesInvites { get; set; }

        [InverseProperty("Sender")]
        public ICollection<MatchInvite> SentInvites { get; set; } = new List<MatchInvite>();

        [InverseProperty("Receiver")]
        public ICollection<MatchInvite> ReceivedInvites { get; set; } = new List<MatchInvite>();

        public Calendar Calendar { get; set; }

        public Guid IdCalendar { get; set; } //FK
    }
}
