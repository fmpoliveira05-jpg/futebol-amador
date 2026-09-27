using Microsoft.AspNetCore.Hosting;
using NUnit.Framework;
using System.Net;
using System.Text.Json;
using Tests.Integration;

namespace IntegrationTests.Producao
{
    /// <summary>/health/live e /health/ready: anónimos, sem detalhes e fora da limitação de pedidos.</summary>
    [TestFixture]
    public class SaudeIntegrationTests
    {
        private ApiTestAppFactory factory = null!;

        [SetUp]
        public void SetUp() => factory = new ApiTestAppFactory();

        [TearDown]
        public void TearDown() => factory.Dispose();

        [TestCase("/health/live")]
        [TestCase("/health/ready")]
        public async Task Responde_200_Sem_Sessao(string caminho)
        {
            var cliente = factory.CreateClient();

            var resposta = await cliente.GetAsync(caminho);
            var corpo = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement;

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(corpo.GetProperty("estado").GetString(), Is.EqualTo("Healthy"));
            Assert.That(resposta.Headers.CacheControl?.NoStore, Is.True);
        }

        [Test]
        public async Task Ready_Lista_As_Verificacoes_Sem_Detalhes()
        {
            var cliente = factory.CreateClient();

            var texto = await (await cliente.GetAsync("/health/ready")).Content.ReadAsStringAsync();
            var verificacoes = JsonDocument.Parse(texto).RootElement.GetProperty("verificacoes");

            Assert.That(verificacoes.GetProperty("base-dados").GetString(), Is.EqualTo("Healthy"));
            Assert.That(verificacoes.GetProperty("firebase-configuracao").GetString(), Is.EqualTo("Healthy"));
            Assert.That(texto, Does.Not.Contain("exception").IgnoreCase.And.Not.Contain("Server="));
        }

        [Test]
        public async Task Nao_Conta_Para_A_Limitacao_De_Pedidos()
        {
            var cliente = factory.WithWebHostBuilder(b => b.UseSetting("LimitacaoPedidos:Global:Pedidos", "2")).CreateClient();

            var estados = new List<HttpStatusCode>();
            for (var i = 0; i < 5; i++)
            {
                estados.Add((await cliente.GetAsync("/health/live")).StatusCode);
            }

            Assert.That(estados, Is.All.EqualTo(HttpStatusCode.OK));
        }
    }
}
