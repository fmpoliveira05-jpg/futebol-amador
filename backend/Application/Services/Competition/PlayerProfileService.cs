using Application.Competition;
using Application.DTOs.Competition;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services.Competition
{
    /// <summary>Perfil do jogador ao estilo do ZeroZero (ver docs/novas-funcionalidades.md, D8 e D10).</summary>
    public class PlayerProfileService : IPlayerProfileService
    {
        private readonly IPlayerRepository players;
        private readonly IMatchDetailsRepository matchDetails;
        private readonly ITransferRepository transfers;
        private readonly ITeamRepository teams;
        private readonly TimeProvider clock;

        public PlayerProfileService(IPlayerRepository players, IMatchDetailsRepository matchDetails,
            ITransferRepository transfers, ITeamRepository teams, TimeProvider clock)
        {
            this.players = players;
            this.matchDetails = matchDetails;
            this.transfers = transfers;
            this.teams = teams;
            this.clock = clock;
        }

        public async Task<PlayerProfileDto> GetProfileAsync(string playerId)
        {
            var player = await players.GetPlayerByIdAsync(playerId) ?? throw new NotFoundException("O jogador não existe.");
            var team = player.IdTeam.HasValue ? await teams.GetTeamByIdAsync(player.IdTeam.Value) : null;
            var (matches, events, slots) = await matchDetails.GetPlayerMatchDataAsync(playerId);

            var slotByMatch = slots
                .GroupBy(s => s.Lineup.IdMatch)
                .ToDictionary(g => g.Key, g => g.First());

            var playerMatches = new List<PlayerMatch>();
            foreach (var m in matches)
            {
                slotByMatch.TryGetValue(m.Id, out var slot);
                var teamId = slot?.Lineup.IdTeam
                             ?? events.FirstOrDefault(e => e.IdMatch == m.Id && (e.PlayerId == playerId || e.RelatedPlayerId == playerId))?.IdTeam;
                if (teamId == null)
                {
                    continue;
                }

                var teamEvents = events
                    .Where(e => e.IdMatch == m.Id && e.IdTeam == teamId)
                    .Select(e => new EventInfo(e.Type, e.Minute, e.PlayerId, e.RelatedPlayerId))
                    .ToList();

                playerMatches.Add(new PlayerMatch(
                    m.Id,
                    m.MatchDate,
                    m.Season?.Name ?? PlayerStatsCalculator.SportsSeason(m.MatchDate),
                    teamId.Value,
                    m.Teams.FirstOrDefault(t => t.IdTeam == teamId)?.Team?.Name ?? "",
                    slot?.IsStarter ?? false,
                    slot != null,
                    teamEvents));
            }

            var career = PlayerStatsCalculator.Career(playerId, playerMatches);
            var totals = PlayerStatsCalculator.Totals(career);
            var records = await transfers.GetRecordsAsync(playerId);
            var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

            return new PlayerProfileDto
            {
                Id = player.Id,
                Name = player.Name,
                ImageUrl = player.ImageUrl,
                DateOfBirth = player.DateOfBirth,
                Age = Age(player.DateOfBirth, today),
                Position = player.Position,
                Height = player.Height,
                Weight = player.Weight,
                PreferredFoot = player.PreferredFoot,
                Status = player.Status,
                Nationality = player.Nationality,
                CountryOfBirth = player.CountryOfBirth,
                CurrentTeam = team == null ? null : new TeamDto { IdTeam = team.Id, Name = team.Name },
                JoinedTeamAt = player.JoinedTeamAt,
                IsListed = await transfers.GetListingAsync(playerId) != null,
                Totals = new PlayerTotalsDto
                {
                    Games = totals.Games,
                    Goals = totals.Goals,
                    Assists = totals.Assists,
                    Minutes = totals.Minutes,
                    YellowCards = totals.YellowCards,
                    RedCards = totals.RedCards,
                },
                Career = career.Select(c => new CareerLineDto
                {
                    Season = c.Season,
                    TeamId = c.TeamId,
                    TeamName = c.TeamName,
                    Games = c.Games,
                    Goals = c.Goals,
                    Assists = c.Assists,
                    Minutes = c.Minutes,
                    YellowCards = c.YellowCards,
                    RedCards = c.RedCards,
                }).ToList(),
                Transfers = records.Select(r => new TransferHistoryDto
                {
                    Date = r.Date,
                    FromTeamName = r.FromTeamName,
                    ToTeamName = r.ToTeamName,
                    Kind = r.Kind switch
                    {
                        TransferKind.TRANSFER => "TRANSFERENCIA",
                        TransferKind.JOINED => "ADESAO",
                        _ => "SAIDA",
                    },
                }).ToList(),
            };
        }

        private static int Age(DateOnly birth, DateOnly today)
        {
            var age = today.Year - birth.Year;
            if (birth > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
