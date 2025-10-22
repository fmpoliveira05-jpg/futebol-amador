using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Validators
{
    internal class PlayerValidator : IPlayerValidator
    {
        public void PlayerExists(Player player)
        {
            if (player == null)
            {
                throw new NotFoundException("O Player não existe");
            }
        }
    }
}
