using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IPlayerRepository
    {
        Task<Player?> GetPlayerByIdAsync(Guid id);

        Task<Player?> GetPlayerByIdWithMembershipRequests(Guid id);

        Task UpdatePlayer(Player player);
    }
}