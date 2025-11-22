using Application.DTOs.Filters;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;

namespace Application.Interfaces.Services
{
    public interface IPlayerService
    {
        Task<string> CreatePlayerAsync(CreatePlayerDto playerDTO);

        Task<PlayerDetailsDto> GetPlayerByIdAsync(string playerId);

        Task UpdatePlayerAsync(string playerId, UpdatePlayerDto dto);

        Task DeletePlayerAsync(string playerId);

        Task<List<InfoTeamsDto>> GetTeamListWithFilters(FilterListTeamDto filter);

        Task<List<InfoTeamsDto>> GetListTeams();

        Task<string> LeaveTeam(string playerId);

        Task<List<InfoPlayerDto?>> ListPlayers(FilterTeamDto? filter);
    }
}
