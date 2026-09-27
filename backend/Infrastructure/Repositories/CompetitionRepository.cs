using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CompetitionRepository : ICompetitionRepository
    {
        private readonly AmateurFootballContext db;

        public CompetitionRepository(AmateurFootballContext db)
        {
            this.db = db;
        }

        public Task<List<League>> GetLeaguesAsync() =>
            db.League.Include(l => l.Seasons).OrderBy(l => l.Level).ToListAsync();

        public async Task<Dictionary<Guid, int>> CountTeamsByLeagueAsync() =>
            await db.Team.Where(t => t.IdLeague != null)
                .GroupBy(t => t.IdLeague!.Value)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

        public Task<League?> GetLeagueAsync(Guid id) =>
            db.League.Include(l => l.Seasons).FirstOrDefaultAsync(l => l.Id == id);

        public Task<League?> GetTopLeagueAsync() =>
            db.League.Include(l => l.Seasons).OrderBy(l => l.Level).FirstOrDefaultAsync();

        public Task<bool> LevelExistsAsync(int level) => db.League.AnyAsync(l => l.Level == level);

        public Task<League?> GetAdjacentLeagueAsync(int level, bool above) => above
            ? db.League.Where(l => l.Level < level).OrderByDescending(l => l.Level).FirstOrDefaultAsync()
            : db.League.Where(l => l.Level > level).OrderBy(l => l.Level).FirstOrDefaultAsync();

        public async Task AddLeagueAsync(League league) => await db.League.AddAsync(league);

        public async Task AddSeasonAsync(Season season) => await db.Season.AddAsync(season);

        public async Task AddSeasonTeamAsync(SeasonTeam seasonTeam) => await db.SeasonTeam.AddAsync(seasonTeam);

        public Task<Season?> GetSeasonAsync(Guid id) =>
            db.Season
                .Include(s => s.League)
                .Include(s => s.Teams).ThenInclude(st => st.Team).ThenInclude(t => t.Pitch)
                .FirstOrDefaultAsync(s => s.Id == id);

        public Task<List<Season>> GetSeasonsOfLeagueAsync(Guid leagueId) =>
            db.Season.Include(s => s.Teams)
                .Where(s => s.IdLeague == leagueId)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

        public Task<Season?> GetOpenSeasonAsync(Guid leagueId) =>
            db.Season
                .Include(s => s.League)
                .Include(s => s.Teams)
                .Where(s => s.IdLeague == leagueId && s.Status != SeasonStatus.FINISHED)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

        public Task<List<Season>> GetSeasonsToStartAsync(DateTime until) =>
            db.Season.Where(s => s.Status == SeasonStatus.REGISTRATION && s.StartDate <= until).ToListAsync();

        public Task<List<Season>> GetSeasonsToCloseAsync(DateTime now) =>
            db.Season.Where(s => s.Status == SeasonStatus.IN_PROGRESS && s.EndDate <= now).ToListAsync();

        public Task<List<Matches>> GetSeasonMatchesAsync(Guid seasonId) =>
            db.Match
                .Include(m => m.Teams).ThenInclude(ts => ts.Team)
                .Where(m => m.IdSeason == seasonId)
                .OrderBy(m => m.Round).ThenBy(m => m.MatchDate)
                .ToListAsync();

        public Task<List<Team>> GetTeamsOfLeagueAsync(Guid leagueId) =>
            db.Team.Include(t => t.Pitch).Where(t => t.IdLeague == leagueId).ToListAsync();

        public Task<Team?> GetTeamWithPitchAsync(Guid teamId) =>
            db.Team.Include(t => t.Pitch).FirstOrDefaultAsync(t => t.Id == teamId);

        public Task<List<SeasonTeam>> GetOpenRegistrationsOfTeamAsync(Guid teamId) =>
            db.SeasonTeam.Include(st => st.Season)
                .Where(st => st.IdTeam == teamId && st.Season.Status != SeasonStatus.FINISHED)
                .ToListAsync();

        public void RemoveSeasonTeam(SeasonTeam seasonTeam) => db.SeasonTeam.Remove(seasonTeam);

        public async Task AddTitleAsync(TeamTitle title) => await db.TeamTitle.AddAsync(title);

        public Task<List<TeamTitle>> GetTitlesAsync(Guid teamId) =>
            db.TeamTitle.AsNoTracking().Where(t => t.IdTeam == teamId).OrderBy(t => t.WonAt).ToListAsync();

        public async Task<bool> IsSuperAdminAsync(string? userId) =>
            !string.IsNullOrEmpty(userId) && await db.SuperAdmin.AnyAsync(s => s.Id == userId);

        public async Task AddMatchesAsync(IEnumerable<Matches> matches) => await db.Match.AddRangeAsync(matches);
    }
}
