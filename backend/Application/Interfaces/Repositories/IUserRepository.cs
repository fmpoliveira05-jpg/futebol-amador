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
        Task<List<User?>> GetAllUsersAsync();

        Task<User?> GetUserByIdAsync(string userId);

        Task<User?> GetUserByEmailAsync(string email);

        Task<User?> GetUserByPhoneAsync(string phone);
    }
}