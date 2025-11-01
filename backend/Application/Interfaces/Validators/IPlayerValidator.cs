using Application.DTOs.Filters;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface IPlayerValidator
    {
        void PlayerExists(Player player);
        void ValidateFiltersListTeams(FilterListTeamDto filter);
    }
}
