using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tests.Integration;

namespace Tests.SqlServer
{
    /// <summary>Direitos RGPD de ponta a ponta (API real + SQL Server): exportação e eliminação.</summary>
    [TestFixture]
    [Category("SqlServer")]
    [NonParallelizable]
    public class RgpdSqlServerTests
    {
        private ApiSqlServerFactory factory = null!;
        private Mock<IAuthService> auth = null!;
        private HttpClient cliente = null!;
        private Team equipa = null!;

        [SetUp]
        public async Task SetUp()
        {
            factory = await ApiSqlServerFactory.CriarAsync();

            var rank = new Rank { Name = "Divisão RGPD" };
            equipa = new Team("Os Titulares", null, "", new Pitch("Campo RGPD", "Rua do Campo, Guimarães"), rank);
            var eu = new Player(TestAuthHandler.TestUserId, "Francisco", new DateOnly(2000, 3, 4), "Rua A, Guimarães", "eu@exemplo.pt", "+351911111111", Position.MIDFIELDER, 180, null)
            {
                IsAdmin = true, IdTeam = equipa.Id, CreationDate = DateTime.UtcNow.AddYears(-2),
                PoliticaPrivacidadeVersao = "2026-09-27", PoliticaPrivacidadeAceiteEm = DateTime.UtcNow,
            };
            var colega = new Player("uid-colega", "Colega", new DateOnly(1999, 1, 1), "Rua B, Braga", "colega@exemplo.pt", "+351922222222", Position.DEFENDER, 178, null)
            {
                IdTeam = equipa.Id, CreationDate = DateTime.UtcNow.AddYears(-1),
            };
            equipa.CreatorId = eu.Id;

            await using (var db = factory.NovoContexto())
            {
                db.Team.Add(equipa);
                db.Player.AddRange(eu, colega);
                db.TransferRecord.Add(new TransferRecord { PlayerId = eu.Id, IdToTeam = equipa.Id, ToTeamName = equipa.Name, Kind = TransferKind.JOINED });
                await db.SaveChangesAsync();
            }

            auth = new Mock<IAuthService>();
            cliente = factory.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
            {
                s.RemoveAll<IAuthService>();
                s.AddSingleton(auth.Object);
            })).CreateClient();
            cliente.DefaultRequestHeaders.Add("Authorization", "Test");
        }

        [TearDown]
        public async Task TearDown()
        {
            cliente?.Dispose();
            if (factory != null)
            {
                await factory.DisposeAsync();
            }
        }

        [Test]
        public async Task Exportacao_Devolve_Os_Dados_Do_Proprio_Em_Ficheiro_Json()
        {
            var resposta = await cliente.GetAsync("/api/User/me/export");
            var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement;

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(resposta.Content.Headers.ContentDisposition?.DispositionType, Is.EqualTo("attachment"));
            Assert.That(json.GetProperty("conta").GetProperty("email").GetString(), Is.EqualTo("eu@exemplo.pt"));
            Assert.That(json.GetProperty("conta").GetProperty("politicaPrivacidadeVersao").GetString(), Is.EqualTo("2026-09-27"));
            Assert.That(json.GetProperty("equipa").GetProperty("nome").GetString(), Is.EqualTo("Os Titulares"));
            Assert.That(json.GetProperty("transferencias").GetArrayLength(), Is.EqualTo(1));
            Assert.That(json.ToString(), Does.Not.Contain("colega@exemplo.pt"));
        }

        [Test]
        public async Task Eliminar_Anonimiza_Passa_A_Administracao_E_Apaga_No_Firebase()
        {
            var pedido = new HttpRequestMessage(HttpMethod.Delete, "/api/User/me") { Content = JsonContent.Create(new { password = "Certa#2026abc" }) };
            var resposta = await cliente.SendAsync(pedido);

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.NoContent), await resposta.Content.ReadAsStringAsync());
            auth.Verify(a => a.ConfirmarPalavraPasseAsync(TestAuthHandler.TestUserId, "Certa#2026abc"), Times.Once);
            auth.Verify(a => a.DeleteUserAsync(TestAuthHandler.TestUserId), Times.Once);

            await using var db = factory.NovoContexto();
            var eu = await db.Player.SingleAsync(p => p.Id == TestAuthHandler.TestUserId);
            var colega = await db.Player.SingleAsync(p => p.Id == "uid-colega");
            var team = await db.Team.SingleAsync(t => t.Id == equipa.Id);

            Assert.That(eu.Name, Is.EqualTo(ModelConstants.RgpdConst.NomeAnonimo));
            Assert.That(eu.Email, Does.EndWith("@anonimo.invalid"));
            Assert.That(eu.Phone, Is.EqualTo("+000000000000"));
            Assert.That(eu.IdTeam, Is.Null);
            Assert.That(eu.EliminadoEm, Is.Not.Null);
            Assert.That(colega.IsAdmin, Is.True, "a administração passa ao membro que fica");
            Assert.That(team.CreatorId, Is.EqualTo("uid-colega"), "e o estatuto de administrador principal");

            var lista = await cliente.GetStringAsync("/api/Player/listPlayers");
            Assert.That(lista, Does.Not.Contain(ModelConstants.RgpdConst.NomeAnonimo));
        }

        [Test]
        public async Task Palavra_Passe_Errada_Nao_Elimina()
        {
            auth.Setup(a => a.ConfirmarPalavraPasseAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Domain.Exceptions.AuthenticationException("A palavra-passe atual está incorreta."));

            var pedido = new HttpRequestMessage(HttpMethod.Delete, "/api/User/me") { Content = JsonContent.Create(new { password = "errada" }) };
            var resposta = await cliente.SendAsync(pedido);

            Assert.That(resposta.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            await using var db = factory.NovoContexto();
            Assert.That((await db.Player.SingleAsync(p => p.Id == TestAuthHandler.TestUserId)).Name, Is.EqualTo("Francisco"));
            auth.Verify(a => a.DeleteUserAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
