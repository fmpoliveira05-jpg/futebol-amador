using Application.Competition;
using Domain.Enums;
using NUnit.Framework;

namespace Tests.Unit.CompetitionTests
{
    [TestFixture]
    public class LineupAutoFillerTests
    {
        private static List<SquadPlayer> Squad()
        {
            var list = new List<SquadPlayer>
            {
                new("gr1", "Guarda-redes 1", Position.GOALKEEPER, PlayerStatus.ACTIVE),
                new("gr2", "Guarda-redes 2", Position.GOALKEEPER, PlayerStatus.ACTIVE),
            };
            for (var i = 1; i <= 6; i++) list.Add(new($"d{i}", $"Defesa {i}", Position.DEFENDER, PlayerStatus.ACTIVE));
            for (var i = 1; i <= 5; i++) list.Add(new($"m{i}", $"Médio {i}", Position.MIDFIELDER, PlayerStatus.ACTIVE));
            for (var i = 1; i <= 4; i++) list.Add(new($"a{i}", $"Avançado {i}", Position.FORWARD, PlayerStatus.ACTIVE));
            return list;
        }

        [Test(Description = "Sem onze anterior: tática por omissão e cada posição com um jogador dessa posição.")]
        public void Fill_WithoutPrevious_UsesDefaultAndMatchesRoles()
        {
            var result = LineupAutoFiller.Fill(Squad(), null, null, null);
            var formation = Formations.Find(result.Formation)!;
            var roles = Squad().ToDictionary(p => p.Id, p => p.Position);

            Assert.Multiple(() =>
            {
                Assert.That(result.Formation, Is.EqualTo(Formations.Default));
                Assert.That(result.Starters, Has.Count.EqualTo(11));
                foreach (var slot in formation.Slots)
                {
                    Assert.That(roles[result.Starters[slot.Slot]], Is.EqualTo(slot.Role), slot.PositionCode);
                }

                Assert.That(result.Bench, Has.Count.EqualTo(6));
                Assert.That(result.Bench.First(), Is.EqualTo("gr2"), "o guarda-redes suplente vai primeiro para o banco");
                Assert.That(result.Starters.Values.Intersect(result.Bench), Is.Empty);
            });
        }

        [Test(Description = "Repete o último onze e substitui só quem saiu ou está lesionado, por alguém da mesma posição.")]
        public void Fill_RepeatsPreviousAndReplacesUnavailable()
        {
            var squad = Squad();
            var injured = squad.FindIndex(p => p.Id == "m1");
            squad[injured] = squad[injured] with { Status = PlayerStatus.INJURED };

            var previous = new Dictionary<int, string>
            {
                [0] = "gr1", [1] = "d1", [2] = "d2", [3] = "d3", [4] = "d4",
                [5] = "m1", [6] = "m2", [7] = "m3", [8] = "m4", [9] = "a1", [10] = "a2",
            };
            // "a2" saiu da equipa.
            squad.RemoveAll(p => p.Id == "a2");

            var result = LineupAutoFiller.Fill(squad, "4-4-2", previous, new[] { "gr2", "d5" });

            Assert.Multiple(() =>
            {
                Assert.That(result.Formation, Is.EqualTo("4-4-2"));
                Assert.That(result.Starters[0], Is.EqualTo("gr1"));
                Assert.That(result.Starters[6], Is.EqualTo("m2"));
                Assert.That(result.Starters[5], Is.EqualTo("m5"), "o médio lesionado é trocado pelo médio disponível");
                Assert.That(result.Starters[10], Is.AnyOf("a3", "a4"), "o avançado que saiu é trocado por outro avançado");
                Assert.That(result.Starters.Values, Does.Not.Contain("m1"));
                Assert.That(result.Bench.Take(2), Is.EqualTo(new[] { "gr2", "d5" }), "mantém a ordem do banco anterior");
            });
        }

        [Test(Description = "Plantel curto: o onze fica com os que há, e posições sem jogador da posição recebem outro qualquer.")]
        public void Fill_ShortSquad_FillsWhatItCan()
        {
            var squad = new List<SquadPlayer>
            {
                new("x1", "Ana", Position.FORWARD, PlayerStatus.ACTIVE),
                new("x2", "Bia", Position.FORWARD, PlayerStatus.ACTIVE),
                new("x3", "Carla", Position.DEFENDER, PlayerStatus.UNAVAILABLE),
            };

            var result = LineupAutoFiller.Fill(squad, null, null, null);

            Assert.Multiple(() =>
            {
                Assert.That(result.Starters, Has.Count.EqualTo(2));
                Assert.That(result.Starters.ContainsKey(0), Is.True, "a baliza é a primeira a ser preenchida");
                Assert.That(result.Bench, Is.Empty);
            });
        }
    }
}
