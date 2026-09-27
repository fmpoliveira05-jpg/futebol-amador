using Application.DTOs.Chat;
using Application.DTOs.Competition;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using Tests.Integration;

namespace IntegrationTests.Producao
{
    /// <summary>Cache das consultas públicas: servida da memória, invalidada por escritas.</summary>
    [TestFixture]
    public class CachePublicaIntegrationTests
    {
        private ApiTestAppFactory factory = null!;
        private Mock<ILeagueService> ligas = null!;
        private HttpClient cliente = null!;

        [SetUp]
        public void SetUp()
        {
            ligas = new Mock<ILeagueService>();
            ligas.Setup(l => l.GetLeaguesAsync()).ReturnsAsync(new List<LeagueDto>());
            var chat = new Mock<IChatRoomService>();
            chat.Setup(c => c.CreateRoomAsync(It.IsAny<CreateChatRoomDto>(), It.IsAny<string>())).ReturnsAsync("sala");

            factory = new ApiTestAppFactory();
            cliente = factory.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
            {
                s.RemoveAll<ILeagueService>();
                s.AddSingleton(ligas.Object);
                s.RemoveAll<IChatRoomService>();
                s.AddSingleton(chat.Object);
            })).CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            cliente.Dispose();
            factory.Dispose();
        }

        [Test]
        public async Task Segundo_Pedido_Vem_Da_Cache_Com_CacheControl_Publico()
        {
            var primeiro = await cliente.GetAsync("/api/leagues");
            var segundo = await cliente.GetAsync("/api/leagues");

            Assert.That(primeiro.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(segundo.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            ligas.Verify(l => l.GetLeaguesAsync(), Times.Once);
            Assert.That(segundo.Headers.CacheControl?.Public, Is.True);
            Assert.That(segundo.Headers.CacheControl?.MaxAge, Is.EqualTo(TimeSpan.FromSeconds(60)));
        }

        [Test]
        public async Task Pedido_Autenticado_Tambem_Usa_A_Cache()
        {
            await cliente.GetAsync("/api/leagues");
            var pedido = new HttpRequestMessage(HttpMethod.Get, "/api/leagues");
            pedido.Headers.Add("Authorization", "Test");
            await cliente.SendAsync(pedido);

            ligas.Verify(l => l.GetLeaguesAsync(), Times.Once);
        }

        [Test]
        public async Task Escrita_Com_Sucesso_Invalida_A_Cache()
        {
            await cliente.GetAsync("/api/leagues");

            var escrita = new HttpRequestMessage(HttpMethod.Post, "/api/Chat/create-room")
            {
                Content = JsonContent.Create(new CreateChatRoomDto { RoomName = "Sala", MemberIds = new() { "x" } }),
            };
            escrita.Headers.Add("Authorization", "Test");
            Assert.That((await cliente.SendAsync(escrita)).StatusCode, Is.EqualTo(HttpStatusCode.OK));

            await cliente.GetAsync("/api/leagues");
            ligas.Verify(l => l.GetLeaguesAsync(), Times.Exactly(2));
        }

        [Test]
        public async Task Respostas_Com_Sessao_Continuam_NoStore()
        {
            var pedido = new HttpRequestMessage(HttpMethod.Get, "/api/Chat/my-rooms");
            pedido.Headers.Add("Authorization", "Test");

            var resposta = await cliente.SendAsync(pedido);

            Assert.That(resposta.Headers.CacheControl?.NoStore, Is.True);
        }
    }
}
