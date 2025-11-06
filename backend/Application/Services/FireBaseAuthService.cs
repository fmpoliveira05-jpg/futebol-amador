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

        public Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public void DeleteUser(string userId)
        {
            FirebaseAuth.DefaultInstance.DeleteUserAsync(userId);
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

        public Task LogoutAsync()
        {
            throw new NotImplementedException();
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
    }
}
