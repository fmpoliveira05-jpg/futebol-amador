using Application.DTOs.MatchInvites;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IMatchInviteRepository
    {
        public Task AddMatchInvite(MatchInvite matchInvite);
        public void DeleteMatchInvite(MatchInvite matchInvite);
        public Task<MatchInvite?> GetMatchInviteById(Guid id);
        public Task<MatchInvite?> GetMatchInviteWithPitchByTeams(Guid idSender, Guid idReceiver);
        public Task<MatchInvite?> GetMatchInvite(Guid idSender, Guid idReceiver, DateTime gameDate);
        public Task<List<InfoMatchInviteDTO>> GetAllMatchInviteReceiverById(Guid idReceiver);
    }
}
