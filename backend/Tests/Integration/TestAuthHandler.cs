using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;

namespace Tests.Integration
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string TestUserId = "test-user-id-12345";

        /// <summary>
        /// Valor do cabeçalho Authorization para um utilizador com o e-mail ainda por confirmar
        /// (sem a claim email_verified=true).
        /// </summary>
        public const string EmailNaoVerificado = "Test-NaoVerificado";

        /// <summary>
        /// Prefixo para autenticar outro utilizador: <c>Authorization: Test-Uid outro-uid</c>
        /// (testes com vários utilizadores, por exemplo pedidos simultâneos).
        /// </summary>
        public const string PrefixoUid = "Test-Uid ";

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger, 
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            string authorizationHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Test"))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var uid = authorizationHeader.StartsWith(PrefixoUid, StringComparison.Ordinal)
                ? authorizationHeader.Substring(PrefixoUid.Length).Trim()
                : TestUserId;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, uid),
                new Claim(ClaimTypes.Name, "TestUser")
            };

            // Como nos ID tokens do Firebase: a política por omissão exige email_verified=true.
            if (authorizationHeader != EmailNaoVerificado)
            {
                claims.Add(new Claim("email_verified", "true"));
            }
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
