using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface IAuthorizationValidator
    {
        void ValidateUserId(string userId);
        void ValidatePlayerAutorizationIsAdmin(Player user, Guid idTeam);
        void ValidatePlayerAutorizationIsNotAdmin(Player user, Guid idTeam);
        void ValidatePlayerAutorizationIsMember(Player user, Guid idTeam);
        void ValidatePlayerAutorizationWithoutTeam(Player user);
    }
}
