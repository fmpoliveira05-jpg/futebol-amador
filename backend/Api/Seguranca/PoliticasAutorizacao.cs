using Microsoft.AspNetCore.Authorization;

namespace Api.Seguranca
{
    /// <summary>
    /// Políticas de autorização da API.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Política por omissão e de recurso (<c>FallbackPolicy</c>): utilizador autenticado e, com
    /// <c>Auth:RequireVerifiedEmail</c> (por omissão <c>true</c>), com a claim <c>email_verified</c> a
    /// <c>true</c>. Um endpoint sem atributos fica protegido; os públicos têm <c>[AllowAnonymous]</c>.</item>
    /// <item><see cref="SemVerificacaoEmail"/>: só exige sessão (por exemplo, terminar sessão).</item>
    /// </list>
    /// </remarks>
    public static class PoliticasAutorizacao
    {
        /// <summary>Autenticado, mesmo sem o e-mail confirmado.</summary>
        public const string SemVerificacaoEmail = "SemVerificacaoEmail";

        /// <summary>Claim do ID token do Firebase que indica o e-mail confirmado.</summary>
        public const string ClaimEmailVerificado = "email_verified";

        public static IServiceCollection AddPoliticasAutorizacao(this IServiceCollection services, IConfiguration configuration)
        {
            var exigirEmailVerificado = configuration.GetValue("Auth:RequireVerifiedEmail", true);

            var construtor = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
            if (exigirEmailVerificado)
            {
                construtor.RequireAssertion(ctx => ctx.User.Claims.Any(c =>
                    c.Type == ClaimEmailVerificado && string.Equals(c.Value, "true", StringComparison.OrdinalIgnoreCase)));
            }
            var politicaPrincipal = construtor.Build();

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = politicaPrincipal;
                options.FallbackPolicy = politicaPrincipal;
                options.AddPolicy(SemVerificacaoEmail, p => p.RequireAuthenticatedUser());
            });

            return services;
        }
    }
}
