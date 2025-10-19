using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IMembershipRequestRepository
    {
        Task AddMembershipRequest(MembershipRequests membershipRequest);

        Task DeleteMembershipRequest(MembershipRequests membershipRequest);

        Task<MembershipRequests?> GetMembershipRequestById(Guid id);

        Task<IEnumerable<MembershipRequests>> GetMembershipRequestsByPlayer(Guid idPlayer);

        Task<IEnumerable<MembershipRequests>> GetMembershipRequestsByTeam(Guid idTeam);

        Task<MembershipRequests?> GetMembershipRequestByPlayerAndTeam(Guid idPlayer, Guid idTeam);
    }
}
