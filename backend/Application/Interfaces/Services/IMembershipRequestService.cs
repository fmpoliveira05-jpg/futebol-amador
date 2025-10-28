using Application.DTOs.Membership;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IMembershipRequestService
    {
        Task<IEnumerable<MembershipRequestDTO>> GetRequestsReceivedByPlayerFromTeams(Guid idPlayer);

        Task<IEnumerable<MembershipRequestDTO>> GetRequestsSentByPlayer(Guid idPlayer);

        Task SendMembershipRequest(SendMembershipRequestDTO dto);

        Task<Player> AcceptMembershipRequest(Guid idTeam, Guid idMembershipRequest);

        Task RefuseMembershipRequest(Guid idTeam, Guid idMembershipRequest);
    }
}