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

        public void PlayerHasChatRoomsValidation(Player player) {
            PlayerExists(player);
            
            
        }

        // acabar! falta ver como buscar as chatrooms do firebase e ver se faz sentido guardar no db do backend tambem
        private void PlayerHasChatRooms(Player player) { 
            //if(player.)
        }
    }
}
