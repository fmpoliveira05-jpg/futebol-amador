using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IMatchService
    {
        public Task<List<InfoMatchCalendar>> GetCalendar(Guid idTeam);
        public Task<List<InfoMatchCalendar>> GetCalendarWithFilters(Guid idTeam, FilterCalendarDto filter);
        public Task<InfoPostPoneMatch> PostPoneMatch(Guid idTeam, PostPoneMatchDto dto);
        public Task<MatchDto> AcceptPostPoneMatch(Guid idTeam, AcceptRefusePostPoneDto dto);
        public Task RejectPostPoneMatch(Guid idTeam, AcceptRefusePostPoneDto dto);
        public Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeam(Guid idTeam);
        public Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeamWithFilters(Guid idTeam, FilterPostPoneMatchDto filter);
        public Task CancelMatch(Guid idTeam, Guid idMatch, string description, Player player);
    }
}
