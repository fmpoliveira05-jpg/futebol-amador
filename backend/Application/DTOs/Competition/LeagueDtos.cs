using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Competition
{
    public class SeasonDto
    {
        public Guid Id { get; set; }
        public Guid LeagueId { get; set; }
        public string Name { get; set; } = null!;
        public SeasonStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TeamCount { get; set; }
    }

    public class LeagueDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int Level { get; set; }
        public int PromotionSpots { get; set; }
        public int RelegationSpots { get; set; }
        public int SeasonDurationDays { get; set; }
        public string TrophyName { get; set; } = null!;
        public int TeamCount { get; set; }
        public SeasonDto? CurrentSeason { get; set; }
    }

    public class CreateLeagueDto
    {
        [Required, StringLength(60, MinimumLength = 3)]
        public string Name { get; set; } = null!;

        [Range(1, 50)]
        public int Level { get; set; }

        [Range(0, 10)]
        public int PromotionSpots { get; set; }

        [Range(0, 10)]
        public int RelegationSpots { get; set; }

        [Range(7, 366)]
        public int SeasonDurationDays { get; set; }

        [Required, StringLength(80, MinimumLength = 3)]
        public string TrophyName { get; set; } = null!;
    }

    public class CreateSeasonDto
    {
        /// <summary>Nome da época; por omissão, a época desportiva da data de início ("2026/27").</summary>
        [StringLength(20)]
        public string? Name { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
    }

    public class StartSeasonDto
    {
        /// <summary>Hora de início dos jogos sorteados ("HH:mm").</summary>
        [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "A hora deve estar no formato HH:mm.")]
        public string? KickoffTime { get; set; }
    }

    public class StandingRowDto
    {
        public int Position { get; set; }
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = null!;
        public string? Icon { get; set; }
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference { get; set; }
        public int Points { get; set; }
        public List<string> Form { get; set; } = new();
        public string? Zone { get; set; }
    }

    public class StandingsDto
    {
        public LeagueDto League { get; set; } = null!;
        public SeasonDto? Season { get; set; }
        public List<StandingRowDto> Rows { get; set; } = new();
    }

    public class FixtureMatchDto
    {
        public Guid IdMatch { get; set; }
        public DateTime Date { get; set; }
        public MatchStatus Status { get; set; }
        public Guid HomeTeamId { get; set; }
        public string HomeTeamName { get; set; } = null!;
        public Guid AwayTeamId { get; set; }
        public string AwayTeamName { get; set; } = null!;
        public int? HomeGoals { get; set; }
        public int? AwayGoals { get; set; }
    }

    public class FixtureRoundDto
    {
        public int Round { get; set; }
        public List<FixtureMatchDto> Matches { get; set; } = new();
    }

    public class TeamTitleDto
    {
        public string TrophyName { get; set; } = null!;
        public string LeagueName { get; set; } = null!;
        public int Count { get; set; }
        public List<string> Seasons { get; set; } = new();
    }
}
