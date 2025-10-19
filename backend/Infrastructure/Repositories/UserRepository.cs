using Application.Interfaces.Repositories;
using Domain.Entities;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        public Task<Teams?> GetUserByIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
