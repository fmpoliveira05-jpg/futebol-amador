using Application.DTOs;
using System.Globalization;

namespace Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(string email, string password);

        Task ChangePasswordAsync(string userId, string currentPassword, string newPassword);

        Task UpdateEmailAsync(string userId, string newEmail);

        Task LogoutAsync(string userId);

        void DeleteUserAsync(string userId);

        Task<string> RegisterUser(string email, string password, string phoneNumber);
    }
}
