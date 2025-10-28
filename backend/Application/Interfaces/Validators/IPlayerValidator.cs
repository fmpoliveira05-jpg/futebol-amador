using Application.DTOs.PlayerDTOs;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface IPlayerValidator
    {
        void PlayerExists(Player? player);

        void CreatePlayerValidator(CreatePlayerDTO createPlayerDTO, Player[] players);

        void DeletePlayerValidator(Player? player);

        void GetPlayerByIdValidator(Player player);

        void UpdatePlayerValidator(UpdatePlayerDTO updatePlayerDTO,Player player, Player playerEmail);

        void LeaveTeamValidator(Player player);


    }
}
