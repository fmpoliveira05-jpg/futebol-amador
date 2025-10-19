using Application.Interfaces.Repositories;
using Domain.Entities;

namespace Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        Task<Player?> IPlayerRepository.GetPlayerByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        Task<Player?> IPlayerRepository.GetPlayerByIdWithMembershipRequests(Guid id)
        {
            throw new NotImplementedException();
        }

        Task IPlayerRepository.UpdatePlayer(Player player)
        {
            throw new NotImplementedException();
        }
    }
}
