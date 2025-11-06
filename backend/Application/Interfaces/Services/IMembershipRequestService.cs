using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IMembershipRequestService
    {
        Task<MemberShipRequestDto> SendMembershipRequestTeam(Guid teamId, string playerIdToInvite);

        Task AcceptMembershipRequestTeam(Guid teamId, Guid requestId, string adminId);

        Task RejectMembershipRequestTeam(Guid teamId, Guid requestId, Player player);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId, Player player);

        //Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeamWithFilters(Guid teamId, FilterMembershipRequestsTeam filters);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncPlayer(string playerId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncPlayerWithFilters(string playerId, FilterMembershipRequestsPlayer filters);

        Task<MemberShipRequestDto> AcceptMembershipRequestAsyncPlayer(string playerId, Guid requestId);

        Task<MemberShipRequestDto> RejectMembershipRequestAsyncPlayer(string playerId, Guid requestId);

        Task<MemberShipRequestDto> SendMembershipRequestAsyncPlayer(string playerId, Guid teamId);


    }
}