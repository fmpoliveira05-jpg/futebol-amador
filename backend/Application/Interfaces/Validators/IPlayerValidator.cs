using Application.DTOs.PlayerDTOs;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    internal interface IPlayerValidator
    {
        void PlayerExists(Player player);

        void CreatePlayerValidator(CreatePlayerDTO createPlayerDTO, Player player);

        void DeleteTeamValidator(Player player);

        void GetPlayerByIdValidator(Player player);

        void UpdatePlayerValidator(UpdatePlayerDTO updatePlayerDTO,Player player);

        void LeaveTeamValidator(Player player);


    }
}
