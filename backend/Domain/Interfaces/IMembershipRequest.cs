public interface IMemberShipRequest
{
    MemberShipRequest SendMemberShipRequest(MemberShipRequest: MemberShipRequest);

    MemberShipRequest AddMemberShipRequest(MemberShipRequest: MemberShipRequest);

    MemberShipRequest AcceptMemberShipRequest(IdMemberShipRequest: string);

    MemberShipRequest RefuseMemberShipRequest(IdMemberShipRequest: string);

    MemberShipRequest ShowMemberShipRequest(IdMemberShipRequest: string);

    List<MemberShipRequest> ListMemberShipRequestsByTeam();

    List<MemberShipRequest> ClearMemberShipRequest();
}