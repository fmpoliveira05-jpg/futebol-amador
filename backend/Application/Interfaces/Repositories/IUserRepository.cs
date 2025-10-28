using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<Player?> GetUserByEmailAsync(string email);

        Task<Player?> GetUserByIdAsync(Guid id);

        void UpdateUser(Player player);
    }
}