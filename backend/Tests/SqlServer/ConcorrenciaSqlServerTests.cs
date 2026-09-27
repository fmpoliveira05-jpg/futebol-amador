using Api.Middlewares;
using Application.Interfaces;
using Application.Interfaces.Services.Hub;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using Tests.Integration;

namespace Tests.SqlServer
{
    /// <summary>
    /// Pedidos simultâneos contra um SQL Server real: concorrência otimista (rowversion),
    /// restrições únicas e transações.
    /// </summary>
    [TestFixture]
    [Category("SqlServer")]
    [NonParallelizable]
    public class ConcorrenciaSqlServerTests
    {
        private ApiSqlServerFactory factory = null!;

        [SetUp]
        public async Task SetUp() => factory = await ApiSqlServerFactory.CriarAsync();

        [TearDown]
        public async Task TearDown()
        {
            if (factory != null)
            {
                await factory.DisposeAsync();
            }
        }

        private static Player NovoJogador(string id, string email) =>
            new(id, "Jogador " + id, new DateOnly(1995, 5, 1), "Rua A, Braga", email, "+351912345678", Position.MIDFIELDER, 180, null);

        private static Team NovaEquipa(string nome, Rank rank) =>
            new(nome, null, "", new Pitch("Campo " + nome, "Rua do Campo, Guimarães"), rank);

        [Test]
        public async Task Duas_Alteracoes_Ao_Mesmo_Jogador_A_Segunda_Da_Conflito()
        {
            await using (var db = factory.NovoContexto())
            {
                db.Player.Add(NovoJogador("uid-conc-1", "conc1@exemplo.pt"));
                await db.SaveChangesAsync();
            }

            await using var pedidoA = factory.NovoContexto();
            await using var pedidoB = factory.NovoContexto();
            var a = await pedidoA.Player.SingleAsync(p => p.Id == "uid-conc-1");
            var b = await pedidoB.Player.SingleAsync(p => p.Id == "uid-conc-1");

            a.IsAdmin = true;
            await pedidoA.SaveChangesAsync();

            b.Height = 175;
            var erro = Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => pedidoB.SaveChangesAsync());
            Assert.That(GlobalExceptionHandler.Classificar(erro!).Status, Is.EqualTo(409));
        }

        [Test]
        public async Task Alteracao_So_Na_Tabela_User_Tambem_E_Protegida()
        {
            // Player usa TPT (tabelas User e Player): o rowversion está na tabela Player.
            await using (var db = factory.NovoContexto())
            {
                db.Player.Add(NovoJogador("uid-conc-2", "conc2@exemplo.pt"));
                await db.SaveChangesAsync();
            }

            await using var pedidoA = factory.NovoContexto();
            await using var pedidoB = factory.NovoContexto();
            var a = await pedidoA.Player.SingleAsync(p => p.Id == "uid-conc-2");
            var b = await pedidoB.Player.SingleAsync(p => p.Id == "uid-conc-2");

            a.Name = "Nome A";
            await pedidoA.SaveChangesAsync();

            b.Name = "Nome B";
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => pedidoB.SaveChangesAsync());
        }

        [Test]
        public async Task Pedido_De_Adesao_Repetido_Viola_A_Restricao_Unica_E_Da_409()
        {
            var rank = new Rank { Name = "Divisão teste" };
            var equipa = NovaEquipa("Equipa Única", rank);
            await using (var db = factory.NovoContexto())
            {
                db.Player.Add(NovoJogador("uid-adesao", "adesao@exemplo.pt"));
                db.Team.Add(equipa);
                await db.SaveChangesAsync();
            }

            await using var pedidoA = factory.NovoContexto();
            await using var pedidoB = factory.NovoContexto();
            pedidoA.MembershipRequests.Add(new MembershipRequest { IdPlayer = "uid-adesao", IdTeam = equipa.Id, InviteDate = DateTime.UtcNow, IsPlayerSender = true });
            pedidoB.MembershipRequests.Add(new MembershipRequest { IdPlayer = "uid-adesao", IdTeam = equipa.Id, InviteDate = DateTime.UtcNow, IsPlayerSender = true });

            await pedidoA.SaveChangesAsync();
            var erro = Assert.ThrowsAsync<DbUpdateException>(() => pedidoB.SaveChangesAsync());
            Assert.That(GlobalExceptionHandler.Classificar(erro!).Status, Is.EqualTo(409));
        }

        [Test]
        public async Task Nome_De_Equipa_Repetido_E_Recusado_Pela_Base_De_Dados()
        {
            var rank = new Rank { Name = "Divisão nomes" };
            await using (var db = factory.NovoContexto())
            {
                db.Team.Add(NovaEquipa("Os Mesmos", rank));
                await db.SaveChangesAsync();
            }

            await using var outro = factory.NovoContexto();
            var rankExistente = await outro.Rank.FirstAsync();
            outro.Team.Add(NovaEquipa("os mesmos", rankExistente));
            Assert.ThrowsAsync<DbUpdateException>(() => outro.SaveChangesAsync());
        }

        [Test]
        public async Task Aceitar_O_Mesmo_Convite_Em_Simultaneo_Cria_Um_So_Jogo()
        {
            var rank = new Rank { Name = "Divisão convites" };
            var remetente = NovaEquipa("Remetente FC", rank);
            var recetora = NovaEquipa("Recetora FC", rank);
            var admin = NovoJogador(TestAuthHandler.TestUserId, "admin@exemplo.pt");
            admin.IsAdmin = true;
            admin.IdTeam = recetora.Id;
            var convite = new MatchInvite(remetente, recetora, DateTime.UtcNow.AddDays(5).Date.AddHours(18), remetente.Pitch);

            await using (var db = factory.NovoContexto())
            {
                db.Team.AddRange(remetente, recetora);
                db.Player.Add(admin);
                db.MatchInvite.Add(convite);
                await db.SaveChangesAsync();
            }

            var cliente = factory.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
            {
                s.RemoveAll<INotificationFirebaseService>();
                s.AddSingleton(new Mock<INotificationFirebaseService>().Object);
                s.RemoveAll<INotificationService>();
                s.AddSingleton(new Mock<INotificationService>().Object);
            })).CreateClient();
            cliente.DefaultRequestHeaders.Add("Authorization", "Test");

            var url = $"/api/MatchInvite/{recetora.Id}/AcceptMatchInvite";
            var respostas = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => cliente.PostAsJsonAsync(url, convite.Id)));
            var estados = respostas.Select(r => r.StatusCode).ToList();

            await using var verificacao = factory.NovoContexto();
            var jogos = await verificacao.Match.CountAsync();

            Assert.That(jogos, Is.EqualTo(1), "Só um jogo pode ser criado. Estados: " + string.Join(",", estados));
            Assert.That(estados.Count(e => e == HttpStatusCode.OK), Is.EqualTo(1));
            Assert.That(estados.Where(e => e != HttpStatusCode.OK),
                Is.All.AnyOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest, HttpStatusCode.NotFound));
            Assert.That(await verificacao.MatchInvite.CountAsync(), Is.EqualTo(0));
        }
    }
}
