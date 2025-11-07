using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Authentication;

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

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var projectId = firebaseProjectId;
                    options.Authority = $"https://securetoken.google.com/{projectId}";
                    options.Audience = projectId;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = $"https://securetoken.google.com/{projectId}",
                        ValidateAudience = true,
                        ValidAudience = projectId,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(5),
                        RequireSignedTokens = true,
                        RequireExpirationTime = true
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("JWT Auth Failed: " + context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            Console.WriteLine("JWT Challenge: " + context.ErrorDescription);
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            Console.WriteLine("JWT Received");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("JWT Validated for user: " +
                                context.Principal?.FindFirst("user_id")?.Value);
                            return Task.CompletedTask;
                        }
                    };
                    });

            return services;
        }
    }
}