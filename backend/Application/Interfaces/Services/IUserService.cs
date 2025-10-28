using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<string> LoginAsync(string email, string password);

        Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

        Task LogoutAsync();
    }
}
