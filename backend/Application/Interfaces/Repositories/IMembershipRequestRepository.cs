using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IMembershipRequestRepository
    {
        Task AddMembershipRequest(MembershipRequest request);

        void RemoveMembershipRequest(MembershipRequest request);

        Task<MembershipRequest?> GetMembershipRequestById(Guid id);

        Task<MembershipRequest?> GetMembershipRequestByPlayerAndTeam(string playerId, Guid teamId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeamWithFilters(Guid teamId, FilterMembershipRequestsTeam filters);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsByPlayer(string playerId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsByPlayerWithFilters(string playerId, FilterMembershipRequestsPlayer filters);

        Task<bool> ExistsRequestBetweenPlayerAndTeam(string playerId, Guid teamId);
        Task RemoveAllMemberShipRequestsOfPlayer(string playerId);
        Task RemoveAllMemberShipRequestsOfTeam(Guid idTeam);
    }
}