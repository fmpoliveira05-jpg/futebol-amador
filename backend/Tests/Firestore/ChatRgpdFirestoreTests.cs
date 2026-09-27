using Application.Interfaces.Repositories;
using Application.Services;
using Google.Cloud.Firestore;
using Moq;
using NUnit.Framework;

namespace Tests.Firestore
{
    /// <summary>
    /// Exportação e eliminação das mensagens do chat (RGPD) contra o emulador do Firestore.
    /// Só corre com <c>FIRESTORE_EMULATOR_HOST</c>, por exemplo:
    /// <code>cd firebase &amp;&amp; firebase emulators:exec --only firestore --project demo-futebol-amador "dotnet test ../backend/Tests --filter Category=Firestore"</code>
    /// </summary>
    [TestFixture]
    [Category("Firestore")]
    [NonParallelizable]
    public class ChatRgpdFirestoreTests
    {
        private FirestoreDb db = null!;
        private FirebaseChatService chat = null!;
        private string prefixo = null!;

        [SetUp]
        public async Task SetUp()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FIRESTORE_EMULATOR_HOST")))
            {
                Assert.Ignore("Sem emulador do Firestore (FIRESTORE_EMULATOR_HOST).");
            }

            db = await new FirestoreDbBuilder
            {
                ProjectId = "demo-futebol-amador",
                EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOnly,
            }.BuildAsync();
            chat = new FirebaseChatService(db, new Mock<ITeamRepository>().Object, new Mock<IPlayerRepository>().Object);
            prefixo = Guid.NewGuid().ToString("N")[..8];

            for (var s = 1; s <= 2; s++)
            {
                var sala = db.Collection("chatRooms").Document($"{prefixo}-sala{s}");
                await sala.SetAsync(new Dictionary<string, object>
                {
                    ["name"] = $"Sala {s}",
                    ["createdBy"] = $"{prefixo}-a",
                    ["members"] = new List<string> { $"{prefixo}-a", $"{prefixo}-b" },
                });
                for (var m = 1; m <= 4; m++)
                {
                    await sala.Collection("messages").Document($"m{m}").SetAsync(new Dictionary<string, object>
                    {
                        ["text"] = $"Mensagem {m}",
                        ["senderId"] = m % 2 == 1 ? $"{prefixo}-a" : $"{prefixo}-b",
                        ["timestamp"] = Timestamp.GetCurrentTimestamp(),
                    });
                }
            }
        }

        [Test]
        public async Task Exporta_So_As_Mensagens_Do_Titular()
        {
            var mensagens = await chat.ExportarMensagensAsync($"{prefixo}-a");

            Assert.That(mensagens, Has.Count.EqualTo(4));
            Assert.That(mensagens.Select(m => m.Sala).Distinct(), Is.EquivalentTo(new[] { $"{prefixo}-sala1", $"{prefixo}-sala2" }));
            Assert.That(mensagens.All(m => m.EnviadaEm != null), Is.True);
        }

        [Test]
        public async Task Eliminar_Apaga_As_Mensagens_E_Retira_Das_Salas()
        {
            await chat.EliminarDadosUtilizadorAsync($"{prefixo}-a");

            Assert.That(await chat.ExportarMensagensAsync($"{prefixo}-a"), Is.Empty);
            Assert.That(await chat.ExportarMensagensAsync($"{prefixo}-b"), Has.Count.EqualTo(4), "as mensagens dos outros ficam");

            var sala = await db.Collection("chatRooms").Document($"{prefixo}-sala1").GetSnapshotAsync();
            Assert.That(sala.GetValue<List<string>>("members"), Is.EqualTo(new[] { $"{prefixo}-b" }));
            Assert.That(sala.GetValue<string>("createdBy"), Is.EqualTo("removido"));
        }
    }
}
