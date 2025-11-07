using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface IMembershipValidator
    {
        void ValidateSendRequestByPlayer(Player player, Team team, MembershipRequest? existingRequest);

        void ValidateSendRequestByTeam(Team team, Player invitedPlayer, MembershipRequest? existingRequest, Player player);

        void ValidateAcceptRequestByTeam(Team team, MembershipRequest request, Player playerAccepting);

        void ValidateRejectRequestByTeam(Team team, MembershipRequest request, Player player);

        void ValidateGetRequestsByTeam(Team team, Player player);

        void ValidateAcceptRequestByPlayer(Player player, MembershipRequest request, Team team);

        void ValidateRejectRequestByPlayer(MembershipRequest request, Player player);
    }
}