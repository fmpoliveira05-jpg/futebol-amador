using Domain.Entities;

namespace Application.Interfaces.Validators
{
    internal interface IPlayerValidator
    {
        void PlayerExists(Player player);
    }
}
