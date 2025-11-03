using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IMembershipRequestRepository
    {
        Task AddMembershipRequest(MembershipRequest membershipRequest);

        Task DeleteMembershipRequest(MembershipRequest membershipRequest);

        Task<MembershipRequest?> GetMembershipRequestById(Guid id);

        Task<IEnumerable<MembershipRequest>> GetMembershipRequestsByPlayer(string idPlayer);

        Task<IEnumerable<MembershipRequest>> GetMembershipRequestsByTeam(Guid idTeam);

        Task<MembershipRequest?> GetMembershipRequestByPlayerAndTeam(string idPlayer, Guid idTeam);
    }
}