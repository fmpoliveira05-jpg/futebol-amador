using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<Teams?> GetUserByIdAsync(Guid userId);
    }
}
