using Application.DTOs;
using Application.DTOs.Chat;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace UnitTests.Rgpd
{
    /// <summary>Exportação, eliminação da conta e retenção (RGPD).</summary>
    [TestFixture]
    public class ContaServiceTests
    {
        private Mock<IContaRepository> contas = null!;
        private Mock<IPlayerService> jogadores = null!;
        private Mock<IAuthService> auth = null!;
        private Mock<IChatRoomService> chat = null!;
        private Mock<IImagensUtilizador> imagens = null!;
        private Mock<IUnityOfWork> unidade = null!;
        private ContaService servico = null!;
        private Player jogador = null!;
        private List<string> ordem = null!;

        [SetUp]
        public void SetUp()
        {
            ordem = new();
            jogador = new Player("uid-1", "Francisco", new DateOnly(2000, 1, 2), "Rua X, Guimarães", "f@exemplo.pt", "+351912345678", Position.MIDFIELDER, 180, "https://img")
            {
                Nationality = "Portugal",
                DeviceToken = "token-fcm",
                PoliticaPrivacidadeVersao = "2026-09-27",
            };

            contas = new Mock<IContaRepository>();
            contas.Setup(c => c.GetJogadorComEquipaAsync("uid-1")).ReturnsAsync(() => jogador);
            contas.Setup(c => c.GetIconesEmUsoAsync()).ReturnsAsync(new List<string>());
            jogadores = new Mock<IPlayerService>();
            auth = new Mock<IAuthService>();
            auth.Setup(a => a.DeleteUserAsync("uid-1")).Callback(() => ordem.Add("firebase")).Returns(Task.CompletedTask);
            chat = new Mock<IChatRoomService>();
            chat.Setup(c => c.EliminarDadosUtilizadorAsync("uid-1")).Callback(() => ordem.Add("chat")).Returns(Task.CompletedTask);
            imagens = new Mock<IImagensUtilizador>();
            unidade = new Mock<IUnityOfWork>();
            unidade.Setup(u => u.SaveChangesAsync()).Callback(() => ordem.Add("bd")).ReturnsAsync(1);

            servico = new ContaService(contas.Object, jogadores.Object, auth.Object, chat.Object, imagens.Object, unidade.Object,
                new ConfigurationBuilder().Build(), TimeProvider.System, NullLogger<ContaService>.Instance);
        }

        [Test]
        public async Task Exportacao_Tem_Os_Dados_Pessoais_E_As_Mensagens()
        {
            chat.Setup(c => c.GetMyRoomsAsync("uid-1")).ReturnsAsync(new List<ChatRoomDto> { new() { RoomId = "s1", RoomName = "Equipa" } });
            chat.Setup(c => c.ExportarMensagensAsync("uid-1")).ReturnsAsync(new List<MensagemExportadaDto> { new() { Sala = "s1", Texto = "Olá" } });

            var dados = await servico.ExportarAsync("uid-1");

            Assert.That(dados.Conta.Email, Is.EqualTo("f@exemplo.pt"));
            Assert.That(dados.Conta.Telefone, Is.EqualTo("+351912345678"));
            Assert.That(dados.Conta.DataNascimento, Is.EqualTo(new DateOnly(2000, 1, 2)));
            Assert.That(dados.Conta.NotificacoesPushRegistadas, Is.True);
            Assert.That(dados.SalasDeChat.Single().Nome, Is.EqualTo("Equipa"));
            Assert.That(dados.MensagensEnviadas.Single().Texto, Is.EqualTo("Olá"));
            contas.Verify(c => c.PreencherExportacaoAsync("uid-1", dados), Times.Once);
        }

        [Test]
        public async Task Exportacao_Continua_Sem_Chat_Com_Nota()
        {
            chat.Setup(c => c.GetMyRoomsAsync("uid-1")).ThrowsAsync(new HttpRequestException());

            var dados = await servico.ExportarAsync("uid-1");

            Assert.That(dados.Notas, Is.Not.Empty);
            Assert.That(dados.Conta.Nome, Is.EqualTo("Francisco"));
        }

        [Test]
        public void Eliminar_Com_Palavra_Passe_Errada_Nao_Altera_Nada()
        {
            auth.Setup(a => a.ConfirmarPalavraPasseAsync("uid-1", "errada")).ThrowsAsync(new AuthenticationException("A palavra-passe atual está incorreta."));

            Assert.ThrowsAsync<AuthenticationException>(() => servico.EliminarContaAsync("uid-1", "errada"));

            unidade.Verify(u => u.SaveChangesAsync(), Times.Never);
            auth.Verify(a => a.DeleteUserAsync(It.IsAny<string>()), Times.Never);
            Assert.That(jogador.Name, Is.EqualTo("Francisco"));
        }

        [Test]
        public async Task Eliminar_Anonimiza_Sai_Da_Equipa_E_Apaga_No_Fim_O_Firebase()
        {
            jogador.IdTeam = Guid.NewGuid();
            jogadores.Setup(j => j.LeaveTeam("uid-1")).Callback(() => jogador.IdTeam = null).ReturnsAsync(new Application.DTOs.Player.InfoPlayerDto());

            await servico.EliminarContaAsync("uid-1", "certa");

            jogadores.Verify(j => j.LeaveTeam("uid-1"), Times.Once);
            contas.Verify(c => c.RemoverPendentesDoJogadorAsync("uid-1"), Times.Once);
            imagens.Verify(i => i.ApagarDoUtilizadorAsync("uid-1", It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(jogador.Name, Is.EqualTo(ModelConstants.RgpdConst.NomeAnonimo));
            Assert.That(jogador.Email, Does.Not.Contain("exemplo.pt"));
            Assert.That(jogador.Phone, Is.EqualTo("+000000000000"));
            Assert.That(jogador.Address, Is.EqualTo("Removido"));
            Assert.That(jogador.DateOfBirth, Is.EqualTo(new DateOnly(1900, 1, 1)));
            Assert.That(jogador.ImageUrl, Is.Null);
            Assert.That(jogador.DeviceToken, Is.Null);
            Assert.That(jogador.Nationality, Is.Null);
            Assert.That(jogador.EliminadoEm, Is.Not.Null);
            Assert.That(ordem, Is.EqualTo(new[] { "bd", "chat", "firebase" }));
        }

        [Test]
        public async Task Falha_No_Chat_Nao_Impede_A_Eliminacao()
        {
            chat.Setup(c => c.EliminarDadosUtilizadorAsync("uid-1")).ThrowsAsync(new HttpRequestException());

            await servico.EliminarContaAsync("uid-1", "certa");

            auth.Verify(a => a.DeleteUserAsync("uid-1"), Times.Once);
        }

        [Test]
        public void Conta_Ja_Eliminada_Responde_Nao_Encontrada()
        {
            jogador.EliminadoEm = DateTime.UtcNow;
            Assert.ThrowsAsync<NotFoundException>(() => servico.ExportarAsync("uid-1"));
            Assert.ThrowsAsync<NotFoundException>(() => servico.EliminarContaAsync("uid-1", "x"));
        }

        [Test]
        public async Task Retencao_Apaga_So_Contas_Por_Confirmar()
        {
            var confirmada = new Player { Id = "uid-2" };
            contas.Setup(c => c.GetJogadoresSemEquipaCriadosAntesAsync(It.IsAny<DateTime>(), It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "uid-1", "uid-2" });
            contas.Setup(c => c.GetJogadorComEquipaAsync("uid-2")).ReturnsAsync(confirmada);
            auth.Setup(a => a.ContasPorConfirmarAsync(It.IsAny<IReadOnlyCollection<string>>())).ReturnsAsync(new HashSet<string> { "uid-1" });
            contas.Setup(c => c.ApagarPedidosAdesaoAntigosAsync(It.IsAny<DateTime>())).ReturnsAsync(3);
            contas.Setup(c => c.ApagarConvitesExpiradosAsync(It.IsAny<DateTime>())).ReturnsAsync(2);

            var (apagadas, pedidos, convites) = await servico.AplicarRetencaoAsync(DateTime.UtcNow);

            Assert.That((apagadas, pedidos, convites), Is.EqualTo((1, 3, 2)));
            contas.Verify(c => c.RemoverJogador(jogador), Times.Once);
            contas.Verify(c => c.RemoverJogador(confirmada), Times.Never);
            auth.Verify(a => a.DeleteUserAsync("uid-1"), Times.Once);
        }
    }
}
