using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.MatchInvites;

namespace Application.Interfaces.Services
{
    public interface IMatchInviteService
    {
        public Task<InfoMatchInviteDto> SendMatchInvite(string userId, Guid idSender, SendMatchInviteDto dto);
        public Task<MatchDto> AcceptMatchInvite(string userId, Guid idTeam, Guid idMatchInvite);
        public Task RefuseMatchInvites(string userId, Guid idTeam, Guid idMatchInvite);
        public Task<InfoMatchInviteDto> NegociateMatchInvite(string userId, Guid idSender, SendMatchInviteDto dto);
        public Task<List<InfoMatchInviteDto>> GetAllMatchInvitesTeam(string userId, Guid idTeam);
        public Task<List<InfoMatchInviteDto>> GetAllMatchInvitesTeamWithFilters(string userId,Guid idTeam, FilterMatchInvitesDto filter);
    }
}
