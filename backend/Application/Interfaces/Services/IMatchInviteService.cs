using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.MatchInvites;

namespace Application.Interfaces.Services
{
    public interface IMatchInviteService
    {
        public Task<InfoMatchInviteDTO> SendMatchInvite(SendMatchInviteDTO dto);
        public Task<MatchDto> AcceptMatchInvite(Guid idTeam, Guid idMatchInvite);
        public Task RefuseMatchInvites(Guid idTeam, Guid idMatchInvite);
        public Task<InfoMatchInviteDTO> NegociateMatchInvite(SendMatchInviteDTO dto);
        public Task<List<InfoMatchInviteDTO>> GetAllMatchInvitesTeam(Guid idTeam);
        public Task<List<InfoMatchInviteDTO>> GetAllMatchInvitesTeamWithFilters(Guid idTeam, FilterMatchInvitesDto filter);
    }
}
