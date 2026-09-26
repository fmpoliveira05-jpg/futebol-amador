using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Tests.Unit.CompetitionTests
{
    /// <summary>Passagem das divisões antigas para ligas, no arranque da API.</summary>
    [TestFixture]
    public class LigasIniciaisTests
    {
        [Test]
        public async Task CreatesOneLeaguePerDivision_AndMovesTeams()
        {
            using var t = new CompetitionTestDb();
            var d1 = t.AddRank("Divisão 1");
            var d4 = t.AddRank("Divisão 4");
            var top = t.AddTeam("Topo", rank: d1);
            var low = t.AddTeam("Baixo", rank: d4);
            var older = t.AddPlayer("antigo", team: top, admin: true, adminSince: new DateTime(2025, 1, 1));
            t.AddPlayer("recente", team: top, admin: true, adminSince: new DateTime(2025, 9, 1));
            await t.Db.SaveChangesAsync();

            var now = new DateTime(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc); // quarta-feira
            await LigasIniciais.GarantirAsync(t.Db, now);
            await LigasIniciais.GarantirAsync(t.Db, now); // uma segunda vez não faz nada

            var leagues = await t.Db.League.OrderBy(l => l.Level).ToListAsync();
            var seasons = await t.Db.Season.Include(s => s.Teams).ToListAsync();

            Assert.Multiple(() =>
            {
                Assert.That(leagues.Select(l => (l.Name, l.Level)), Is.EqualTo(new[] { ("Divisão 1", 1), ("Divisão 4", 2) }));
                Assert.That(top.IdLeague, Is.EqualTo(leagues[0].Id));
                Assert.That(low.IdLeague, Is.EqualTo(leagues[1].Id));
                Assert.That(seasons, Has.Count.EqualTo(2));
                Assert.That(seasons.All(s => s.Status == SeasonStatus.REGISTRATION && s.Teams.Count == 1), Is.True);
                Assert.That(seasons[0].StartDate, Is.EqualTo(new DateTime(2026, 10, 3)), "sábado depois de uma semana");
                Assert.That(top.CreatorId, Is.EqualTo(older.Id), "o administrador mais antigo passa a principal");
            });
        }
    }
}
