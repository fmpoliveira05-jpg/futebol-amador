using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IPlayerRepository
    {
        Task<List<Player>> GetPlayersListByIdListAsync(List<string> playerIdList);

        Task<List<Player>?> GetAllTPlayersAsync();

        Task<Player?> GetPlayerByIdAsync(string id);

        Task<Player?> GetPlayerByEmailAsync(string email);

        void DeletePlayer(Player playerToRemove);

        void UpdatePlayer(Player updatedPlayer);

        Task<Player?> GetPlayerByIdWithRequestsAsync(Guid id);

        Task AddAsync(Player player);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsDtoAsync(Guid playerId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsDtoAsyncWithFilters(Guid playerId, FilterMembershipRequestsPlayer filters);

        Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeamAsync();
    }
}
