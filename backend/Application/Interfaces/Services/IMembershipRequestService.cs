using Application.DTOs.Filters;
using Application.DTOs.MemberShip;

namespace Application.Interfaces.Services
{
    public interface IMembershipRequestService
    {
        Task<MemberShipRequestDto> SendMembershipRequestTeam(Guid teamId, string playerIdToInvite);

        Task AcceptMembershipRequestTeam(Guid teamId, Guid requestId);

        Task RejectMembershipRequestTeam(Guid teamId, Guid requestId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeamWithFilters(Guid teamId, FilterMembershipRequestsTeam filters);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncPlayer(string playerId);
        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncPlayerWithFilters(string playerId, FilterMembershipRequestsPlayer filters);
        Task<MemberShipRequestDto> AcceptMembershipRequestAsyncPlayer(string playerId, Guid requestId);
        Task<MemberShipRequestDto> RejectMembershipRequestAsyncPlayer(string playerId, Guid requestId);
        Task<MemberShipRequestDto> SendMembershipRequestAsyncPlayer(string playerId, Guid teamId);


    }
}