using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IMatchInviteRepository
    {
        Task AddMatchInvite(MatchInvite matchInvite);

        Task DeleteMatchInvite(MatchInvite matchInvite);

        Task<MatchInvite?> GetMatchInviteById(Guid id);

        Task<MatchInvite?> GetMatchInviteByTeams(Guid idSender, Guid idReceiver);
    }
}
