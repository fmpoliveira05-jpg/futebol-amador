using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface IPlayerAuthorizationValidator
    {
        void ValidateUserId(string userId);
        public void ValidateUserIdIsSameUrl(string userId, string userIdUrl);
        void ValidatePlayerAutorizationIsAdmin(Player user, Guid idTeam);
        void ValidatePlayerAutorizationIsNotAdmin(Player user, Guid idTeam);
        void ValidatePlayerAutorizationIsMember(Player user, Guid idTeam);
        void ValidatePlayerAutorizationWithoutTeam(Player user);
    }
}
