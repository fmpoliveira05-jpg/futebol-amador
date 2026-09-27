using Application.DTOs.Chat;
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
    /// <summary>Idempotency-Key: um POST repetido não cria outra vez o recurso.</summary>
    [TestFixture]
    public class IdempotenciaIntegrationTests
    {
        private ApiTestAppFactory factory = null!;
        private Mock<IChatRoomService> chat = null!;
        private HttpClient cliente = null!;
        private int criadas;

        [SetUp]
        public void SetUp()
        {
            criadas = 0;
            chat = new Mock<IChatRoomService>();
            chat.Setup(c => c.CreateRoomAsync(It.IsAny<CreateChatRoomDto>(), It.IsAny<string>()))
                .Returns(async () =>
                {
                    await Task.Delay(150);
                    return $"sala-{Interlocked.Increment(ref criadas)}";
                });

            factory = new ApiTestAppFactory();
            cliente = factory.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
            {
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

        private HttpRequestMessage Pedido(string? chave, string nome = "Treino")
        {
            var pedido = new HttpRequestMessage(HttpMethod.Post, "/api/Chat/create-room")
            {
                Content = JsonContent.Create(new CreateChatRoomDto { RoomName = nome, MemberIds = new() { "outro" } }),
            };
            pedido.Headers.Add("Authorization", "Test");
            if (chave != null)
            {
                pedido.Headers.Add("Idempotency-Key", chave);
            }
            return pedido;
        }

        [Test]
        public async Task Pedido_Repetido_Devolve_A_Mesma_Resposta_Sem_Criar_Outra_Vez()
        {
            var primeiro = await cliente.SendAsync(Pedido("chave-0001"));
            var segundo = await cliente.SendAsync(Pedido("chave-0001"));

            Assert.That(primeiro.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(segundo.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await segundo.Content.ReadAsStringAsync(), Is.EqualTo(await primeiro.Content.ReadAsStringAsync()));
            Assert.That(segundo.Headers.Contains("Idempotent-Replayed"), Is.True);
            Assert.That(criadas, Is.EqualTo(1));
        }

        [Test]
        public async Task Pedidos_Simultaneos_Com_A_Mesma_Chave_Criam_So_Um()
        {
            var respostas = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => cliente.SendAsync(Pedido("chave-simultanea"))));

            Assert.That(criadas, Is.EqualTo(1));
            Assert.That(respostas.Select(r => r.StatusCode), Is.All.EqualTo(HttpStatusCode.OK).Or.EqualTo(HttpStatusCode.Conflict));
        }

        [Test]
        public async Task Chave_Reutilizada_Noutro_Pedido_Responde_422()
        {
            await cliente.SendAsync(Pedido("chave-0002", "Treino"));
            var outro = await cliente.SendAsync(Pedido("chave-0002", "Jantar"));

            Assert.That(outro.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
            Assert.That(criadas, Is.EqualTo(1));
        }

        [Test]
        public async Task Sem_Chave_Cada_Pedido_E_Processado()
        {
            await cliente.SendAsync(Pedido(null));
            await cliente.SendAsync(Pedido(null));

            Assert.That(criadas, Is.EqualTo(2));
        }

        [Test]
        public async Task Chave_Mal_Formada_Responde_400()
        {
            var resposta = await cliente.SendAsync(Pedido("curta"));

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(criadas, Is.EqualTo(0));
        }
    }
}
