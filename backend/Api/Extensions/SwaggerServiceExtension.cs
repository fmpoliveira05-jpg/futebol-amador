using Microsoft.OpenApi.Models;

namespace Api.Extensions
{
    public static class SwaggerServiceExtension
    {
        public static IServiceCollection AddSwaggerDocumentacion(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                // Define a informação básica do Swagger
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Api",
                    Version = "v1"
                });

                // 1. Definir o esquema de segurança (Security Scheme) que o Swagger vai usar
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Autenticação JWT (Bearer). Insira apenas o seu token de acesso."
                });

                // 2. Tornar o esquema de segurança obrigatório para os endpoints
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            return services;
        }
    }
}
