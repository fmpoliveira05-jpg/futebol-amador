using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Competition
{
    public class MarketFilterDto
    {
        /// <summary>true: só jogadores com equipa; false: só livres; nulo: todos.</summary>
        public bool? HasTeam { get; set; }
        public Guid? LeagueId { get; set; }
        public string? Nationality { get; set; }
        public Position? Position { get; set; }
        public string? Name { get; set; }
        public bool? OnlyListed { get; set; }
    }

    public class MarketPlayerDto
    {
        public string PlayerId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public Position Position { get; set; }
        public string? Nationality { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? TeamId { get; set; }
        public string? TeamName { get; set; }
        public Guid? LeagueId { get; set; }
        public string? LeagueName { get; set; }
        public bool IsListed { get; set; }
    }

    public class CreateTransferOfferDto
    {
        /// <summary>Equipa que faz a proposta.</summary>
        [Required]
        public Guid TeamId { get; set; }

        [Required, MaxLength(128)]
        public string PlayerId { get; set; } = null!;

        [MaxLength(250)]
        public string? Message { get; set; }
    }

    public class TransferOfferDto
    {
        public Guid Id { get; set; }
        public string PlayerId { get; set; } = null!;
        public string PlayerName { get; set; } = null!;
        public Guid FromTeamId { get; set; }
        public string FromTeamName { get; set; } = null!;
        public Guid ToTeamId { get; set; }
        public string ToTeamName { get; set; } = null!;
        public TransferOfferStatus Status { get; set; }
        public string? Message { get; set; }
        public bool ViaListing { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DecidedAt { get; set; }
    }

    public class TeamTransferOffersDto
    {
        public List<TransferOfferDto> Received { get; set; } = new();
        public List<TransferOfferDto> Sent { get; set; } = new();
    }

    public class TransferHistoryDto
    {
        public DateTime Date { get; set; }
        public string? FromTeamName { get; set; }
        public string? ToTeamName { get; set; }

        /// <summary>"TRANSFERENCIA", "ADESAO" ou "SAIDA".</summary>
        public string Kind { get; set; } = null!;
    }
}
