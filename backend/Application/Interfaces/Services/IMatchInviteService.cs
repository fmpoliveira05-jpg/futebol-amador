using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.MatchInvites;

namespace Application.Interfaces.Services
{
    public interface IMatchInviteService
    {
        public Task<InfoMatchInviteDto> SendMatchInvite(Guid idSender, SendMatchInviteDto dto);
        public Task<MatchDto> AcceptMatchInvite(Guid idTeam, Guid idMatchInvite);
        public Task RefuseMatchInvites(Guid idTeam, Guid idMatchInvite);
        public Task<InfoMatchInviteDto> NegociateMatchInvite(Guid idSender, SendMatchInviteDto dto);
        public Task<List<InfoMatchInviteDto>> GetAllMatchInvitesTeam(Guid idTeam);
        public Task<List<InfoMatchInviteDto>> GetAllMatchInvitesTeamWithFilters(Guid idTeam, FilterMatchInvitesDto filter);
    }
}
