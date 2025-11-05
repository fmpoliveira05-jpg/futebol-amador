using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface IMembershipValidator
    {
        void ValidateSendRequestByPlayer(Player player, Team team, MembershipRequest? existingRequest);

        void ValidateSendRequestByTeam(Team team, Player invitedPlayer, MembershipRequest? existingRequest);

        void ValidateAcceptRequestByTeam(Team team, MembershipRequest request);

        void ValidateRejectRequestByTeam(Team team, MembershipRequest request);

        void ValidateGetRequestsByTeam(Team team);

        void ValidateAcceptRequestByPlayer(Player player, MembershipRequest request, Team team);

        void ValidateRejectRequestByPlayer(MembershipRequest request, Player player);
    }
}