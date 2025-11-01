using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<List<Users?>> GetAllUsersAsync();

        Task<Users?> GetUserByIdAsync(Guid id);

        Task<Users?> GetUserByEmailAsync(string email);

        Task<Users?> GetUserByPhoneAsync(string phone);
    }
}
