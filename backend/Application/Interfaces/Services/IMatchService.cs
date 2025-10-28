using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;

namespace Application.Interfaces.Services
{
    public interface IMatchService
    {
        public Task<List<InfoMatchCalendar>> GetCalendar(Guid idTeam);
        public Task<List<InfoMatchCalendar>> GetCalendarWithFilters(Guid idTeam, FilterCalendar filter);
        public Task<InfoPostPoneMatch> PostPoneMatch(PostponeMatchDTO dto);
        public Task<MatchDto> AcceptPostPoneMatch(Guid idTeamUrl, AcceptRefusePostPoneDTO dto);
        public Task RejectPostPoneMatch(Guid idTeamUrl, AcceptRefusePostPoneDTO dto);
        public Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeam(Guid idTeam);
        public Task CancelMatch(Guid idTeam, Guid idMatch, string description);
    }
}
