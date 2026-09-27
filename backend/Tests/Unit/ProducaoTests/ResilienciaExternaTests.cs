using Api.Operacao;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Polly.Timeout;
using System.Net;

namespace UnitTests.Producao
{
    /// <summary>
    /// Pedidos a serviços externos: novas tentativas só em métodos idempotentes e tempo máximo por
    /// tentativa (Limites.AddResilienciaExterna).
    /// </summary>
    [TestFixture]
    public class ResilienciaExternaTests
    {
        private sealed class Contador : HttpMessageHandler
        {
            public int Pedidos;
            public HttpStatusCode Estado = HttpStatusCode.ServiceUnavailable;
            public TimeSpan Atraso = TimeSpan.Zero;

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Interlocked.Increment(ref Pedidos);
                if (Atraso > TimeSpan.Zero)
                {
                    await Task.Delay(Atraso, cancellationToken);
                }
                return new HttpResponseMessage(Estado);
            }
        }

        private static (HttpClient Cliente, Contador Servidor) Criar(Dictionary<string, string?>? extra = null)
        {
            var valores = new Dictionary<string, string?>
            {
                ["Limites:Externos:TimeoutTentativaSegundos"] = "1",
                ["Limites:Externos:TimeoutTotalSegundos"] = "10",
            };
            foreach (var (k, v) in extra ?? new())
            {
                valores[k] = v;
            }
            var configuracao = new ConfigurationBuilder().AddInMemoryCollection(valores).Build();
            var servidor = new Contador();

            var services = new ServiceCollection();
            services.AddHttpClient("externo", c => c.BaseAddress = new Uri("https://externo.test/"))
                .ConfigurePrimaryHttpMessageHandler(() => servidor)
                .AddResilienciaExterna(configuracao);

            var cliente = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>().CreateClient("externo");
            return (cliente, servidor);
        }

        [Test]
        public async Task Get_Com_Erro_Transitorio_E_Repetido()
        {
            var (cliente, servidor) = Criar();

            var resposta = await cliente.GetAsync("recurso");

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
            Assert.That(servidor.Pedidos, Is.EqualTo(3), "1 pedido + 2 novas tentativas");
        }

        [Test]
        public async Task Post_Nunca_E_Repetido()
        {
            var (cliente, servidor) = Criar();

            var resposta = await cliente.PostAsync("enviar-email", new StringContent("{}"));

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
            Assert.That(servidor.Pedidos, Is.EqualTo(1));
        }

        [Test]
        public void Servico_Lento_E_Cortado_Pelo_Timeout()
        {
            var (cliente, servidor) = Criar();
            servidor.Atraso = TimeSpan.FromSeconds(30);

            Assert.ThrowsAsync<TimeoutRejectedException>(() => cliente.PostAsync("lento", new StringContent("{}")));
        }
    }
}
