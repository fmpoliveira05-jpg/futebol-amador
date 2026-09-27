using Api.Seguranca;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Api.Extensions
{
    /// <summary>
    /// Autenticação com os ID tokens do Firebase Authentication.
    /// </summary>
    /// <remarks>
    /// O Firebase publica os metadados OpenID em <c>https://securetoken.google.com/{projectId}</c>;
    /// com essa <c>Authority</c>, o JwtBearer descarrega e renova sozinho as chaves públicas (a
    /// Google roda-as com frequência). Antes as chaves eram descarregadas à mão no arranque e
    /// renovadas com um pedido HTTP síncrono dentro da validação de cada token.
    /// </remarks>
    public static class FirebaseAuthenticationExtensions
    {
        /// <summary>Caminhos dos hubs SignalR, onde o token chega na query string.</summary>
        private static readonly string[] CaminhosHubs = { "/StartMatch", "/FinishMatch", "/Notification" };

        public static IServiceCollection AddFirebaseAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var projectId = configuration["Firebase:ProjectId"];
            var credentialPath = configuration["Firebase:CredentialPath"];

            if (string.IsNullOrEmpty(projectId))
            {
                throw new InvalidOperationException("Falta a configuração Firebase:ProjectId (ver README do backend).");
            }

            if (string.IsNullOrEmpty(credentialPath) || !File.Exists(credentialPath))
            {
                throw new FileNotFoundException(
                    "Ficheiro de credenciais do Firebase não encontrado. Indica o caminho em Firebase:CredentialPath (user-secrets ou variável de ambiente).",
                    credentialPath);
            }

            // O SDK Admin (gestão de utilizadores, mensagens push) usa estas credenciais.
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(credentialPath),
                    ProjectId = projectId,
                });
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://securetoken.google.com/{projectId}";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = $"https://securetoken.google.com/{projectId}",
                        ValidateAudience = true,
                        ValidAudience = projectId,
                        ValidateLifetime = true,
                        // A tolerância por omissão era de 5 minutos; os relógios da Google e do servidor
                        // estão sincronizados por NTP, 1 minuto chega.
                        ClockSkew = TimeSpan.FromMinutes(1),
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var token = TokenDoPedido(context.Request);
                            if (token != null)
                            {
                                context.Token = token;
                            }
                            return Task.CompletedTask;
                        },

                        // Tokens de sessões terminadas (logout, mudança de palavra-passe ou de e-mail) ou
                        // de contas desativadas deixam de ser aceites, mesmo antes de expirarem.
                        OnTokenValidated = async context =>
                        {
                            var revogacao = context.HttpContext.RequestServices.GetRequiredService<IRevogacaoTokens>();
                            var uid = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                            var authTime = LerAuthTime(context.Principal);

                            if (string.IsNullOrEmpty(uid) || authTime == null ||
                                !await revogacao.SessaoValidaAsync(uid, authTime.Value, context.HttpContext.RequestAborted))
                            {
                                context.Fail("A sessão foi terminada.");
                            }
                        },
                    };
                });

            return services;
        }

        /// <summary>
        /// Onde vem o token do pedido, além do cabeçalho <c>Authorization</c> (que o JwtBearer lê sozinho):
        /// <list type="bullet">
        /// <item>hubs SignalR: <c>?access_token=</c> (os WebSockets não enviam cabeçalhos);</item>
        /// <item>frontend web: o cookie <see cref="SessaoWeb.CookieSessao"/>, só quando não há <c>Authorization</c>.</item>
        /// </list>
        /// </summary>
        /// <returns>O token, ou <c>null</c> para o JwtBearer usar o cabeçalho.</returns>
        internal static string? TokenDoPedido(HttpRequest request)
        {
            var tokenQuery = request.Query["access_token"].ToString();
            if (!string.IsNullOrEmpty(tokenQuery) &&
                CaminhosHubs.Any(h => request.Path.StartsWithSegments(h, StringComparison.OrdinalIgnoreCase)))
            {
                return tokenQuery;
            }

            if (string.IsNullOrEmpty(request.Headers.Authorization) &&
                request.Cookies.TryGetValue(SessaoWeb.CookieSessao, out var tokenCookie) &&
                !string.IsNullOrEmpty(tokenCookie))
            {
                return tokenCookie;
            }

            return null;
        }

        /// <summary>
        /// A claim <c>auth_time</c> (segundos). Conforme o mapeamento de claims, chega com o nome
        /// original ou como <see cref="ClaimTypes.AuthenticationInstant"/>.
        /// </summary>
        internal static long? LerAuthTime(ClaimsPrincipal? principal)
        {
            var valor = principal?.FindFirst("auth_time")?.Value ?? principal?.FindFirst(ClaimTypes.AuthenticationInstant)?.Value;
            return long.TryParse(valor, out var segundos) ? segundos : null;
        }
    }
}
