using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>Jogador colocado no mercado pela sua equipa. Um jogador só pode estar listado uma vez.</summary>
    public class TransferListing
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(128)]
        [ForeignKey(nameof(Player))]
        public string PlayerId { get; set; } = null!;

        public Player Player { get; set; } = null!;

        [ForeignKey(nameof(Team))]
        public Guid IdTeam { get; set; }

        public Team Team { get; set; } = null!;

        public DateTime ListedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Proposta de uma equipa por um jogador de outra equipa (sem dinheiro).</summary>
    public class TransferOffer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(128)]
        [ForeignKey(nameof(Player))]
        public string PlayerId { get; set; } = null!;

        public Player Player { get; set; } = null!;

        /// <summary>Equipa atual do jogador (a que o cede).</summary>
        [ForeignKey(nameof(FromTeam))]
        public Guid IdFromTeam { get; set; }

        public Team FromTeam { get; set; } = null!;

        /// <summary>Equipa que faz a proposta.</summary>
        [ForeignKey(nameof(ToTeam))]
        public Guid IdToTeam { get; set; }

        public Team ToTeam { get; set; } = null!;

        public TransferOfferStatus Status { get; set; } = TransferOfferStatus.PENDING_CLUB;

        [MaxLength(250)]
        public string? Message { get; set; }

        /// <summary>A proposta foi feita a um jogador listado no mercado (o clube já tinha concordado).</summary>
        public bool ViaListing { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DecidedAt { get; set; }
    }

    /// <summary>Entrada no histórico de transferências de um jogador.</summary>
    public class TransferRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(128)]
        public string PlayerId { get; set; } = null!;

        public Guid? IdFromTeam { get; set; }

        [MaxLength(50)]
        public string? FromTeamName { get; set; }

        public Guid? IdToTeam { get; set; }

        [MaxLength(50)]
        public string? ToTeamName { get; set; }

        public TransferKind Kind { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
