using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IMembershipRequestService
    {
        Task<MemberShipRequestDto> SendMembershipRequest(Guid teamId, string playerIdToInvite, string adminUserId);

        Task AcceptMembershipRequest(Guid teamId, Guid requestId, string adminUserId);

        Task RejectMembershipRequest(Guid teamId, Guid requestId, string adminUserId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId, string adminUserId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId, string adminUserId, FilterMembershipRequestsTeam filters);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsByPlayer(string playerId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsByPlayer(string playerId, FilterMembershipRequestsPlayer filters);

        Task<MemberShipRequestDto> PlayerSendMembershipRequest(string playerId, Guid teamId);

    }
}