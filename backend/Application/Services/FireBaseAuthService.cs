using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Application.Validators;
using Domain.Exceptions;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using System.Net.Http.Json;

namespace Application.Services
{
    internal class FireBaseAuthService : IAuthService
    {
        private readonly FirestoreDb DbContext;

        private readonly ITeamRepository TeamRepository;
        private readonly IPlayerRepository PlayerRepository;
        private readonly IPlayerValidator PlayerValidator;


        public FireBaseAuthService(FirestoreDb firestoreDb, ITeamRepository teamRepository, IPlayerRepository playerRepository)
        {
            DbContext = firestoreDb;
            TeamRepository = teamRepository;
            PlayerRepository = playerRepository;
            PlayerValidator = new PlayerValidator();
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

        public async void DeleteUser(string userId)
        {
            await FirebaseAuth.DefaultInstance.DeleteUserAsync(userId);
        }

        public async Task<FirebaseLoginResponseDto> LoginAsync(string email, string password)
        {
            string firebaseApiKey = "REMOVIDO";

            var firebaseAuthUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={firebaseApiKey}";

            var requestBody = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsJsonAsync(firebaseAuthUrl, requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var firebaseResponse = await response.Content.ReadFromJsonAsync<FirebaseLoginResponseDto>();
                    if (firebaseResponse == null)
                    {
                        throw new AuthenticationException("Failed to parse Firebase response.");
                    }
                    return firebaseResponse;
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new AuthenticationException(errorResponse);
                }
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
    }
}
