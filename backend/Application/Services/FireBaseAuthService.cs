using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Exceptions;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Application.Services
{
    public class FireBaseAuthService : IAuthService
    {
        private readonly FirestoreDb DbContext;

        private readonly ITeamRepository TeamRepository;
        private readonly IPlayerRepository PlayerRepository;
        private readonly ISuperAdminRepository SuperAdminRepository;

        private readonly IPlayerValidator PlayerValidator;

        private readonly IConfiguration Configuration;
        private readonly string FirebaseApiKey;
        private readonly HttpClient HttpClient;

        public FireBaseAuthService(FirestoreDb firestoreDb, ITeamRepository teamRepository, IPlayerRepository playerRepository,
                                   IPlayerValidator playerValidator, ISuperAdminRepository superAdminRepository,
                                   IConfiguration configuration, HttpClient httpClient)
        {
            DbContext = firestoreDb;
            TeamRepository = teamRepository;
            PlayerRepository = playerRepository;
            PlayerValidator = playerValidator;
            SuperAdminRepository = superAdminRepository;
            Configuration = configuration;
            FirebaseApiKey = configuration["Firebase:ApiKey"]
            ?? throw new ArgumentNullException("Firebase:ApiKey não encontrada no secrets.json");
            HttpClient = httpClient;
        }

        public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await FirebaseAuth.DefaultInstance.GetUserAsync(userId);
                if (string.IsNullOrEmpty(user?.Email))
                {
                    throw new AuthenticationException("Usuário não encontrado ou sem email.");
                }


                await this.LoginAsync(user.Email, currentPassword);

                var args = new UserRecordArgs
                {
                    Uid = userId, 
                    Password = newPassword
                };

                await FirebaseAuth.DefaultInstance.UpdateUserAsync(args);
            }
            catch (AuthenticationException ex)
            {
                throw new AuthenticationException("A senha atual está incorreta.", ex);
            }
            catch (FirebaseAuthException ex)
            {
                throw new Exception($"Falha ao processar a mudança de senha: {ex.Message}", ex);
            }
        }

        public async void DeleteUserAsync(string userId)
        {
            await FirebaseAuth.DefaultInstance.DeleteUserAsync(userId);
        }

        public async Task<LoginResponseDto> LoginAsync(string email, string password)
        {
            
            var requestBody = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            var response = await HttpClient.PostAsJsonAsync("", requestBody);
     

            if (response.IsSuccessStatusCode)
            {
                var firebaseResponse = await response.Content.ReadFromJsonAsync<FirebaseLoginResponseDto>();
                if (firebaseResponse == null)
                {
                    throw new AuthenticationException("Failed to parse Firebase response.");
                }

                var userData = await GetFullUserData(firebaseResponse.LocalId);
                userData.FirebaseLoginResponseDto = firebaseResponse;
                return userData;

            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new AuthenticationException(errorResponse);
            }
            
        }

        public async Task LogoutAsync(string userId)
        {
            await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(userId);
        }

        public async Task<string> RegisterUser(string email, string password, string phoneNumber)
        {

            var userArgs = new UserRecordArgs
            {
                Email = email,
                Password = password,
                EmailVerified = false,
                PhoneNumber= phoneNumber,
                Disabled = false
            };

            UserRecord userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
            
            return userRecord.Uid;  
        }

        public async Task UpdateEmailAsync(string userId, string newEmail)
        {
            try
            {
                var args = new UserRecordArgs
                {
                    Uid = userId,
                    Email = newEmail
                };
                await FirebaseAuth.DefaultInstance.UpdateUserAsync(args);
            }
            catch (FirebaseAuthException ex)
            {
                if (ex.AuthErrorCode == AuthErrorCode.EmailAlreadyExists)
                {
                    throw new Exception("Este email já está em uso por outra conta.");
                }
                throw;
            }
        }

        public async Task<LoginResponseDto> GetFullUserData(string userId)
        {
            var superAdminUser = await SuperAdminRepository.GetSuperAdminByIdAsync(userId);
            if (superAdminUser != null)
            {
                return new LoginResponseDto
                {
                    Address = superAdminUser.Address,
                    Email = superAdminUser.Email,
                    DateOfBirth = superAdminUser.DateOfBirth,
                    Name = superAdminUser.Name,
                    Phone = superAdminUser.Phone,
                    CreationDate = superAdminUser.CreationDate,


                };
            }
            var playerUser = await PlayerRepository.GetPlayerByIdAsync(userId);
            if (playerUser != null)
            {
                return new LoginResponseDto
                {
                    Email = playerUser.Email,
                    DateOfBirth = playerUser.DateOfBirth,
                    Name = playerUser.Name,
                    Address = playerUser.Address,
                    IsAdmin = playerUser.IsAdmin,
                    Height = playerUser.Height,
                    IdTeam = playerUser.IdTeam,
                    Phone = playerUser.Phone,
                    Position = playerUser.Position,
                    CreationDate = playerUser.CreationDate,
                    IsAdminLastChanged = playerUser.IsAdminLastChangedAt,
                };
            }

            throw new AuthenticationException("Usuário não encontrado na base de dados.");
        }
    }
}
