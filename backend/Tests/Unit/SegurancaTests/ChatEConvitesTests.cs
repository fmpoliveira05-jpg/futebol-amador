using Application.DTOs.Chat;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using Moq;
using NUnit.Framework;

namespace Unit.SegurancaTests
{
    /// <summary>Controlo de acesso nas salas de chat e nos convites de jogo.</summary>
    [TestFixture]
    public class ChatEConvitesTests
    {
        private Mock<IPlayerRepository> jogadores = null!;
        private FirebaseChatService chat = null!;
        private readonly Guid equipa = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            jogadores = new Mock<IPlayerRepository>();
            // A validação acontece antes de qualquer acesso ao Firestore, por isso não é preciso um.
            chat = new FirebaseChatService(null!, Mock.Of<ITeamRepository>(), jogadores.Object);
        }

        private Player Jogador(string id, Guid? equipaId) => new() { Id = id, IdTeam = equipaId, Name = id };

        [Test]
        public async Task Sala_Com_Colegas_Inclui_Quem_A_Cria()
        {
            jogadores.Setup(r => r.GetPlayerByIdAsync("eu")).ReturnsAsync(Jogador("eu", equipa));
            jogadores.Setup(r => r.GetPlayersListByIdListAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Player> { Jogador("a", equipa), Jogador("b", equipa) });

            var membros = await chat.ValidarMembrosAsync(
                new CreateChatRoomDto { RoomName = "Treino", MemberIds = new() { "a", "b", "a", "eu" } }, "eu");

            Assert.That(membros, Is.EquivalentTo(new[] { "eu", "a", "b" }));
        }

        [Test]
        public void Sala_Com_Jogador_De_Outra_Equipa_E_Recusada()
        {
            jogadores.Setup(r => r.GetPlayerByIdAsync("eu")).ReturnsAsync(Jogador("eu", equipa));
            jogadores.Setup(r => r.GetPlayersListByIdListAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Player> { Jogador("a", equipa), Jogador("estranho", Guid.NewGuid()) });

            Assert.ThrowsAsync<ForbiddenException>(() => chat.ValidarMembrosAsync(
                new CreateChatRoomDto { RoomName = "Sala", MemberIds = new() { "a", "estranho" } }, "eu"));
        }

        [Test]
        public void Sala_Com_Id_Inexistente_E_Recusada()
        {
            jogadores.Setup(r => r.GetPlayerByIdAsync("eu")).ReturnsAsync(Jogador("eu", equipa));
            jogadores.Setup(r => r.GetPlayersListByIdListAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Player> { Jogador("a", equipa) });

            Assert.ThrowsAsync<ForbiddenException>(() => chat.ValidarMembrosAsync(
                new CreateChatRoomDto { RoomName = "Sala", MemberIds = new() { "a", "inventado" } }, "eu"));
        }

        [Test]
        public void Quem_Nao_Tem_Equipa_Nao_Cria_Salas()
        {
            jogadores.Setup(r => r.GetPlayerByIdAsync("eu")).ReturnsAsync(Jogador("eu", null));

            Assert.ThrowsAsync<ForbiddenException>(() => chat.ValidarMembrosAsync(
                new CreateChatRoomDto { RoomName = "Sala", MemberIds = new() { "a" } }, "eu"));
        }

        [Test]
        public void Sala_Com_Demasiados_Membros_Ou_Sem_Nome_E_Recusada()
        {
            var muitos = Enumerable.Range(0, 40).Select(i => $"j{i}").ToList();

            Assert.ThrowsAsync<ValidationException>(() => chat.ValidarMembrosAsync(
                new CreateChatRoomDto { RoomName = "Sala", MemberIds = muitos }, "eu"));
            Assert.ThrowsAsync<ValidationException>(() => chat.ValidarMembrosAsync(
                new CreateChatRoomDto { RoomName = "  ", MemberIds = new() { "a" } }, "eu"));
            Assert.ThrowsAsync<ValidationException>(() => chat.ValidarMembrosAsync(
                new CreateChatRoomDto { RoomName = "Sala", MemberIds = new() { "eu" } }, "eu"));
        }

        [Test]
        public async Task Convite_De_Outras_Equipas_Nao_E_Devolvido()
        {
            var convites = new Mock<IMatchInviteRepository>();
            var servico = new MatchInviteService(
                convites.Object, Mock.Of<ITeamRepository>(), Mock.Of<IMatchRepository>(), Mock.Of<IPitchRepository>(),
                Mock.Of<IMatchInviteValidator>(), Mock.Of<IUnityOfWork>(), Mock.Of<INotificationService>(),
                Mock.Of<IChatRoomService>(), Mock.Of<INotificationFirebaseService>());

            var campo = new Pitch("Campo", "Rua A, Guimarães");
            var remetente = new Team { Name = "A", Pitch = campo };
            var destinatario = new Team { Name = "B", Pitch = new Pitch("Outro", "Rua B, Braga") };
            var convite = new MatchInvite(remetente, destinatario, DateTime.UtcNow.AddDays(3), campo);
            convites.Setup(r => r.GetMatchInviteById(convite.Id)).ReturnsAsync(convite);

            // Administrador de uma terceira equipa com o id de um convite que não é seu.
            var alheio = await servico.GetMatchInvite(Guid.NewGuid(), convite.Id);
            var doRemetente = await servico.GetMatchInvite(remetente.Id, convite.Id);
            var doDestinatario = await servico.GetMatchInvite(destinatario.Id, convite.Id);

            Assert.That(alheio, Is.Null);
            Assert.That(doRemetente, Is.Not.Null);
            Assert.That(doRemetente!.isHome, Is.True);
            Assert.That(doDestinatario, Is.Not.Null);
            Assert.That(doDestinatario!.isHome, Is.False);
        }
    }
}
