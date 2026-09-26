using Application.DTOs.Team;
using Domain.Enums;

namespace Application.DTOs.Competition
{
    public class PlayerTotalsDto
    {
        public int Games { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int Minutes { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
    }

    public class CareerLineDto : PlayerTotalsDto
    {
        public string Season { get; set; } = null!;
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = null!;
    }

    public class PlayerProfileDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public int Age { get; set; }
        public Position Position { get; set; }
        public int Height { get; set; }
        public int? Weight { get; set; }
        public PreferredFoot? PreferredFoot { get; set; }
        public PlayerStatus Status { get; set; }
        public string? Nationality { get; set; }
        public string? CountryOfBirth { get; set; }
        public TeamDto? CurrentTeam { get; set; }
        public DateTime? JoinedTeamAt { get; set; }
        public bool IsListed { get; set; }
        public PlayerTotalsDto Totals { get; set; } = new();
        public List<CareerLineDto> Career { get; set; } = new();
        public List<TransferHistoryDto> Transfers { get; set; } = new();
    }
}
