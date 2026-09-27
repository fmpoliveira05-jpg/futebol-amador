using Api.Middlewares;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Net;
using System.Text.Json;
using Tests.Integration;

namespace IntegrationTests.Producao
{
    /// <summary>
    /// Respostas de erro consistentes: 404, 405 e 401 como ProblemDetails em português, sem
    /// detalhes internos; conflitos de concorrência como 409.
    /// </summary>
    [TestFixture]
    public class RespostasErroIntegrationTests
    {
        private ApiTestAppFactory factory = null!;
        private HttpClient cliente = null!;

        [OneTimeSetUp]
        public void SetUp()
        {
            factory = new ApiTestAppFactory();
            cliente = factory.CreateClient();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            cliente.Dispose();
            factory.Dispose();
        }

        private static async Task<JsonElement> Problema(HttpResponseMessage resposta)
        {
            Assert.That(resposta.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement;
        }

        [Test]
        public async Task Rota_Inexistente_Responde_404_ProblemDetails()
        {
            // Sem sessão a resposta é 401 (a política por omissão exige sessão e não revela que rotas existem).
            var pedido = new HttpRequestMessage(HttpMethod.Get, "/api/nao-existe");
            pedido.Headers.Add("Authorization", "Test");
            var resposta = await cliente.SendAsync(pedido);

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            var corpo = await Problema(resposta);
            Assert.That(corpo.GetProperty("title").GetString(), Is.EqualTo("Recurso não encontrado"));
            Assert.That(corpo.GetProperty("status").GetInt32(), Is.EqualTo(404));
            Assert.That(corpo.TryGetProperty("traceId", out _), Is.True);
        }

        [Test]
        public async Task Metodo_Errado_Responde_405_ProblemDetails()
        {
            var pedido = new HttpRequestMessage(HttpMethod.Delete, "/api/leagues");
            pedido.Headers.Add("Authorization", "Test");
            var resposta = await cliente.SendAsync(pedido);

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.MethodNotAllowed));
            var corpo = await Problema(resposta);
            Assert.That(corpo.GetProperty("title").GetString(), Is.EqualTo("Método não permitido"));
        }

        [Test]
        public async Task Sem_Sessao_Responde_401_ProblemDetails()
        {
            var resposta = await cliente.GetAsync("/api/Chat/my-rooms");

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            var corpo = await Problema(resposta);
            Assert.That(corpo.GetProperty("title").GetString(), Is.EqualTo("Não autenticado"));
        }

        [Test]
        public void Concorrencia_Otimista_E_Classificada_Como_409()
        {
            var (estado, _) = GlobalExceptionHandler.Classificar(new DbUpdateConcurrencyException("x"));
            Assert.That(estado, Is.EqualTo(409));
        }

        [Test]
        public void Servico_Externo_Lento_E_Classificado_Como_503()
        {
            var (estado, _) = GlobalExceptionHandler.Classificar(new Polly.Timeout.TimeoutRejectedException());
            Assert.That(estado, Is.EqualTo(503));
        }

        [Test]
        public void Corpo_Demasiado_Grande_E_413()
        {
            var (estado, _) = GlobalExceptionHandler.Classificar(new Microsoft.AspNetCore.Http.BadHttpRequestException("grande", 413));
            Assert.That(estado, Is.EqualTo(413));
        }

        [Test]
        public void Erro_Desconhecido_Continua_500_Sem_Mensagem()
        {
            var (estado, titulo) = GlobalExceptionHandler.Classificar(new NullReferenceException("detalhe interno"));
            Assert.That(estado, Is.EqualTo(500));
            Assert.That(titulo, Does.Not.Contain("detalhe interno"));
        }
    }
}
