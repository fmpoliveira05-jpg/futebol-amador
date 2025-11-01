using System;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly IPasswordHasher passwordHasher;
        private readonly IUnityOfWork unitOfWork;
        private readonly IConfiguration configuration;

        public UserService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUnityOfWork unitOfWork,
            IConfiguration configuration)
        {
            this.userRepository = userRepository;
            this.passwordHasher = passwordHasher;
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await userRepository.GetUserByEmailAsync(email);
            if (user == null)
                throw new ValidationException("Email ou password incorretos.");

            var valid = passwordHasher.VerifyPassword(password, user.Password);
            if (!valid)
                throw new ValidationException("Email ou password incorretos.");

            var jwtSection = configuration.GetSection("JwtSettings");
            var secret = jwtSection["Secret"] ?? throw new InvalidOperationException("JWT Secret missing.");
            var expirationHours = int.TryParse(jwtSection["TokenExpirationHours"], out var hours) ? hours : 12;

            var key = Encoding.ASCII.GetBytes(secret);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                }),
                Expires = DateTime.UtcNow.AddHours(expirationHours),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new ValidationException("Utilizador não encontrado.");

            if (!passwordHasher.VerifyPassword(currentPassword, user.Password))
                throw new ValidationException("A password atual está incorreta.");

            user.Password = passwordHasher.HashPassword(newPassword);
            userRepository.UpdateUser(user);

            await unitOfWork.SaveChangesAsync();
        }

        public Task LogoutAsync()
        {
            return Task.CompletedTask;
        }
    }
}