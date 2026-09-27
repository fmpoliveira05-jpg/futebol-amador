using Application.DTOs.Chat;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
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
    /// <summary>Quotas por utilizador nas operações caras (aqui, salas de chat).</summary>
    [TestFixture]
    public class QuotasIntegrationTests
    {
        [Test]
        public async Task Quota_E_Por_Utilizador_E_Responde_429()
        {
            var chat = new Mock<IChatRoomService>();
            chat.Setup(c => c.CreateRoomAsync(It.IsAny<CreateChatRoomDto>(), It.IsAny<string>())).ReturnsAsync("sala");
            using var factory = new ApiTestAppFactory();
            using var cliente = factory.WithWebHostBuilder(b =>
            {
                b.UseSetting("LimitacaoPedidos:SalasChat:Pedidos", "2");
                b.ConfigureTestServices(s =>
                {
                    s.RemoveAll<IChatRoomService>();
                    s.AddSingleton(chat.Object);
                });
            }).CreateClient();

            async Task<HttpStatusCode> Criar(string autorizacao)
            {
                var pedido = new HttpRequestMessage(HttpMethod.Post, "/api/Chat/create-room")
                {
                    Content = JsonContent.Create(new CreateChatRoomDto { RoomName = "Sala", MemberIds = new() { "x" } }),
                };
                pedido.Headers.Add("Authorization", autorizacao);
                return (await cliente.SendAsync(pedido)).StatusCode;
            }

            var a = new[] { await Criar("Test"), await Criar("Test"), await Criar("Test") };
            var outro = await Criar(TestAuthHandler.PrefixoUid + "outro-utilizador");

            Assert.That(a[0], Is.EqualTo(HttpStatusCode.OK));
            Assert.That(a[1], Is.EqualTo(HttpStatusCode.OK));
            Assert.That(a[2], Is.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(outro, Is.EqualTo(HttpStatusCode.OK), "a quota de um utilizador não afeta outro");
        }
    }
}
