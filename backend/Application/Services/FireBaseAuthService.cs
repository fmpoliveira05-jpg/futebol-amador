using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Application.Validators;
using Google.Cloud.Firestore;
using FirebaseAdmin.Auth;

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

        public Task<string> LoginAsync(string email, string password)
        {
            throw new NotImplementedException();
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
