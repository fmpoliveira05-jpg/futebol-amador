using Application.DTOs.Competition;
using Application.Services.Competition;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Tests.Unit.CompetitionTests
{
    [TestFixture]
    public class TransferServiceTests
    {
        private CompetitionTestDb t = null!;
        private TransferService sut = null!;
        private Team sellers = null!, buyers = null!;
        private Player sellerAdmin = null!, buyerAdmin = null!, target = null!;

        [SetUp]
        public void SetUp()
        {
            t = new CompetitionTestDb();
            var league = t.AddLeague("Divisão 1", 1);
            sellers = t.AddTeam("Vendedores", league);
            buyers = t.AddTeam("Compradores", league);
            sellerAdmin = t.AddPlayer("vendedor-admin", team: sellers, admin: true);
            buyerAdmin = t.AddPlayer("comprador-admin", team: buyers, admin: true);
            target = t.AddPlayer("alvo", Position.FORWARD, sellers);
            target.Nationality = "Brasil";
            sellers.CreatorId = sellerAdmin.Id;
            buyers.CreatorId = buyerAdmin.Id;
            t.Db.SaveChanges();

            sut = new TransferService(t.Transfers, t.Players, t.Teams, t.Authorization.Object, t.UnitOfWork,
                t.Notifications.Object, t.Clock, NullLogger<TransferService>.Instance);
        }

        [TearDown]
        public void TearDown() => t.Dispose();

        private Task<TransferOfferDto> Offer() =>
            sut.CreateOfferAsync(buyerAdmin.Id, new CreateTransferOfferDto { TeamId = buyers.Id, PlayerId = target.Id, Message = "Queremos-te!" });

        [Test(Description = "Proposta direta: o clube aceita, depois o jogador aceita e muda de equipa.")]
        public async Task DirectOffer_ClubThenPlayer_CompletesTransfer()
        {
            var offer = await Offer();
            Assert.That(offer.Status, Is.EqualTo(TransferOfferStatus.PENDING_CLUB));

            var afterClub = await sut.AcceptOfferAsync(sellerAdmin.Id, offer.Id);
            Assert.That(afterClub.Status, Is.EqualTo(TransferOfferStatus.PENDING_PLAYER));
            Assert.That((await sut.GetPlayerOffersAsync(target.Id)).Select(o => o.Id), Is.EqualTo(new[] { offer.Id }));

            var done = await sut.AcceptOfferAsync(target.Id, offer.Id);
            var player = await t.Db.Player.SingleAsync(p => p.Id == target.Id);
            var history = await t.Db.TransferRecord.Where(r => r.PlayerId == target.Id).ToListAsync();

            Assert.Multiple(() =>
            {
                Assert.That(done.Status, Is.EqualTo(TransferOfferStatus.ACCEPTED));
                Assert.That(player.IdTeam, Is.EqualTo(buyers.Id));
                Assert.That(player.JoinedTeamAt, Is.EqualTo(t.Clock.GetUtcNow().UtcDateTime));
                Assert.That(history.Single().Kind, Is.EqualTo(TransferKind.TRANSFER));
                Assert.That((history.Single().FromTeamName, history.Single().ToTeamName), Is.EqualTo(("Vendedores", "Compradores")));
            });
        }

        [Test(Description = "Mercado: um jogador listado recebe a proposta diretamente (o clube já concordou).")]
        public async Task ListedPlayer_OfferGoesStraightToPlayer()
        {
            await sut.ListPlayerAsync(sellerAdmin.Id, sellers.Id, target.Id);

            var market = await sut.GetMarketAsync(buyerAdmin.Id, buyers.Id, new MarketFilterDto { OnlyListed = true });
            var offer = await Offer();

            Assert.Multiple(() =>
            {
                Assert.That(market.Select(m => m.PlayerId), Is.EqualTo(new[] { target.Id }));
                Assert.That(market[0].IsListed, Is.True);
                Assert.That(market[0].TeamName, Is.EqualTo("Vendedores"));
                Assert.That(offer.Status, Is.EqualTo(TransferOfferStatus.PENDING_PLAYER));
                Assert.That(offer.ViaListing, Is.True);
            });

            await sut.AcceptOfferAsync(target.Id, offer.Id);
            Assert.That(await t.Db.TransferListing.AnyAsync(), Is.False, "ao mudar de equipa sai do mercado");
        }

        [Test(Description = "Filtros do mercado: com ou sem equipa, liga e nacionalidade; nunca mostra a própria equipa.")]
        public async Task Market_Filters()
        {
            var free = t.AddPlayer("livre");
            free.Nationality = "Portugal";
            await t.Db.SaveChangesAsync();

            var all = await sut.GetMarketAsync(buyerAdmin.Id, buyers.Id, new MarketFilterDto());
            var withTeam = await sut.GetMarketAsync(buyerAdmin.Id, buyers.Id, new MarketFilterDto { HasTeam = true });
            var freeOnly = await sut.GetMarketAsync(buyerAdmin.Id, buyers.Id, new MarketFilterDto { HasTeam = false });
            var brazil = await sut.GetMarketAsync(buyerAdmin.Id, buyers.Id, new MarketFilterDto { Nationality = "Brasil" });
            var league = await sut.GetMarketAsync(buyerAdmin.Id, buyers.Id, new MarketFilterDto { LeagueId = sellers.IdLeague });

            Assert.Multiple(() =>
            {
                Assert.That(all.Select(p => p.PlayerId), Is.EquivalentTo(new[] { "alvo", "vendedor-admin", "livre" }));
                Assert.That(withTeam.Select(p => p.PlayerId), Is.EquivalentTo(new[] { "alvo", "vendedor-admin" }));
                Assert.That(freeOnly.Select(p => p.PlayerId), Is.EqualTo(new[] { "livre" }));
                Assert.That(brazil.Select(p => p.PlayerId), Is.EqualTo(new[] { "alvo" }));
                Assert.That(league.Select(p => p.LeagueName).Distinct(), Is.EqualTo(new[] { "Divisão 1" }));
            });
        }

        [Test(Description = "O jogador tem sempre a última palavra: pode recusar.")]
        public async Task PlayerCanReject()
        {
            await sut.ListPlayerAsync(sellerAdmin.Id, sellers.Id, target.Id);
            var offer = await Offer();

            var rejected = await sut.RejectOfferAsync(target.Id, offer.Id);

            Assert.That(rejected.Status, Is.EqualTo(TransferOfferStatus.REJECTED));
            Assert.That((await t.Db.Player.SingleAsync(p => p.Id == target.Id)).IdTeam, Is.EqualTo(sellers.Id));
        }

        [Test(Description = "Só o clube do jogador aceita no primeiro passo; o comprador só pode retirar a proposta.")]
        public async Task Permissions()
        {
            var offer = await Offer();

            Assert.ThrowsAsync<ForbiddenException>(() => sut.AcceptOfferAsync(buyerAdmin.Id, offer.Id));
            Assert.ThrowsAsync<ForbiddenException>(() => sut.AcceptOfferAsync(target.Id, offer.Id));
            var cancelled = await sut.RejectOfferAsync(buyerAdmin.Id, offer.Id);
            Assert.That(cancelled.Status, Is.EqualTo(TransferOfferStatus.CANCELLED));
            Assert.ThrowsAsync<BusinessRuleException>(() => sut.AcceptOfferAsync(sellerAdmin.Id, offer.Id));
        }

        [Test]
        public void CannotOfferForFreeAgentOrOwnPlayer()
        {
            t.AddPlayer("livre");
            t.Db.SaveChanges();

            Assert.ThrowsAsync<BusinessRuleException>(() =>
                sut.CreateOfferAsync(buyerAdmin.Id, new CreateTransferOfferDto { TeamId = buyers.Id, PlayerId = "livre" }));
            Assert.ThrowsAsync<BusinessRuleException>(() =>
                sut.CreateOfferAsync(buyerAdmin.Id, new CreateTransferOfferDto { TeamId = buyers.Id, PlayerId = buyerAdmin.Id }));
        }

        [Test]
        public async Task DuplicateOpenOffer_Rejected()
        {
            await Offer();
            Assert.ThrowsAsync<BusinessRuleException>(Offer);
        }

        [Test(Description = "O único administrador não pode sair; com outro administrador, o estatuto de principal passa-lhe.")]
        public async Task SoleAdmin_CannotLeave_ButSupremacyIsHandedOver()
        {
            var offer = await sut.CreateOfferAsync(buyerAdmin.Id, new CreateTransferOfferDto { TeamId = buyers.Id, PlayerId = sellerAdmin.Id });
            await sut.AcceptOfferAsync(sellerAdmin.Id, offer.Id);

            Assert.ThrowsAsync<BusinessRuleException>(() => sut.AcceptOfferAsync(sellerAdmin.Id, offer.Id));

            target.IsAdmin = true;
            target.IsAdminLastChangedAt = new DateTime(2026, 3, 1);
            await t.Db.SaveChangesAsync();

            await sut.AcceptOfferAsync(sellerAdmin.Id, offer.Id);

            var team = await t.Db.Team.SingleAsync(x => x.Id == sellers.Id);
            var moved = await t.Db.Player.SingleAsync(p => p.Id == sellerAdmin.Id);
            Assert.Multiple(() =>
            {
                Assert.That(team.CreatorId, Is.EqualTo(target.Id));
                Assert.That(moved.IdTeam, Is.EqualTo(buyers.Id));
                Assert.That(moved.IsAdmin, Is.False, "quem chega a uma equipa nova não é administrador");
            });
        }

        [Test(Description = "Quando o jogador muda de equipa, as outras propostas por ele são anuladas.")]
        public async Task AcceptingCancelsOtherOffers()
        {
            var third = t.AddTeam("Terceiros", null);
            var thirdAdmin = t.AddPlayer("terceiro-admin", team: third, admin: true);
            await t.Db.SaveChangesAsync();
            await sut.ListPlayerAsync(sellerAdmin.Id, sellers.Id, target.Id);

            var first = await Offer();
            var second = await sut.CreateOfferAsync(thirdAdmin.Id, new CreateTransferOfferDto { TeamId = third.Id, PlayerId = target.Id });
            await sut.AcceptOfferAsync(target.Id, first.Id);

            Assert.That((await t.Db.TransferOffer.SingleAsync(o => o.Id == second.Id)).Status, Is.EqualTo(TransferOfferStatus.CANCELLED));
        }

        [Test]
        public void ListingRequiresOwnPlayer()
        {
            Assert.ThrowsAsync<ForbiddenException>(() => sut.ListPlayerAsync(buyerAdmin.Id, buyers.Id, target.Id));
        }
    }
}
