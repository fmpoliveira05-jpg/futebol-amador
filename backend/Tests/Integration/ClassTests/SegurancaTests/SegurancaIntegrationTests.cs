using Api.Seguranca;
using Application.DTOs;
using Application.Interfaces.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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
    /// Limitação de pedidos, cabeçalhos de segurança, sessão web em cookies, CSRF, confirmação do
    /// e-mail e Turnstile, através do pipeline HTTP completo.
    /// </summary>
    [TestFixture]
    public class SegurancaIntegrationTests
    {
        private const string OrigemWeb = "http://localhost:4200";

        private ApiTestAppFactory factory = null!;

        [SetUp]
        public void SetUp() => factory = new ApiTestAppFactory();

        [TearDown]
        public void TearDown() => factory.Dispose();

        private static LoginResponseDto RespostaLogin() => new()
        {
            Name = "Francisco",
            Email = "f@exemplo.pt",
            FirebaseLoginResponseDto = new FirebaseLoginResponseDto
            {
                IdToken = "id-token-secreto",
                RefreshToken = "refresh-token-secreto",
                ExpiresIn = "3600",
                LocalId = "uid-1",
                Email = "f@exemplo.pt",
            },
        };

        private HttpClient Cliente(Mock<IAuthService> auth, Action<IWebHostBuilder>? extra = null, string baseAddress = "http://localhost")
        {
            return factory.WithWebHostBuilder(builder =>
            {
                extra?.Invoke(builder);
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IAuthService>();
                    services.AddSingleton(auth.Object);
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri(baseAddress), HandleCookies = false });
        }

        private static HttpRequestMessage PedidoLogin(string? origem = null)
        {
            var pedido = new HttpRequestMessage(HttpMethod.Post, "/api/User/login")
            {
                Content = JsonContent.Create(new LoginDto { Email = "f@exemplo.pt", Password = "Futebol#2026" }),
            };
            if (origem != null)
            {
                pedido.Headers.Add("Origin", origem);
                pedido.Headers.Add(SessaoWeb.CabecalhoCsrf, SessaoWeb.ValorCsrf);
            }
            return pedido;
        }

        #region Limitação de pedidos

        [Test]
        public async Task Login_Responde_429_Com_RetryAfter_Depois_Do_Limite()
        {
            var auth = new Mock<IAuthService>();
            auth.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(RespostaLogin());
            var cliente = Cliente(auth, b => b.UseSetting("LimitacaoPedidos:Autenticacao:Pedidos", "2"));

            var primeiro = await cliente.SendAsync(PedidoLogin());
            var segundo = await cliente.SendAsync(PedidoLogin());
            var terceiro = await cliente.SendAsync(PedidoLogin());

            Assert.That(primeiro.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(segundo.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(terceiro.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(terceiro.Headers.RetryAfter, Is.Not.Null);
            auth.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public async Task Limite_Global_Aplica_Se_A_Todos_Os_Endpoints()
        {
            var cliente = Cliente(new Mock<IAuthService>(), b => b.UseSetting("LimitacaoPedidos:Global:Pedidos", "3"));

            var estados = new List<HttpStatusCode>();
            for (var i = 0; i < 4; i++)
            {
                estados.Add((await cliente.GetAsync("/api/Chat/my-rooms")).StatusCode);
            }

            Assert.That(estados.Take(3), Is.All.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(estados[3], Is.EqualTo(HttpStatusCode.TooManyRequests));
        }

        #endregion

        #region Cabeçalhos

        [Test]
        public async Task Respostas_Levam_Cabecalhos_De_Seguranca_E_Nao_Anunciam_O_Servidor()
        {
            var cliente = Cliente(new Mock<IAuthService>());

            var resposta = await cliente.GetAsync("/api/Chat/my-rooms");

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(resposta.Headers.GetValues("Content-Security-Policy").Single(), Does.Contain("default-src 'none'").And.Contain("frame-ancestors 'none'"));
            Assert.That(resposta.Headers.GetValues("X-Content-Type-Options").Single(), Is.EqualTo("nosniff"));
            Assert.That(resposta.Headers.GetValues("X-Frame-Options").Single(), Is.EqualTo("DENY"));
            Assert.That(resposta.Headers.GetValues("Referrer-Policy").Single(), Is.EqualTo("no-referrer"));
            Assert.That(resposta.Headers.Contains("Permissions-Policy"), Is.True);
            Assert.That(resposta.Headers.GetValues("Cross-Origin-Opener-Policy").Single(), Is.EqualTo("same-origin"));
            Assert.That(resposta.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(resposta.Headers.Contains("Server"), Is.False);
        }

        [Test]
        public async Task Https_Leva_Hsts()
        {
            var cliente = Cliente(new Mock<IAuthService>(), baseAddress: "https://api.exemplo.pt");

            var resposta = await cliente.GetAsync("/api/Chat/my-rooms");

            Assert.That(resposta.Headers.GetValues("Strict-Transport-Security").Single(), Does.Contain("max-age=31536000"));
        }

        #endregion

        #region Sessão web e CSRF

        [Test]
        public async Task Login_Do_Browser_Fica_Em_Cookies_HttpOnly_Sem_Tokens_No_Corpo()
        {
            var auth = new Mock<IAuthService>();
            auth.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(RespostaLogin());
            var cliente = Cliente(auth);

            var resposta = await cliente.SendAsync(PedidoLogin(OrigemWeb));
            var corpo = await resposta.Content.ReadAsStringAsync();
            var cookies = resposta.Headers.GetValues("Set-Cookie").ToList();

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(corpo, Does.Not.Contain("id-token-secreto").And.Not.Contain("refresh-token-secreto"));
            Assert.That(corpo, Does.Contain("uid-1"));

            var sessao = cookies.Single(c => c.StartsWith(SessaoWeb.CookieSessao + "="));
            Assert.That(sessao, Does.Contain("httponly").IgnoreCase.And.Contain("secure").IgnoreCase
                .And.Contain("samesite=strict").IgnoreCase.And.Contain("path=/;").IgnoreCase);

            var refresh = cookies.Single(c => c.StartsWith(SessaoWeb.CookieRefresh + "="));
            Assert.That(refresh, Does.Contain("path=/api/User/refresh").IgnoreCase.And.Contain("httponly").IgnoreCase);
        }

        [Test]
        public async Task Login_Da_App_Recebe_Os_Tokens_No_Corpo_Sem_Cookies()
        {
            var auth = new Mock<IAuthService>();
            auth.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(RespostaLogin());
            var cliente = Cliente(auth);

            var resposta = await cliente.SendAsync(PedidoLogin());

            Assert.That(await resposta.Content.ReadAsStringAsync(), Does.Contain("id-token-secreto"));
            Assert.That(resposta.Headers.Contains("Set-Cookie"), Is.False);
        }

        [Test]
        public async Task Pedido_Com_Cookie_Sem_Cabecalho_Csrf_E_Recusado()
        {
            var auth = new Mock<IAuthService>();
            var cliente = Cliente(auth);

            var pedido = new HttpRequestMessage(HttpMethod.Post, "/api/User/logout");
            pedido.Headers.Add("Cookie", $"{SessaoWeb.CookieSessao}=token");
            pedido.Headers.Add("Origin", OrigemWeb);

            var resposta = await cliente.SendAsync(pedido);

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            auth.Verify(a => a.LogoutAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Pedido_Com_Cookie_De_Outra_Origem_E_Recusado()
        {
            var cliente = Cliente(new Mock<IAuthService>());

            var pedido = new HttpRequestMessage(HttpMethod.Post, "/api/User/logout");
            pedido.Headers.Add("Cookie", $"{SessaoWeb.CookieSessao}=token");
            pedido.Headers.Add("Origin", "https://site-malicioso.example");
            pedido.Headers.Add(SessaoWeb.CabecalhoCsrf, SessaoWeb.ValorCsrf);

            Assert.That((await cliente.SendAsync(pedido)).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task Pedido_Com_Cookie_Cabecalho_E_Origem_Certos_Passa_A_Protecao()
        {
            var cliente = Cliente(new Mock<IAuthService>());

            var pedido = new HttpRequestMessage(HttpMethod.Post, "/api/User/logout");
            pedido.Headers.Add("Cookie", $"{SessaoWeb.CookieSessao}=token");
            pedido.Headers.Add("Origin", OrigemWeb);
            pedido.Headers.Add(SessaoWeb.CabecalhoCsrf, SessaoWeb.ValorCsrf);

            // Nos testes a autenticação não lê cookies, por isso chega ao 401 (e não ao 403 do CSRF).
            Assert.That((await cliente.SendAsync(pedido)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Refresh_Sem_Cookie_Da_401()
        {
            var cliente = Cliente(new Mock<IAuthService>());

            Assert.That((await cliente.PostAsync("/api/User/refresh", null)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Refresh_Com_Cookie_Renova_A_Sessao()
        {
            var auth = new Mock<IAuthService>();
            auth.Setup(a => a.RenovarSessaoAsync("refresh-antigo")).ReturnsAsync(RespostaLogin().FirebaseLoginResponseDto);
            var cliente = Cliente(auth);

            var pedido = new HttpRequestMessage(HttpMethod.Post, "/api/User/refresh");
            pedido.Headers.Add("Cookie", $"{SessaoWeb.CookieRefresh}=refresh-antigo");
            pedido.Headers.Add("Origin", OrigemWeb);
            pedido.Headers.Add(SessaoWeb.CabecalhoCsrf, SessaoWeb.ValorCsrf);

            var resposta = await cliente.SendAsync(pedido);

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(resposta.Headers.GetValues("Set-Cookie").Any(c => c.StartsWith(SessaoWeb.CookieSessao + "=id-token-secreto")), Is.True);
        }

        [Test]
        public async Task Refresh_Revogado_Apaga_Os_Cookies()
        {
            var auth = new Mock<IAuthService>();
            auth.Setup(a => a.RenovarSessaoAsync(It.IsAny<string>())).ThrowsAsync(new UnauthorizedAccessException());
            var cliente = Cliente(auth);

            var pedido = new HttpRequestMessage(HttpMethod.Post, "/api/User/refresh");
            pedido.Headers.Add("Cookie", $"{SessaoWeb.CookieRefresh}=revogado");
            pedido.Headers.Add("Origin", OrigemWeb);
            pedido.Headers.Add(SessaoWeb.CabecalhoCsrf, SessaoWeb.ValorCsrf);

            var resposta = await cliente.SendAsync(pedido);

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(resposta.Headers.GetValues("Set-Cookie").Any(c => c.StartsWith(SessaoWeb.CookieSessao + "=;")), Is.True);
        }

        #endregion

        #region Confirmação do e-mail e respostas genéricas

        [Test]
        public async Task Login_Sem_Email_Confirmado_Da_403_Com_Codigo()
        {
            var auth = new Mock<IAuthService>();
            auth.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new EmailNaoVerificadoException());
            var cliente = Cliente(auth);

            var resposta = await cliente.SendAsync(PedidoLogin());

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(await resposta.Content.ReadAsStringAsync(), Does.Contain("email_nao_verificado"));
        }

        [Test]
        public async Task Sessao_Sem_Email_Confirmado_So_Pode_Terminar_A_Sessao()
        {
            var auth = new Mock<IAuthService>();
            var cliente = Cliente(auth);
            cliente.DefaultRequestHeaders.Add("Authorization", TestAuthHandler.EmailNaoVerificado);

            var perfil = await cliente.GetAsync("/api/User/get-profile");
            var logout = await cliente.PostAsync("/api/User/logout", null);

            Assert.That(perfil.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(logout.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            auth.Verify(a => a.LogoutAsync(TestAuthHandler.TestUserId), Times.Once);
        }

        [Test]
        public async Task Com_A_Exigencia_Desligada_A_Sessao_Sem_Email_Confirmado_E_Aceite()
        {
            var auth = new Mock<IAuthService>();
            auth.Setup(a => a.GetFullUserData(TestAuthHandler.TestUserId)).ReturnsAsync(new LoginResponseDto { Name = "X" });
            var cliente = Cliente(auth, b => b.UseSetting("Auth:RequireVerifiedEmail", "false"));
            cliente.DefaultRequestHeaders.Add("Authorization", TestAuthHandler.EmailNaoVerificado);

            Assert.That((await cliente.GetAsync("/api/User/get-profile")).StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Reenvio_E_Recuperacao_Respondem_Sempre_O_Mesmo()
        {
            var auth = new Mock<IAuthService>();
            var cliente = Cliente(auth);

            var reenvio = await cliente.PostAsJsonAsync("/api/User/resend-verification",
                new ReenviarVerificacaoDto { Email = "nao.existe@exemplo.pt", Password = "Qualquer#123" });
            var recuperacao = await cliente.PostAsJsonAsync("/api/User/forgot-password",
                new RecuperarPalavraPasseDto { Email = "nao.existe@exemplo.pt" });

            Assert.That(reenvio.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(recuperacao.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(await reenvio.Content.ReadAsStringAsync(), Is.EqualTo(await recuperacao.Content.ReadAsStringAsync()));
            auth.Verify(a => a.PedirRecuperacaoPalavraPasseAsync("nao.existe@exemplo.pt"), Times.Once);
        }

        [Test]
        public async Task Endpoints_Publicos_Continuam_Anonimos_E_Os_Outros_Exigem_Sessao()
        {
            var cliente = Cliente(new Mock<IAuthService>());

            Assert.That((await cliente.GetAsync("/api/lineups/formations")).StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That((await cliente.GetAsync("/api/leagues")).StatusCode, Is.Not.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That((await cliente.PostAsync("/api/uploads/signature", null)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        #endregion

        #region Turnstile

        private HttpClient ClienteComTurnstile(Mock<IAuthService> auth, bool tokenValido)
        {
            var turnstile = new Mock<IVerificadorTurnstile>();
            turnstile.SetupGet(t => t.Ativo).Returns(true);
            turnstile.Setup(t => t.VerificarAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(tokenValido);

            return Cliente(auth, b => b.ConfigureTestServices(s =>
            {
                s.RemoveAll<IVerificadorTurnstile>();
                s.AddSingleton(turnstile.Object);
            }));
        }

        [Test]
        public async Task Login_Web_Sem_Turnstile_Valido_E_Recusado()
        {
            var auth = new Mock<IAuthService>();
            var cliente = ClienteComTurnstile(auth, tokenValido: false);

            var semToken = await cliente.SendAsync(PedidoLogin(OrigemWeb));
            var comTokenInvalido = PedidoLogin(OrigemWeb);
            comTokenInvalido.Headers.Add(ExigirTurnstileAttribute.Cabecalho, "token-falso");

            Assert.That(semToken.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That((await cliente.SendAsync(comTokenInvalido)).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            auth.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Login_Web_Com_Turnstile_Valido_E_Login_Da_App_Passam()
        {
            var auth = new Mock<IAuthService>();
            auth.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(RespostaLogin());
            var cliente = ClienteComTurnstile(auth, tokenValido: true);

            var web = PedidoLogin(OrigemWeb);
            web.Headers.Add(ExigirTurnstileAttribute.Cabecalho, "token-bom");

            Assert.That((await cliente.SendAsync(web)).StatusCode, Is.EqualTo(HttpStatusCode.OK));
            // A app Android não envia Origin nem Turnstile.
            Assert.That((await cliente.SendAsync(PedidoLogin())).StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        #endregion

        #region Uploads e emblemas

        [Test]
        public async Task Assinatura_De_Upload_Sem_Configuracao_Da_503_E_Com_Configuracao_Fica_Na_Pasta_Do_Utilizador()
        {
            var semConfiguracao = Cliente(new Mock<IAuthService>());
            semConfiguracao.DefaultRequestHeaders.Add("Authorization", "Test");
            Assert.That((await semConfiguracao.PostAsync("/api/uploads/signature", null)).StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));

            var configurado = Cliente(new Mock<IAuthService>(), b =>
            {
                b.UseSetting("Cloudinary:CloudName", "demo");
                b.UseSetting("Cloudinary:ApiKey", "123");
                b.UseSetting("Cloudinary:ApiSecret", "segredo");
            });
            configurado.DefaultRequestHeaders.Add("Authorization", "Test");

            var resposta = await configurado.PostAsync("/api/uploads/signature", null);
            var assinatura = await resposta.Content.ReadFromJsonAsync<AssinaturaUploadDto>();

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(assinatura!.Folder, Is.EqualTo("equipas/testuserid12345"));
            Assert.That(assinatura.AllowedFormats, Is.EqualTo("jpg,png,webp"));
            Assert.That(assinatura.Signature, Is.EqualTo(AssinaturaCloudinary.Assinatura(new Dictionary<string, string>
            {
                ["allowed_formats"] = assinatura.AllowedFormats,
                ["folder"] = assinatura.Folder,
                ["timestamp"] = assinatura.Timestamp.ToString(),
                ["upload_preset"] = assinatura.UploadPreset,
            }, "segredo")));
            Assert.That(await resposta.Content.ReadAsStringAsync(), Does.Not.Contain("segredo"));
        }

        [Test]
        public async Task Equipa_Com_Emblema_De_Outro_Anfitriao_E_Recusada()
        {
            var cliente = Cliente(new Mock<IAuthService>());
            cliente.DefaultRequestHeaders.Add("Authorization", "Test");

            var resposta = await cliente.PostAsJsonAsync("/api/Team", new
            {
                name = "Equipa Teste",
                icon = "https://rastreio.example/pixel.png",
                homePitch = new { name = "Campo", address = "Rua A, Guimarães" },
            });

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await resposta.Content.ReadAsStringAsync(), Does.Contain("emblema"));
        }

        #endregion
    }
}
