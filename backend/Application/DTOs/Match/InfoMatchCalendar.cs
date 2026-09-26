using Application.DTOs.Pitch;
using Application.DTOs.Team;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Match
{
    public class InfoMatchCalendar
    {
        [Required]
        public Guid IdMatch { get; set; }

        [Required]
        public MatchStatus MatchStatus { get; set; }

        [Required]
        public DateTime GameDate { get; set; }

        [Required]
        public MatchResult MatchResult { get; set; }

        [Required]
        public bool IsCompetitive { get; set; }

        [Required]
        public TeamStatisticsDto Team { get; set; } = null!;

        [Required]
        public TeamStatisticsDto Opponent { get; set; } = null!;

        [Required]
        public PitchDto PitchGame { get; set; } = null!;

        [Required]
        public bool IsHome { get; set; }

        /// <summary>Equipa da casa (nos jogos antigos, a dona do campo).</summary>
        public TeamStatisticsDto? HomeTeam { get; set; }

        public TeamStatisticsDto? AwayTeam { get; set; }

        /// <summary>Liga do jogo; nulo nos amigáveis.</summary>
        public string? LeagueName { get; set; }

        public int? Round { get; set; }

        /// <summary>Motivo do cancelamento, do pedido de adiamento pendente ou do último adiamento aceite.</summary>
        public string? Reason { get; set; }

        /// <summary>Data original, se o jogo foi adiado por acordo.</summary>
        public DateTime? PostponedFrom { get; set; }
    }
}
