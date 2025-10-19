using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IMatchInviteService
    {
        public Task SendMatchInvite(SendMatchInviteDTO dto);

        public Task<Matches> AcceptMatchInvite(Guid idTeam, Guid idMatchInvite);

        public Task RefuseMatchInvites(Guid idTeam, Guid idMatchInvite);

        public Task<MatchInvite> NegociateMatchInvite(SendMatchInviteDTO dto);
    }
}
