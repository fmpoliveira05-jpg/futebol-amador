using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IMatchRepository
    {
        public Task AddMatch(Matches match);
        public Task<Matches?> GetMatchById(Guid idMatch);
        public Task<Matches?> GetScheduledMatchById(Guid idMatch);
        public Task<Matches?> GetMatchInProgressByIdAsync(Guid idMatch);
        public Task<Matches?> GetMatchToCancelById(Guid idMatch);
        public Task<Matches?> GetMatchWitchPitchById(Guid idMatch);
        public Task<Matches?> GetMatchWithListPlayerById(Guid idMatch);
        public Task<Matches?> GetMatchProxim12HoursMatchs(Guid idReceiver, DateTime gameDate);
        public Task<List<InfoMatchCalendar>> GetAllMatchesTeam(Guid idTeam);
        public Task<List<InfoMatchCalendar>> GetAllMatchesTeamWithFilters(Guid idTeam, FilterCalendar filter);
        public Task<List<InfoPostPoneMatch>> GetAllMatchPostPoneReceiverById(Guid idReceiver);
    }
}
