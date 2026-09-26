using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>Onze inicial e suplentes de uma equipa num jogo.</summary>
    public class MatchLineup
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(Match))]
        public Guid IdMatch { get; set; }

        public Matches Match { get; set; } = null!;

        [ForeignKey(nameof(Team))]
        public Guid IdTeam { get; set; }

        public Team Team { get; set; } = null!;

        /// <summary>Tática, por exemplo "4-3-3".</summary>
        [Required, MaxLength(10)]
        public string Formation { get; set; } = null!;

        /// <summary>Preenchido pelo sistema porque o prazo passou sem onze.</summary>
        public bool IsAutoFilled { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<LineupSlot> Slots { get; set; } = new List<LineupSlot>();
    }

    /// <summary>Um jogador no onze (com a posição na tática) ou no banco.</summary>
    public class LineupSlot
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(Lineup))]
        public Guid IdLineup { get; set; }

        public MatchLineup Lineup { get; set; } = null!;

        [Required, MaxLength(128)]
        [ForeignKey(nameof(Player))]
        public string PlayerId { get; set; } = null!;

        public Player Player { get; set; } = null!;

        public bool IsStarter { get; set; }

        /// <summary>Índice da posição na tática (titulares) ou ordem no banco (suplentes).</summary>
        public int Slot { get; set; }

        /// <summary>Código da posição na tática (GR, DC, MC, PL...). Nulo para suplentes.</summary>
        [MaxLength(4)]
        public string? PositionCode { get; set; }
    }

    /// <summary>Evento de um jogo registado pelo administrador da equipa no fim do jogo.</summary>
    public class MatchEvent
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(Match))]
        public Guid IdMatch { get; set; }

        public Matches Match { get; set; } = null!;

        /// <summary>Equipa a que o evento pertence (a do jogador).</summary>
        public Guid IdTeam { get; set; }

        public MatchEventType Type { get; set; }

        [Range(0, 130)]
        public int? Minute { get; set; }

        /// <summary>Marcador, jogador com cartão ou jogador que sai.</summary>
        [MaxLength(128)]
        public string? PlayerId { get; set; }

        /// <summary>Assistência (golo) ou jogador que entra (substituição).</summary>
        [MaxLength(128)]
        public string? RelatedPlayerId { get; set; }
    }
}
