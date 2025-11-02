using Application.DTOs.PlayerDTOs;
﻿using Application.DTOs.Filters;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface IPlayerValidator
    {
        void PlayerExists(Player? player);

        void CreatePlayerValidator(CreatePlayerDto createPlayerDTO, Users[] players);

        void DeletePlayerValidator(Player? player);

        void GetPlayerByIdValidator(Player player);

        void UpdatePlayerValidator(UpdatePlayerDto updatePlayerDTO,Player player, Users[] existingPlayers);

        void LeaveTeamValidator(Player player);
        
        void ValidateFiltersListTeams(FilterListTeamDto filter);

        void ValidateHasChangeDataPlayer(bool hasChange);

        void SendMembershipRequestValidator(Player player, Teams team, MembershipRequests request);
    }
}
