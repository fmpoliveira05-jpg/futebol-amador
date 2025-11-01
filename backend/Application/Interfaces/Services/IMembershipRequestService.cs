using Application.DTOs.Membership;
using Application.DTOs.MemberShip;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IMembershipRequestService
    {
        Task<IEnumerable<MemberShipRequestDto>> GetRequestsReceivedByPlayerFromTeams(Guid idPlayer);

        Task<IEnumerable<MemberShipRequestDto>> GetRequestsSentByPlayer(Guid idPlayer);

        Task SendMembershipRequest(SendMembershipRequestDTO dto);

        Task<Player> AcceptMembershipRequest(Guid idTeam, Guid idMembershipRequest);

        Task RefuseMembershipRequest(Guid idTeam, Guid idMembershipRequest);
    }
}