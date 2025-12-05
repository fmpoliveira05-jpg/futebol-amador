using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace Api.Extensions
{
    /// <summary>
    /// Classe estática de extensão responsável pela configuração personalizada da Autenticação Firebase.
    /// 
    /// Esta classe configura o pipeline de autenticação JWT (JSON Web Token) para validar tokens emitidos pelo Google Firebase,
    /// implementando uma lógica manual de gestão de chaves públicas (JWKS - JSON Web Key Set) para garantir
    /// que a validação continua a funcionar mesmo quando a Google roda as chaves de assinatura.
    /// </summary>
    public static class FirebaseAuthenticationExtensions
    {
        /// <summary>
        /// Adiciona e configura os serviços de autenticação do Firebase ao contentor de DI.
        /// </summary>
        /// <remarks>
        /// O processo de configuração envolve:
        /// 1. Inicialização do SDK Admin do Firebase com credenciais de ficheiro.
        /// 2. Download síncrono inicial das chaves públicas da Google (JWKS).
        /// 3. Configuração do esquema de autenticação [JwtBearer].
        /// 4. Implementação de um [IssuerSigningKeyResolver] personalizado para atualizar as chaves dinamicamente se um token for assinado com uma chave nova.
        /// </remarks>
        /// <param name="services">A coleção de serviços da aplicação.</param>
        /// <param name="configuration">A configuração da aplicação (para ler o caminho das credenciais).</param>
        /// <returns>A coleção de serviços atualizada (Fluent API).</returns>
        /// <exception cref="FileNotFoundException">Lançada se o ficheiro de credenciais JSON não for encontrado no caminho especificado.</exception>
        public async static Task<IServiceCollection> AddFirebaseAuthentication(this IServiceCollection services, IConfiguration configuration)
        {

            var firebaseCredentialPath = configuration["Firebase:CredentialPath"];

            if (string.IsNullOrEmpty(firebaseCredentialPath) || !File.Exists(firebaseCredentialPath))
            {
                throw new FileNotFoundException(
                    "Ficheiro de credenciais do Firebase não encontrado.", firebaseCredentialPath);
            }

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(firebaseCredentialPath)
                });
            }

            var firebaseProjectId = "frontend-amfootball";
            var issuer = $"https://securetoken.google.com/{firebaseProjectId}";
            var jwksUri = "https://www.googleapis.com/service_accounts/v1/jwk/securetoken@system.gserviceaccount.com";

            // Faz download direto das chaves da Google
            var http = new HttpClient();
            var jwksJson = await http.GetStringAsync(jwksUri);
            var jwks = JsonDocument.Parse(jwksJson);
            var keys = new List<SecurityKey>();

            foreach (var keyElem in jwks.RootElement.GetProperty("keys").EnumerateArray())
            {
                var e = keyElem.GetProperty("e").GetString();
                var n = keyElem.GetProperty("n").GetString();
                var kid = keyElem.GetProperty("kid").GetString();

                var rsa = new System.Security.Cryptography.RSAParameters
                {
                    Exponent = Base64UrlEncoder.DecodeBytes(e),
                    Modulus = Base64UrlEncoder.DecodeBytes(n)
                };

                keys.Add(new RsaSecurityKey(rsa) { KeyId = kid });
            }

            Console.WriteLine($"[Startup] JWKS keys loaded directly: {keys.Count}");

            // Configura autenticação JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = firebaseProjectId,
                        ValidateLifetime = true,
                        RequireSignedTokens = true,
                        IssuerSigningKeys = keys
                    };

                    // Resolver dinâmico (caso o Firebase rode as chaves)
                    options.TokenValidationParameters.IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                    {
                        var match = keys.FirstOrDefault(k => k.KeyId == kid);
                        if (match != null) return new[] { match };

                        Console.WriteLine($"[Resolver] Kid {kid} não encontrado localmente. Atualizando JWKS...");
                        var newJwksJson = http.GetStringAsync(jwksUri).GetAwaiter().GetResult();
                        var newJwks = JsonDocument.Parse(newJwksJson);
                        var updatedKeys = new List<SecurityKey>();

                        foreach (var keyElem in newJwks.RootElement.GetProperty("keys").EnumerateArray())
                        {
                            var e = keyElem.GetProperty("e").GetString();
                            var n = keyElem.GetProperty("n").GetString();
                            var newKid = keyElem.GetProperty("kid").GetString();

                            var rsa = new System.Security.Cryptography.RSAParameters
                            {
                                Exponent = Base64UrlEncoder.DecodeBytes(e),
                                Modulus = Base64UrlEncoder.DecodeBytes(n)
                            };

                            updatedKeys.Add(new RsaSecurityKey(rsa) { KeyId = newKid });
                        }

                        Console.WriteLine($"[Resolver] JWKS atualizado. {updatedKeys.Count} chaves disponíveis.");
                        return updatedKeys;
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = ctx =>
                        {
                            Console.WriteLine($"[Auth Failed] {ctx.Exception.Message}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = ctx =>
                        {
                            Console.WriteLine($"[Token OK] UID: {ctx.Principal?.FindFirst("user_id")?.Value}");
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
}