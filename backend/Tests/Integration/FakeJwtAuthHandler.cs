using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Api.IntegrationTests.Fixtures
{
    /// <summary>
    /// Handler fake para simular autenticação JWT nos testes de integração.
    /// Garante que o HttpContext.User tem o ClaimTypes.NameIdentifier
    /// que o controller PostPoneMatchController espera.
    /// </summary>
    public class FakeJwtAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public FakeJwtAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Claims simulados — correspondem ao que o controller espera
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Name, "Integration Test User"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "FakeJwt");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "FakeJwt");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}