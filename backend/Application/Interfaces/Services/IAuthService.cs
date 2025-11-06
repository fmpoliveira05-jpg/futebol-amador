using System.Globalization;

namespace Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);

        Task ChangePasswordAsync(string userId, string currentPassword, string newPassword);

        Task LogoutAsync();

        void DeleteUser(string userId);

        Task<string> RegisterUser(string email, string password, string phoneNumber);
    }
}
