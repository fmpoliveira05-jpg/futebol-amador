using Application.DTOs.MatchInvites;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IMatchInviteRepository
    {
        public Task AddMatchInvite(MatchInvite matchInvite);
        public void DeleteMatchInvite(MatchInvite matchInvite);
        public Task<MatchInvite?> GetMatchInviteById(Guid id);
        public Task<MatchInvite?> GetMatchInviteByTeams(Guid idSender, Guid idReceiver);
        public Task<MatchInvite?> GetMatchInvite(SendMatchInviteDTO dto);
        public Task<List<InfoMatchInviteDTO>> GetAllMatchInviteReceiverById(Guid idReceiver);
    }
}
