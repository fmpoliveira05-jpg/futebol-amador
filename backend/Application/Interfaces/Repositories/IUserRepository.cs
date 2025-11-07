using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsersAsync();

        Task<User?> GetUserByIdAsync(string userId);

        Task<User?> GetUserByEmailAsync(string email);

        Task<User?> GetUserByPhoneAsync(string phone);
    }
}