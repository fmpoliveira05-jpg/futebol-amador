using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;

namespace Application.Interfaces.Services
{
    public interface IMatchService
    {
        public Task<List<InfoMatchCalendar>> GetCalendar(string userId, Guid idTeam);
        public Task<List<InfoMatchCalendar>> GetCalendarWithFilters(string userId, Guid idTeam, FilterCalendarDto filter);
        public Task<InfoPostPoneMatch> PostPoneMatch(string userId, Guid idTeam, PostPoneMatchDto dto);
        public Task<MatchDto> AcceptPostPoneMatch(string userId, Guid idTeam, AcceptRefusePostPoneDto dto);
        public Task RejectPostPoneMatch(string userId, Guid idTeam, AcceptRefusePostPoneDto dto);
        public Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeam(string userId, Guid idTeam);
        public Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeamWithFilters(string userId, Guid idTeam, FilterPostPoneMatchDto filter);
        public Task CancelMatch(string userId, Guid idTeam, Guid idMatch, string description);
    }
}
