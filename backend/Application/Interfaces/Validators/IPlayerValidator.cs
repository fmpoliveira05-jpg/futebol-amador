using Application.DTOs.PlayerDTOs;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface IPlayerValidator
    {
        void PlayerExists(Player? player);

        void CreatePlayerValidator(CreatePlayerDTO createPlayerDTO, Users[] players);

        void DeletePlayerValidator(Player? player);

        void GetPlayerByIdValidator(Player player);

        void UpdatePlayerValidator(UpdatePlayerDTO updatePlayerDTO,Player player, Users[] existingPlayers);

        void LeaveTeamValidator(Player player);


    }
}
