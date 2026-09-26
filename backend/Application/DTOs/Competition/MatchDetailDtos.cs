using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Competition
{
    public class FormationSlotDto
    {
        public int Slot { get; set; }
        public string PositionCode { get; set; } = null!;
        public Position Role { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class FormationDto
    {
        public string Code { get; set; } = null!;
        public List<FormationSlotDto> Slots { get; set; } = new();
    }

    public class LineupPlayerDto
    {
        public int? Slot { get; set; }
        public string? PositionCode { get; set; }
        public string PlayerId { get; set; } = null!;
        public string PlayerName { get; set; } = null!;
        public Position Position { get; set; }
    }

    public class LineupDto
    {
        public Guid MatchId { get; set; }
        public Guid TeamId { get; set; }
        public string? Formation { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsLocked { get; set; }
        public bool IsAutoFilled { get; set; }

        /// <summary>false enquanto a equipa não definiu o onze.</summary>
        public bool Exists { get; set; }
        public List<LineupPlayerDto> Starters { get; set; } = new();
        public List<LineupPlayerDto> Bench { get; set; } = new();
    }

    public class SaveLineupSlotDto
    {
        [Range(0, 10)]
        public int Slot { get; set; }

        [Required, MaxLength(128)]
        public string PlayerId { get; set; } = null!;
    }

    public class SaveLineupDto
    {
        [Required, MaxLength(10)]
        public string Formation { get; set; } = null!;

        [Required]
        public List<SaveLineupSlotDto> Starters { get; set; } = new();

        public List<string> Bench { get; set; } = new();
    }

    public class GoalEventDto
    {
        [MaxLength(128)]
        public string? ScorerId { get; set; }

        [MaxLength(128)]
        public string? AssistId { get; set; }

        [Range(0, 130)]
        public int? Minute { get; set; }
    }

    public class CardEventDto
    {
        [Required, MaxLength(128)]
        public string PlayerId { get; set; } = null!;

        public CardType Type { get; set; }

        [Range(0, 130)]
        public int? Minute { get; set; }
    }

    public class SubstitutionEventDto
    {
        [Required, MaxLength(128)]
        public string PlayerOutId { get; set; } = null!;

        [Required, MaxLength(128)]
        public string PlayerInId { get; set; } = null!;

        [Range(0, 130)]
        public int? Minute { get; set; }
    }

    /// <summary>Eventos de uma equipa, registados pelo seu administrador no fim do jogo.</summary>
    public class MatchEventsDto
    {
        [Range(0, 200)]
        public int? Fouls { get; set; }

        public List<GoalEventDto> Goals { get; set; } = new();

        public List<CardEventDto> Cards { get; set; } = new();

        public List<SubstitutionEventDto> Substitutions { get; set; } = new();
    }

    public class FinishMatchStateDto
    {
        public bool Submitted { get; set; }
        public bool MatchFinished { get; set; }
        public bool? ResultsCoincide { get; set; }
        public string Message { get; set; } = null!;
    }

    public class MatchEventViewDto
    {
        /// <summary>"GOAL", "YELLOW_CARD", "RED_CARD" ou "SUBSTITUTION".</summary>
        public string Type { get; set; } = null!;
        public int? Minute { get; set; }
        public string? PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public string? RelatedPlayerId { get; set; }
        public string? RelatedPlayerName { get; set; }
    }

    public class TeamReportDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = null!;
        public int? Goals { get; set; }
        public int? Fouls { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
        public int Substitutions { get; set; }
        public LineupDto? Lineup { get; set; }
        public List<MatchEventViewDto> Events { get; set; } = new();
    }

    public class MatchReportDto
    {
        public Guid MatchId { get; set; }
        public DateTime Date { get; set; }
        public MatchStatus Status { get; set; }
        public bool IsCompetitive { get; set; }
        public string? LeagueName { get; set; }
        public int? Round { get; set; }
        public string? PitchName { get; set; }
        public TeamReportDto Home { get; set; } = null!;
        public TeamReportDto Away { get; set; } = null!;
    }

    public class CalendarMarkerDto
    {
        public Guid IdMatch { get; set; }
        public DateTime Date { get; set; }

        /// <summary>"CANCELLED" ou "POSTPONED".</summary>
        public string Kind { get; set; } = null!;
        public string? Reason { get; set; }
        public string OpponentName { get; set; } = null!;
        public DateTime? NewDate { get; set; }
    }

    public class CancelRescheduleDto
    {
        /// <summary>Motivo (até 50 caracteres, o limite dos cancelamentos).</summary>
        [Required, StringLength(50, MinimumLength = 3)]
        public string Reason { get; set; } = null!;

        [Required]
        public DateTime NewDate { get; set; }
    }
}
