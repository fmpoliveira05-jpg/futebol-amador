using Domain.Entities;

/**
 * Validar esta classe
 */
namespace Application.Interfaces
{
    internal interface IMembershipRequest
    {
        MembershipRequests SendMembershipRequest(MembershipRequests membershipRequests);

        MembershipRequests AddMembershipRequest(MembershipRequests membershipRequests);

        MembershipRequests AcceptMembershipRequest(Guid id);

        MembershipRequests RefuseMembershipRequest(Guid id);

        List<MembershipRequests> ShowMemberShipRequest();

        List<MembershipRequests> ClearMemberShipRequest();

        MembershipRequests GetMembershipRequestById(Guid id);
    }
}
