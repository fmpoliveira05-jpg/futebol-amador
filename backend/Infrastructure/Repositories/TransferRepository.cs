using Application.DTOs.Competition;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TransferRepository : ITransferRepository
    {
        private static readonly TransferOfferStatus[] Open =
            { TransferOfferStatus.PENDING_CLUB, TransferOfferStatus.PENDING_PLAYER };

        private readonly AmateurFootballContext db;

        public TransferRepository(AmateurFootballContext db)
        {
            this.db = db;
        }

        public Task<TransferListing?> GetListingAsync(string playerId) =>
            db.TransferListing.FirstOrDefaultAsync(l => l.PlayerId == playerId);

        public async Task AddListingAsync(TransferListing listing) => await db.TransferListing.AddAsync(listing);

        public void RemoveListing(TransferListing listing) => db.TransferListing.Remove(listing);

        public async Task<HashSet<string>> GetListedPlayerIdsAsync() =>
            (await db.TransferListing.Select(l => l.PlayerId).ToListAsync()).ToHashSet();

        public async Task<List<Player>> SearchMarketAsync(MarketFilterDto filter, Guid excludeTeamId, HashSet<string> listed)
        {
            var query = db.Player
                .Include(p => p.Team).ThenInclude(t => t!.League)
                .Where(p => p.IdTeam != excludeTeamId && p.EliminadoEm == null);

            if (filter.HasTeam == true)
            {
                query = query.Where(p => p.IdTeam != null);
            }
            else if (filter.HasTeam == false)
            {
                query = query.Where(p => p.IdTeam == null);
            }

            if (filter.LeagueId.HasValue)
            {
                query = query.Where(p => p.Team != null && p.Team.IdLeague == filter.LeagueId);
            }

            if (!string.IsNullOrWhiteSpace(filter.Nationality))
            {
                var nat = filter.Nationality.Trim();
                query = query.Where(p => p.Nationality != null && p.Nationality == nat);
            }

            if (filter.Position.HasValue)
            {
                query = query.Where(p => p.Position == filter.Position);
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var name = filter.Name.Trim();
                query = query.Where(p => p.Name.Contains(name));
            }

            if (filter.OnlyListed == true)
            {
                var ids = listed.ToList();
                query = query.Where(p => ids.Contains(p.Id));
            }

            return await query.OrderBy(p => p.Name).Take(200).ToListAsync();
        }

        public async Task AddOfferAsync(TransferOffer offer) => await db.TransferOffer.AddAsync(offer);

        public Task<TransferOffer?> GetOfferAsync(Guid id) =>
            db.TransferOffer
                .Include(o => o.Player).ThenInclude(p => p.Team!).ThenInclude(t => t.Members)
                .Include(o => o.FromTeam).ThenInclude(t => t.Members)
                .Include(o => o.ToTeam).ThenInclude(t => t.Members)
                .FirstOrDefaultAsync(o => o.Id == id);

        public Task<List<TransferOffer>> GetOffersOfTeamAsync(Guid teamId) =>
            db.TransferOffer
                .Include(o => o.Player).Include(o => o.FromTeam).Include(o => o.ToTeam)
                .Where(o => o.IdFromTeam == teamId || o.IdToTeam == teamId)
                .OrderByDescending(o => o.CreatedAt)
                .Take(200)
                .ToListAsync();

        public Task<List<TransferOffer>> GetOffersWaitingForPlayerAsync(string playerId) =>
            db.TransferOffer
                .Include(o => o.Player).Include(o => o.FromTeam).Include(o => o.ToTeam)
                .Where(o => o.PlayerId == playerId && o.Status == TransferOfferStatus.PENDING_PLAYER)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

        public Task<List<TransferOffer>> GetOpenOffersOfPlayerAsync(string playerId) =>
            db.TransferOffer.Where(o => o.PlayerId == playerId && Open.Contains(o.Status)).ToListAsync();

        public Task<bool> HasOpenOfferAsync(string playerId, Guid toTeamId) =>
            db.TransferOffer.AnyAsync(o => o.PlayerId == playerId && o.IdToTeam == toTeamId && Open.Contains(o.Status));

        public async Task RemoveOffersOfTeamAsync(Guid teamId)
        {
            var offers = await db.TransferOffer.Where(o => o.IdFromTeam == teamId || o.IdToTeam == teamId).ToListAsync();
            db.TransferOffer.RemoveRange(offers);
        }

        public async Task AddRecordAsync(TransferRecord record) => await db.TransferRecord.AddAsync(record);

        public Task<List<TransferRecord>> GetRecordsAsync(string playerId) =>
            db.TransferRecord.Where(r => r.PlayerId == playerId).OrderByDescending(r => r.Date).ToListAsync();
    }
}
