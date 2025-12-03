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
    /// <summary>
    /// Serviço de autenticação que integra o Firebase Auth com a base de dados local do sistema.
    /// 
    /// Responsável por operações como Login, Registo, Alteração de Senha e Eliminação de Conta,
    /// garantindo a sincronização entre o fornecedor de identidade (Firebase) e os dados de domínio (Player/SuperAdmin).
    /// </summary>
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

        /// <summary>
        /// Construtor do serviço de autenticação Firebase.
        /// </summary>
        /// <param name="firestoreDb">Instância da base de dados Firestore (se utilizada).</param>
        /// <param name="teamRepository">Repositório de equipas.</param>
        /// <param name="playerRepository">Repositório de jogadores.</param>
        /// <param name="playerValidator">Validador de dados de jogador.</param>
        /// <param name="superAdminRepository">Repositório de super administradores.</param>
        /// <param name="configuration">Configurações da aplicação (para aceder à API Key).</param>
        /// <param name="httpClient">Cliente HTTP para chamadas REST ao Firebase.</param>
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

        /// <summary>
        /// Altera a palavra-passe do utilizador.
        /// </summary>
        /// <remarks>
        /// Requer re-autenticação (login) com a senha atual por motivos de segurança antes de permitir a alteração.
        /// </remarks>
        /// <param name="userId">O ID do utilizador (UID).</param>
        /// <param name="currentPassword">A senha atual para validação.</param>
        /// <param name="newPassword">A nova senha a definir.</param>
        /// <exception cref="AuthenticationException">Se o utilizador não existir ou a senha atual estiver incorreta.</exception>
        /// <exception cref="Exception">Se ocorrer um erro genérico no Firebase.</exception>
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

        /// <summary>
        /// Elimina permanentemente a conta do utilizador no Firebase.
        /// </summary>
        /// <param name="userId">O ID do utilizador a eliminar.</param>
        public async void DeleteUserAsync(string userId)
        {
            await FirebaseAuth.DefaultInstance.DeleteUserAsync(userId);
        }

        /// <summary>
        /// Realiza o login do utilizador utilizando a API REST do Firebase.
        /// </summary>
        /// <remarks>
        /// 1. Autentica via HTTP POST para obter o Token.
        /// 2. Se bem-sucedido, recupera os dados completos do utilizador ([GetFullUserData]) da base de dados local.
        /// 3. Anexa o token de resposta ao DTO.
        /// </remarks>
        /// <param name="email">O email do utilizador.</param>
        /// <param name="password">A senha do utilizador.</param>
        /// <returns>Um [LoginResponseDto] com os dados do utilizador e tokens de acesso.</returns>
        /// <exception cref="AuthenticationException">Se as credenciais forem inválidas ou o utilizador não existir na BD local.</exception>
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

        /// <summary>
        /// Revoga os tokens de atualização (Refresh Tokens) do utilizador, forçando o logout.
        /// </summary>
        /// <param name="userId">O ID do utilizador.</param>
        public async Task LogoutAsync(string userId)
        {
            await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(userId);
        }

        /// <summary>
        /// Regista um novo utilizador no Firebase Auth.
        /// </summary>
        /// <param name="email">O email do novo utilizador.</param>
        /// <param name="password">A senha.</param>
        /// <param name="phoneNumber">O número de telemóvel.</param>
        /// <returns>O UID (User ID) gerado pelo Firebase para o novo utilizador.</returns>
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

        /// <summary>
        /// Atualiza o endereço de email do utilizador no Firebase.
        /// </summary>
        /// <param name="userId">O ID do utilizador.</param>
        /// <param name="newEmail">O novo email a definir.</param>
        /// <exception cref="Exception">Se o novo email já estiver em uso por outra conta.</exception>
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

        /// <summary>
        /// Recupera os dados detalhados do perfil do utilizador a partir da base de dados local.
        /// </summary>
        /// <remarks>
        /// Tenta encontrar o utilizador primeiro como [SuperAdmin] e depois como [Player].
        /// </remarks>
        /// <param name="userId">O UID do utilizador (Firebase ID).</param>
        /// <returns>O DTO [LoginResponseDto] preenchido com os dados do perfil.</returns>
        /// <exception cref="AuthenticationException">Se o utilizador não for encontrado em nenhuma das tabelas (SuperAdmin ou Player).</exception>
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