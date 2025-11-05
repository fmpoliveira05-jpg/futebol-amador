using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Api.Extensions
{
    public static class FirebaseAuthenticationExtensions
    {
        public static IServiceCollection AddFirebaseAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
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


            var firebaseProjectId = configuration["Firebase:ProjectId"];
            if (string.IsNullOrEmpty(firebaseProjectId))
            {
                throw new ArgumentNullException("Firebase:ProjectId", "O ProjectId do Firebase não pode ser nulo na configuração.");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = $"https://securetoken.google.com/{firebaseProjectId}",
                        ValidateAudience = true,
                        ValidAudience = firebaseProjectId,
                        ValidateLifetime = true

                    };
                });

            return services;
        }
    }
}