using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class MatchDetailsRepository : IMatchDetailsRepository
    {
        private readonly AmateurFootballContext db;

        public MatchDetailsRepository(AmateurFootballContext db)
        {
            this.db = db;
        }

        public Task<Matches?> GetMatchWithTeamsAsync(Guid matchId) =>
            db.Match
                .Include(m => m.Teams).ThenInclude(ts => ts.Team).ThenInclude(t => t.Members)
                .Include(m => m.Pitch)
                .Include(m => m.Season).ThenInclude(s => s!.League)
                .FirstOrDefaultAsync(m => m.Id == matchId);

        public Task<MatchLineup?> GetLineupAsync(Guid matchId, Guid teamId) =>
            db.MatchLineup
                .Include(l => l.Slots).ThenInclude(s => s.Player)
                .FirstOrDefaultAsync(l => l.IdMatch == matchId && l.IdTeam == teamId);

        public Task<List<MatchLineup>> GetLineupsOfMatchAsync(Guid matchId) =>
            db.MatchLineup
                .Include(l => l.Slots).ThenInclude(s => s.Player)
                .Where(l => l.IdMatch == matchId)
                .ToListAsync();

        public Task<MatchLineup?> GetLastLineupAsync(Guid teamId, DateTime before) =>
            db.MatchLineup
                .Include(l => l.Slots)
                .Where(l => l.IdTeam == teamId && l.Match.MatchDate < before)
                .OrderByDescending(l => l.Match.MatchDate)
                .FirstOrDefaultAsync();

        public async Task AddLineupAsync(MatchLineup lineup) => await db.MatchLineup.AddAsync(lineup);

        public void RemoveSlots(IEnumerable<LineupSlot> slots) => db.LineupSlot.RemoveRange(slots);

        public Task<List<Matches>> GetScheduledMatchesBetweenAsync(DateTime from, DateTime until) =>
            db.Match
                .Include(m => m.Teams)
                .Where(m => m.MatchStatus == MatchStatus.SCHEDULED && m.MatchDate >= from && m.MatchDate <= until)
                .ToListAsync();

        public Task<List<MatchEvent>> GetEventsOfMatchAsync(Guid matchId) =>
            db.MatchEvent.Where(e => e.IdMatch == matchId).OrderBy(e => e.Minute).ToListAsync();

        public async Task AddEventsAsync(IEnumerable<MatchEvent> events) => await db.MatchEvent.AddRangeAsync(events);

        public void RemoveEvents(IEnumerable<MatchEvent> events) => db.MatchEvent.RemoveRange(events);

        public async Task<(List<Matches> Matches, List<MatchEvent> Events, List<LineupSlot> Slots)> GetPlayerMatchDataAsync(string playerId)
        {
            // Só leitura (perfil do jogador): sem tracking.
            var slots = await db.LineupSlot
                .AsNoTracking()
                .Include(s => s.Lineup)
                .Where(s => s.PlayerId == playerId && s.Lineup.Match.MatchStatus == MatchStatus.DONE)
                .ToListAsync();

            var eventMatchIds = await db.MatchEvent
                .Where(e => (e.PlayerId == playerId || e.RelatedPlayerId == playerId)
                            && e.Match.MatchStatus == MatchStatus.DONE)
                .Select(e => e.IdMatch)
                .Distinct()
                .ToListAsync();

            var matchIds = slots.Select(s => s.Lineup.IdMatch).Concat(eventMatchIds).Distinct().ToList();

            var matches = await db.Match
                .AsNoTracking()
                .Include(m => m.Teams).ThenInclude(ts => ts.Team)
                .Include(m => m.Season)
                .Where(m => matchIds.Contains(m.Id))
                .AsSplitQuery()
                .ToListAsync();

            var events = await db.MatchEvent.AsNoTracking().Where(e => matchIds.Contains(e.IdMatch)).ToListAsync();

            return (matches, events, slots);
        }

        public async Task<Dictionary<string, string>> GetPlayerNamesAsync(IEnumerable<string> ids)
        {
            var list = ids.Where(i => !string.IsNullOrEmpty(i)).Distinct().ToList();
            // Só o id e o nome (antes carregava os jogadores inteiros, com dados pessoais).
            return await db.Player.Where(p => list.Contains(p.Id))
                .Select(p => new { p.Id, p.Name })
                .ToDictionaryAsync(p => p.Id, p => p.Name);
        }
    }
}
