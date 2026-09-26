using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
                    };

                    options.Events = new JwtBearerEvents
                    {
                        // Os clientes SignalR (WebSockets) não conseguem enviar o cabeçalho
                        // Authorization, por isso mandam o token em ?access_token=.
                        OnMessageReceived = context =>
                        {
                            var token = context.Request.Query["access_token"];
                            var caminho = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(token) &&
                                CaminhosHubs.Any(h => caminho.StartsWithSegments(h, StringComparison.OrdinalIgnoreCase)))
                            {
                                context.Token = token;
                            }
                            return Task.CompletedTask;
                        },
                    };
                });

            return services;
        }
    }
}
