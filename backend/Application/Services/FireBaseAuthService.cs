using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Exceptions;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Application.Services
{
    /// <summary>
    /// Serviço de autenticação que integra o Firebase Auth com a base de dados local do sistema.
    ///
    /// Responsável por operações como Login, Registo, Alteração de Senha e Eliminação de Conta,
    /// garantindo a sincronização entre o fornecedor de identidade (Firebase) e os dados de domínio (Player/SuperAdmin).
    /// </summary>
    /// <remarks>
    /// Usa a API REST do Firebase Authentication (<c>identitytoolkit</c> e <c>securetoken</c>) com a
    /// chave Web do projeto (<c>Firebase:ApiKey</c>; se faltar, é lida do parâmetro <c>key</c> de
    /// <c>Authentication:TokenUri</c>) e o SDK Admin para gerir as contas. Os erros do Firebase ficam
    /// no log (sem e-mails nem tokens) e o cliente recebe sempre mensagens genéricas.
    /// </remarks>
    public class FireBaseAuthService : IAuthService
    {
        private const string UrlIdentityToolkit = "https://identitytoolkit.googleapis.com/v1/";
        private const string UrlSecureToken = "https://securetoken.googleapis.com/v1/token";

        private readonly IPlayerRepository PlayerRepository;
        private readonly ISuperAdminRepository SuperAdminRepository;
        private readonly HttpClient HttpClient;
        private readonly ILogger<FireBaseAuthService> logger;
        private readonly string? apiKey;

        /// <inheritdoc />
        public bool ExigeEmailVerificado { get; }

        /// <summary>
        /// Construtor do serviço de autenticação Firebase.
        /// </summary>
        /// <param name="playerRepository">Repositório de jogadores.</param>
        /// <param name="superAdminRepository">Repositório de super administradores.</param>
        /// <param name="httpClient">Cliente HTTP para chamadas REST ao Firebase (base: <c>Authentication:TokenUri</c>).</param>
        /// <param name="configuration">Configuração (<c>Auth:RequireVerifiedEmail</c>, <c>Firebase:ApiKey</c>).</param>
        /// <param name="logger">Log (nunca recebe e-mails, palavras-passe nem tokens).</param>
        public FireBaseAuthService(IPlayerRepository playerRepository, ISuperAdminRepository superAdminRepository,
            HttpClient httpClient, IConfiguration configuration, ILogger<FireBaseAuthService> logger)
        {
            PlayerRepository = playerRepository;
            SuperAdminRepository = superAdminRepository;
            HttpClient = httpClient;
            this.logger = logger;
            ExigeEmailVerificado = configuration.GetValue("Auth:RequireVerifiedEmail", true);
            apiKey = ObterApiKey(configuration);
        }

        #region Sessão

        /// <inheritdoc />
        public async Task<LoginResponseDto> LoginAsync(string email, string password)
        {
            var sessao = await IniciarSessaoFirebaseAsync(email, password);

            // Só se chega aqui com a palavra-passe certa: dizer que falta confirmar o e-mail não
            // revela a existência da conta a quem não a conhece.
            if (ExigeEmailVerificado && !EmailVerificadoNoToken(sessao.IdToken))
            {
                throw new EmailNaoVerificadoException();
            }

            var userData = await GetFullUserData(sessao.LocalId);
            userData.FirebaseLoginResponseDto = sessao;
            return userData;
        }

        /// <inheritdoc />
        public async Task<LoginResponseDto?> IniciarSessaoAposRegistoAsync(string email, string password)
        {
            var sessao = await IniciarSessaoFirebaseAsync(email, password);
            await EnviarVerificacaoEmailAsync(sessao.IdToken);

            if (ExigeEmailVerificado)
            {
                return null;
            }

            var userData = await GetFullUserData(sessao.LocalId);
            userData.FirebaseLoginResponseDto = sessao;
            return userData;
        }

        /// <inheritdoc />
        public async Task ReenviarVerificacaoEmailAsync(string email, string password)
        {
            FirebaseLoginResponseDto sessao;
            try
            {
                sessao = await IniciarSessaoFirebaseAsync(email, password);
            }
            catch (UnauthorizedAccessException)
            {
                // Credenciais erradas: não se diz nada ao cliente (resposta sempre igual).
                return;
            }

            if (!EmailVerificadoNoToken(sessao.IdToken))
            {
                await EnviarVerificacaoEmailAsync(sessao.IdToken);
            }
        }

        /// <inheritdoc />
        public async Task PedirRecuperacaoPalavraPasseAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                logger.LogWarning("Recuperação da palavra-passe indisponível: falta Firebase:ApiKey.");
                return;
            }

            var resposta = await HttpClient.PostAsJsonAsync(
                $"{UrlIdentityToolkit}accounts:sendOobCode?key={apiKey}",
                new { requestType = "PASSWORD_RESET", email });

            if (!resposta.IsSuccessStatusCode)
            {
                // EMAIL_NOT_FOUND e afins ficam só no log (sem o e-mail): o cliente recebe sempre a mesma resposta.
                logger.LogInformation("O Firebase não enviou a recuperação da palavra-passe ({Estado}).", (int)resposta.StatusCode);
            }
        }

        /// <inheritdoc />
        public async Task<FirebaseLoginResponseDto> RenovarSessaoAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Falta a configuração Firebase:ApiKey.");
            }

            var corpo = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
            });

            var resposta = await HttpClient.PostAsync($"{UrlSecureToken}?key={apiKey}", corpo);
            if (!resposta.IsSuccessStatusCode)
            {
                throw new UnauthorizedAccessException("A sessão terminou. Entra outra vez.");
            }

            var dados = await resposta.Content.ReadFromJsonAsync<RespostaSecureToken>()
                        ?? throw new UnauthorizedAccessException("A sessão terminou. Entra outra vez.");

            // Depois de mudar de e-mail, o token novo já vem com email_verified=false.
            if (ExigeEmailVerificado && !EmailVerificadoNoToken(dados.IdToken))
            {
                throw new UnauthorizedAccessException("A sessão terminou. Entra outra vez.");
            }

            return new FirebaseLoginResponseDto
            {
                IdToken = dados.IdToken,
                RefreshToken = dados.RefreshToken,
                ExpiresIn = dados.ExpiresIn,
                LocalId = dados.UserId,
                Email = string.Empty,
            };
        }

        /// <summary>
        /// Revoga os tokens de atualização (Refresh Tokens) do utilizador, forçando o logout.
        /// </summary>
        /// <param name="userId">O ID do utilizador.</param>
        public async Task LogoutAsync(string userId)
        {
            await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(userId);
        }

        #endregion

        #region Conta

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
        /// <exception cref="ValidationException">Se o Firebase recusar a alteração.</exception>
        public async Task<FirebaseLoginResponseDto?> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var email = await ConfirmarPalavraPasseAtualAsync(userId, currentPassword);

            try
            {
                await FirebaseAuth.DefaultInstance.UpdateUserAsync(new UserRecordArgs
                {
                    Uid = userId,
                    Password = newPassword
                });
            }
            catch (FirebaseAuthException ex)
            {
                logger.LogWarning("O Firebase recusou a alteração da palavra-passe ({Codigo}).", ex.AuthErrorCode);
                throw new ValidationException("Não foi possível alterar a palavra-passe.");
            }

            // O Firebase termina as sessões antigas; abre-se já uma nova com a palavra-passe nova.
            try
            {
                return await IniciarSessaoFirebaseAsync(email, newPassword);
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

        /// <summary>
        /// Elimina permanentemente a conta do utilizador no Firebase.
        /// </summary>
        /// <param name="userId">O ID do utilizador a eliminar.</param>
        public async Task DeleteUserAsync(string userId)
        {
            await FirebaseAuth.DefaultInstance.DeleteUserAsync(userId);
        }

        /// <inheritdoc />
        public async Task ConfirmarPalavraPasseAsync(string userId, string password)
        {
            await ConfirmarPalavraPasseAtualAsync(userId, password);
        }

        /// <inheritdoc />
        public async Task<HashSet<string>> ContasPorConfirmarAsync(IReadOnlyCollection<string> userIds)
        {
            var porConfirmar = new HashSet<string>(StringComparer.Ordinal);

            // O SDK Admin aceita no máximo 100 identificadores por pedido.
            foreach (var bloco in userIds.Chunk(100))
            {
                var resultado = await FirebaseAuth.DefaultInstance.GetUsersAsync(
                    bloco.Select(uid => (UserIdentifier)new UidIdentifier(uid)).ToList());

                foreach (var utilizador in resultado.Users)
                {
                    if (!utilizador.EmailVerified)
                    {
                        porConfirmar.Add(utilizador.Uid);
                    }
                }

                // Perfis sem conta no Firebase (a conta foi apagada na consola, por exemplo).
                var existentes = resultado.Users.Select(u => u.Uid).ToHashSet(StringComparer.Ordinal);
                foreach (var uid in bloco.Where(uid => !existentes.Contains(uid)))
                {
                    porConfirmar.Add(uid);
                }
            }

            return porConfirmar;
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
                PhoneNumber = phoneNumber,
                Disabled = false
            };

            try
            {
                UserRecord userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
                return userRecord.Uid;
            }
            catch (FirebaseAuthException ex)
            {
                // EMAIL_EXISTS, PHONE_NUMBER_EXISTS, ...: a mensagem não diz qual dos dados já existe.
                logger.LogInformation("O Firebase recusou a criação da conta ({Codigo}).", ex.AuthErrorCode);
                throw new ValidationException("Não foi possível criar a conta com estes dados.");
            }
        }

        /// <summary>
        /// Atualiza o endereço de email do utilizador no Firebase, depois de confirmar a palavra-passe.
        /// </summary>
        /// <remarks>
        /// O e-mail novo fica por confirmar e recebe a mensagem de confirmação; as sessões abertas
        /// (incluindo a atual) são revogadas.
        /// </remarks>
        /// <param name="userId">O ID do utilizador.</param>
        /// <param name="currentPassword">A palavra-passe atual.</param>
        /// <param name="newEmail">O novo email a definir.</param>
        /// <exception cref="AuthenticationException">Se a palavra-passe atual estiver errada.</exception>
        /// <exception cref="ValidationException">Se o e-mail não puder ser usado.</exception>
        public async Task UpdateEmailAsync(string userId, string currentPassword, string newEmail)
        {
            await ConfirmarPalavraPasseAtualAsync(userId, currentPassword);

            try
            {
                await FirebaseAuth.DefaultInstance.UpdateUserAsync(new UserRecordArgs
                {
                    Uid = userId,
                    Email = newEmail,
                    EmailVerified = false,
                });
            }
            catch (FirebaseAuthException ex)
            {
                logger.LogInformation("O Firebase recusou a alteração do e-mail ({Codigo}).", ex.AuthErrorCode);
                throw new ValidationException("Não foi possível usar este e-mail.");
            }

            try
            {
                var sessao = await IniciarSessaoFirebaseAsync(newEmail, currentPassword);
                await EnviarVerificacaoEmailAsync(sessao.IdToken);
            }
            catch (UnauthorizedAccessException)
            {
                logger.LogWarning("Não foi possível enviar a confirmação do e-mail novo.");
            }

            await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(userId);
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

            throw new AuthenticationException("Utilizador não encontrado na base de dados.");
        }

        #endregion

        #region Auxiliares

        /// <summary>
        /// <c>signInWithPassword</c> do Firebase. Qualquer falha dá a mesma mensagem, para não se
        /// saber se o e-mail existe.
        /// </summary>
        private async Task<FirebaseLoginResponseDto> IniciarSessaoFirebaseAsync(string email, string password)
        {
            var requestBody = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var response = await HttpClient.PostAsJsonAsync("", requestBody);

            if (!response.IsSuccessStatusCode)
            {
                // EMAIL_NOT_FOUND, INVALID_PASSWORD, USER_DISABLED, ...: não se mostra ao cliente.
                throw new UnauthorizedAccessException("E-mail ou palavra-passe incorretos.");
            }

            return await response.Content.ReadFromJsonAsync<FirebaseLoginResponseDto>()
                   ?? throw new AuthenticationException("Resposta inesperada do serviço de autenticação.");
        }

        /// <summary>Confirma a palavra-passe atual e devolve o e-mail da conta.</summary>
        private async Task<string> ConfirmarPalavraPasseAtualAsync(string userId, string currentPassword)
        {
            var user = await FirebaseAuth.DefaultInstance.GetUserAsync(userId);
            if (string.IsNullOrEmpty(user?.Email))
            {
                throw new AuthenticationException("Utilizador não encontrado ou sem e-mail.");
            }

            try
            {
                await IniciarSessaoFirebaseAsync(user.Email, currentPassword);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Não pode sair como 401: para o frontend isso significa "sessão terminada".
                throw new AuthenticationException("A palavra-passe atual está incorreta.", ex);
            }

            return user.Email;
        }

        /// <summary><c>accounts:sendOobCode</c> com <c>VERIFY_EMAIL</c>. Falhas ficam só no log.</summary>
        private async Task EnviarVerificacaoEmailAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                logger.LogWarning("Não foi enviada a confirmação do e-mail: falta Firebase:ApiKey.");
                return;
            }

            try
            {
                var resposta = await HttpClient.PostAsJsonAsync(
                    $"{UrlIdentityToolkit}accounts:sendOobCode?key={apiKey}",
                    new { requestType = "VERIFY_EMAIL", idToken });

                if (!resposta.IsSuccessStatusCode)
                {
                    logger.LogWarning("O Firebase não enviou a confirmação do e-mail ({Estado}).", (int)resposta.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "Falha de rede ao pedir a confirmação do e-mail.");
            }
        }

        /// <summary>
        /// Lê a claim <c>email_verified</c> de um ID token acabado de receber do Firebase (por HTTPS,
        /// por isso não é preciso validar a assinatura aqui).
        /// </summary>
        internal static bool EmailVerificadoNoToken(string? idToken)
        {
            var partes = idToken?.Split('.');
            if (partes == null || partes.Length < 2)
            {
                return false;
            }

            try
            {
                var payload = partes[1].Replace('-', '+').Replace('_', '/');
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                using var json = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
                return json.RootElement.TryGetProperty("email_verified", out var valor) &&
                       valor.ValueKind == JsonValueKind.True;
            }
            catch (Exception ex) when (ex is FormatException or JsonException or ArgumentException)
            {
                return false;
            }
        }

        /// <summary>Chave Web do Firebase: <c>Firebase:ApiKey</c> ou o <c>key</c> de <c>Authentication:TokenUri</c>.</summary>
        private static string? ObterApiKey(IConfiguration configuration)
        {
            var chave = configuration["Firebase:ApiKey"];
            if (!string.IsNullOrWhiteSpace(chave))
            {
                return chave;
            }

            var tokenUri = configuration["Authentication:TokenUri"];
            if (!string.IsNullOrWhiteSpace(tokenUri) && Uri.TryCreate(tokenUri, UriKind.Absolute, out var uri))
            {
                return HttpUtility.ParseQueryString(uri.Query)["key"];
            }

            return null;
        }

        /// <summary>Resposta do <c>securetoken.googleapis.com/v1/token</c>.</summary>
        private sealed class RespostaSecureToken
        {
            [JsonPropertyName("id_token")] public string IdToken { get; set; } = string.Empty;
            [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = string.Empty;
            [JsonPropertyName("expires_in")] public string ExpiresIn { get; set; } = string.Empty;
            [JsonPropertyName("user_id")] public string UserId { get; set; } = string.Empty;
        }

        #endregion
    }
}
