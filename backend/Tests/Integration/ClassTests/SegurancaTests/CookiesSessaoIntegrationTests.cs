using Api.Seguranca;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using Tests.Integration;

namespace IntegrationTests.Seguranca
{
    /// <summary>
    /// Atributos dos cookies de sessão e limpeza no logout e na eliminação da conta; respostas sem
    /// palavras-passe nem refresh tokens.
    /// </summary>
    [TestFixture]
    public class CookiesSessaoIntegrationTests
    {
        private ApiTestAppFactory factory = null!;

        [SetUp]
        public void SetUp() => factory = new ApiTestAppFactory();

        [TearDown]
        public void TearDown() => factory.Dispose();

        private HttpClient Cliente(Mock<IAuthService> auth, Mock<IContaService>? contas = null) =>
            factory.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
            {
                s.RemoveAll<IAuthService>();
                s.AddSingleton(auth.Object);
                if (contas != null)
                {
                    s.RemoveAll<IContaService>();
                    s.AddSingleton(contas.Object);
                }
            })).CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = false });

        private static void AssertCookiesApagados(HttpResponseMessage resposta)
        {
            var cookies = resposta.Headers.GetValues("Set-Cookie").ToList();
            foreach (var nome in new[] { SessaoWeb.CookieSessao, SessaoWeb.CookieRefresh })
            {
                var cookie = cookies.Single(c => c.StartsWith(nome + "="));
                Assert.That(cookie, Does.StartWith(nome + "=;"), "valor vazio");
                Assert.That(cookie, Does.Contain("expires=Thu, 01 Jan 1970").IgnoreCase, "expirado");
                Assert.That(cookie, Does.Contain("secure").IgnoreCase.And.Contain("httponly").IgnoreCase);
            }
        }

        [Test]
        public async Task Logout_Apaga_Os_Dois_Cookies()
        {
            var cliente = Cliente(new Mock<IAuthService>());
            var pedido = new HttpRequestMessage(HttpMethod.Post, "/api/User/logout");
            pedido.Headers.Add("Authorization", "Test");

            var resposta = await cliente.SendAsync(pedido);

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            AssertCookiesApagados(resposta);
        }

        [Test]
        public async Task Eliminar_Conta_Apaga_Os_Cookies()
        {
            var cliente = Cliente(new Mock<IAuthService>(), new Mock<IContaService>());
            var pedido = new HttpRequestMessage(HttpMethod.Delete, "/api/User/me") { Content = JsonContent.Create(new { password = "x" }) };
            pedido.Headers.Add("Authorization", "Test");

            var resposta = await cliente.SendAsync(pedido);

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            AssertCookiesApagados(resposta);
        }

        [Test]
        public async Task Perfil_Nunca_Devolve_Palavra_Passe_Nem_Refresh_Token()
        {
            var auth = new Mock<IAuthService>();
            auth.Setup(a => a.GetFullUserData(It.IsAny<string>())).ReturnsAsync(new LoginResponseDto { Name = "Francisco", Email = "f@exemplo.pt" });
            var cliente = Cliente(auth);
            var pedido = new HttpRequestMessage(HttpMethod.Get, "/api/User/get-profile");
            pedido.Headers.Add("Authorization", "Test");

            var corpo = await (await cliente.SendAsync(pedido)).Content.ReadAsStringAsync();

            Assert.That(corpo, Does.Not.Contain("password").IgnoreCase.And.Not.Contain("refreshToken").IgnoreCase);
        }
    }
}
