using Api.Extensions;
using Api.Middlewares;
using Api.Seguranca;
using Application.DTOs.Player;
using Application.Services;
using Application.Validators.Atributos;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using System.Security.Claims;
using System.Text;

namespace Unit.SegurancaTests
{
    /// <summary>Regras de validação e utilitários de segurança sem dependências externas.</summary>
    [TestFixture]
    public class ValidacoesSegurancaTests
    {
        #region Emblema

        [TestCase(null)]
        [TestCase("")]
        [TestCase("https://res.cloudinary.com/demo/image/upload/v1/equipas/abc/emblema.png")]
        [TestCase("data:image/png;base64,iVBORw0KGgo=")]
        [TestCase("data:image/webp;base64,UklGRg==")]
        [TestCase("data:image/jpeg;base64,/9j/4AAQ")]
        public void Emblema_Aceita_Cloudinary_E_Imagens_Embebidas(string? icone)
        {
            Assert.That(EmblemaPermitidoAttribute.Valido(icone), Is.True);
        }

        [TestCase("http://res.cloudinary.com/demo/image/upload/x.png")]
        [TestCase("https://evil.example/x.png")]
        [TestCase("https://res.cloudinary.com.evil.example/x.png")]
        [TestCase("https://user@res.cloudinary.com/x.png")]
        [TestCase("https://res.cloudinary.com:8443/x.png")]
        [TestCase("javascript:alert(1)")]
        [TestCase("data:image/svg+xml;base64,PHN2Zz4=")]
        [TestCase("data:text/html;base64,PGgxPg==")]
        [TestCase("data:image/png;base64,<script>")]
        [TestCase("iVBORw0KGgo=")]
        public void Emblema_Recusa_Outros_Esquemas_E_Anfitrioes(string icone)
        {
            Assert.That(EmblemaPermitidoAttribute.Valido(icone), Is.False);
        }

        [Test]
        public void Emblema_Recusa_Url_E_Imagem_Demasiado_Grandes()
        {
            var urlLonga = "https://res.cloudinary.com/" + new string('a', 600);
            var imagemGrande = "data:image/png;base64," + new string('A', 200_000);

            Assert.That(EmblemaPermitidoAttribute.Valido(urlLonga), Is.False);
            Assert.That(EmblemaPermitidoAttribute.Valido(imagemGrande), Is.False);
        }

        #endregion

        #region Palavra-passe

        [TestCase("Futebol#2026")]
        [TestCase("uma Frase longa com 1 símbolo!")]
        public void PalavraPasse_Aceita_Palavras_Passe_Fortes(string palavraPasse)
        {
            Assert.That(PalavraPasseSeguraAttribute.Cumpre(palavraPasse), Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("Curta#1")]
        [TestCase("semmaiusculas#1")]
        [TestCase("SEMMINUSCULAS#1")]
        [TestCase("SemAlgarismos#")]
        [TestCase("SemSimbolos123")]
        public void PalavraPasse_Recusa_Palavras_Passe_Fracas(string? palavraPasse)
        {
            Assert.That(PalavraPasseSeguraAttribute.Cumpre(palavraPasse), Is.False);
        }

        [Test]
        public void PalavraPasse_Recusa_Mais_De_128_Caracteres()
        {
            Assert.That(PalavraPasseSeguraAttribute.Cumpre("Aa1#" + new string('x', 125)), Is.False);
            Assert.That(PalavraPasseSeguraAttribute.Cumpre("Aa1#" + new string('x', 124)), Is.True);
        }

        #endregion

        #region Dados pessoais

        [TestCase("Rua de Santo António, 12, Guimarães", "Guimarães")]
        [TestCase("Avenida da Liberdade 100, Braga", "Braga")]
        [TestCase("Porto", "Porto")]
        [TestCase("Rua Sem Localidade 25", "")]
        [TestCase("", "")]
        [TestCase(null, "")]
        public void Zona_Mostra_So_A_Localidade(string? morada, string esperado)
        {
            Assert.That(Zona.DaMorada(morada), Is.EqualTo(esperado));
        }

        #endregion

        #region Tokens

        private static string Token(string payloadJson)
        {
            static string B64(string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return $"{B64("{\"alg\":\"RS256\"}")}.{B64(payloadJson)}.assinatura";
        }

        [Test]
        public void EmailVerificado_Le_A_Claim_Do_Id_Token()
        {
            Assert.That(FireBaseAuthService.EmailVerificadoNoToken(Token("{\"email_verified\":true,\"sub\":\"u1\"}")), Is.True);
            Assert.That(FireBaseAuthService.EmailVerificadoNoToken(Token("{\"email_verified\":false}")), Is.False);
            Assert.That(FireBaseAuthService.EmailVerificadoNoToken(Token("{\"sub\":\"u1\"}")), Is.False);
            Assert.That(FireBaseAuthService.EmailVerificadoNoToken("nao-e-um-jwt"), Is.False);
            Assert.That(FireBaseAuthService.EmailVerificadoNoToken(null), Is.False);
        }

        [Test]
        public void TokenDoPedido_Usa_O_Cookie_So_Sem_Cabecalho_Authorization()
        {
            var comCookie = new DefaultHttpContext();
            comCookie.Request.Headers.Cookie = $"{SessaoWeb.CookieSessao}=token-do-cookie";
            Assert.That(FirebaseAuthenticationExtensions.TokenDoPedido(comCookie.Request), Is.EqualTo("token-do-cookie"));

            var comCabecalho = new DefaultHttpContext();
            comCabecalho.Request.Headers.Cookie = $"{SessaoWeb.CookieSessao}=token-do-cookie";
            comCabecalho.Request.Headers.Authorization = "Bearer token-da-app";
            Assert.That(FirebaseAuthenticationExtensions.TokenDoPedido(comCabecalho.Request), Is.Null);

            var hub = new DefaultHttpContext();
            hub.Request.Path = "/Notification";
            hub.Request.QueryString = new QueryString("?access_token=token-do-hub");
            Assert.That(FirebaseAuthenticationExtensions.TokenDoPedido(hub.Request), Is.EqualTo("token-do-hub"));

            var queryForaDosHubs = new DefaultHttpContext();
            queryForaDosHubs.Request.Path = "/api/Team/1";
            queryForaDosHubs.Request.QueryString = new QueryString("?access_token=token");
            Assert.That(FirebaseAuthenticationExtensions.TokenDoPedido(queryForaDosHubs.Request), Is.Null);
        }

        [Test]
        public void AuthTime_Le_Os_Dois_Nomes_Da_Claim()
        {
            var original = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("auth_time", "1700000000") }));
            var mapeada = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.AuthenticationInstant, "1700000001") }));

            Assert.That(FirebaseAuthenticationExtensions.LerAuthTime(original), Is.EqualTo(1700000000));
            Assert.That(FirebaseAuthenticationExtensions.LerAuthTime(mapeada), Is.EqualTo(1700000001));
            Assert.That(FirebaseAuthenticationExtensions.LerAuthTime(new ClaimsPrincipal()), Is.Null);
        }

        #endregion

        #region Erros

        [Test]
        public void Excecoes_Da_Framework_Nao_Expoem_A_Mensagem()
        {
            Exception daFramework;
            try
            {
                _ = new List<int>().First();
                throw new InvalidOperationException("não chega aqui");
            }
            catch (InvalidOperationException ex)
            {
                daFramework = ex;
            }

            Assert.That(GlobalExceptionHandler.MensagemDaAplicacao(daFramework), Is.False);
            Assert.That(GlobalExceptionHandler.MensagemDaAplicacao(new Domain.Exceptions.ValidationException("x")), Is.True);
        }

        #endregion

        #region Cloudinary

        [Test]
        public void Assinatura_Cloudinary_Segue_O_Algoritmo_Documentado()
        {
            // Exemplo da documentação do Cloudinary ("Generating authentication signatures").
            var parametros = new Dictionary<string, string>
            {
                ["timestamp"] = "1315060510",
                ["public_id"] = "sample_image",
                ["eager"] = "w_400,h_300,c_pad|w_260,h_200,c_crop",
            };

            Assert.That(AssinaturaCloudinary.Assinatura(parametros, "abcd"), Is.EqualTo("bfd09f95f331f558cbd1320e67aa8d488770583e"));
        }

        #endregion
    }
}
