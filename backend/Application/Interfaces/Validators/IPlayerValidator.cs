using Application.DTOs.PlayerDTOs;
﻿using Application.DTOs.Filters;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface IPlayerValidator
    {
        void PlayerExists(Player? player);

        void CreatePlayerValidator(CreatePlayerDto CreatePlayerDto, User? phoneUser, User? emailUser);

        void DeletePlayerValidator(Player? player);

        void GetPlayerByIdValidator(Player? player);

        void UpdatePlayerValidator(UpdatePlayerDto UpdatePlayerDto,Player player, User[] existingPlayers);

        void LeaveTeamValidator(Player player);
        
        void ValidateFiltersListTeams(FilterListTeamDto filter);

        void ValidateHasChangeDataPlayer(bool hasChange);

        void SendMembershipRequestValidator(Player player, Team team, MembershipRequest request);
    }
}
