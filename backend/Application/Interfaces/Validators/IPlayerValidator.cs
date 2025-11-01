using Application.DTOs.PlayerDTOs;
﻿using Application.DTOs.Filters;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface IPlayerValidator
    {
        void CreatePlayerValidator(CreatePlayerDto createPlayerDTO, Player[] players);
        void DeletePlayerValidator(Player? player);
        void UpdatePlayerValidator(UpdatePlayerDTO updatePlayerDTO,Player player, Player playerEmail);
        void ValidateHasChangeDataPlayer(bool hasChange);
        void LeaveTeamValidator(Player player);
        void PlayerExists(Player player);
        void ValidateFiltersListTeams(FilterListTeamDto filter);
    }
}
