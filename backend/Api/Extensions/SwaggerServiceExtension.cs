using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Api.Extensions
{
    /// <summary>
    /// Classe estática de extensão responsável pela configuração do serviço de documentação Swagger/OpenAPI.
    /// 
    /// Esta classe centraliza toda a configuração da interface de documentação interativa (Swagger UI),
    /// incluindo definições de metadados, configuração de segurança (JWT) e integração com comentários XML do código.
    /// </summary>
    public static class SwaggerServiceExtension
    {
        /// <summary>
        /// Regista e configura o gerador do Swagger no contentor de injeção de dependências.
        /// </summary>
        /// <remarks>
        /// <b>Configurações incluídas:</b>
        /// <list type="bullet">
        ///     <item><description>Metadados da API (Título e Versão).</description></item>
        ///     <item><description><b>Definição de Segurança:</b> Configura o suporte para autenticação via <b>JWT Bearer Token</b>. Adiciona o botão "Authorize" na UI.</description></item>
        ///     <item><description><b>Requisito de Segurança:</b> Define que os endpoints requerem o esquema de segurança configurado.</description></item>
        ///     <item><description><b>Comentários XML:</b> Utiliza <see cref="System.Reflection"/> para localizar e carregar o ficheiro XML gerado pelo compilador, permitindo que os comentários do código apareçam na documentação web.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="services">A coleção de serviços da aplicação.</param>
        /// <returns>A coleção de serviços atualizada, permitindo encadeamento (Fluent API).</returns>
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
                
                // 3. Configuração dos Comentários XML 
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            return services;
        }
    }
}