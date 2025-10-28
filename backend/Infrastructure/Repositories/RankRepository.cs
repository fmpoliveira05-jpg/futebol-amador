using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RankRepository : IRankRepository
    {
        private readonly AmateurFootballContext DbContext;

        public RankRepository(AmateurFootballContext DbContext)
        {
            this.DbContext = DbContext;
        }

        public async Task AddRankAsync(Rank rank)
        {
            await DbContext.Rank.AddAsync(rank);
        }

        public async Task AddRankInOtherRankPlaceAsync(Rank rank, Rank nextRank) { 
            rank.IdNextRank = nextRank.Id;
            rank.IdPreviousRank = nextRank.IdPreviousRank;
            nextRank.IdPreviousRank = nextRank.Id;
            await DbContext.Rank.AddAsync(rank);
            DbContext.Rank.Update(nextRank);
            
        }

        public void DeleteRank(Rank rank)
        {
            DbContext.Remove(rank);
        }

        public async Task<List<Rank>> GetAllRanksAsync()
        {
            return await DbContext.Rank.ToListAsync();
        }

        public async Task<Rank> GetDefaultRankAsync() 
        {
            return await DbContext.Rank.FirstOrDefaultAsync(r => r.IdPreviousRank == null);
        }

        public async Task<Rank> GetNextRankAsync(Rank currentRank)
        {
            return await DbContext.Rank.FirstOrDefaultAsync(r => r.Id == currentRank.IdNextRank);
        }

        public async Task<Rank> GetPreviousRankAsync(Rank currentRank)
        {
            return await DbContext.Rank.FirstOrDefaultAsync(r => r.Id == currentRank.IdPreviousRank);
        }

        public async Task<Rank> GetRankByIdAsync(Guid rankId)
        {
            return await DbContext.Rank.FirstOrDefaultAsync(r => r.Id == rankId);
        }

        public async Task<Rank> GetRankByNameAsync(string rankName)
        {
            return await DbContext.Rank.FirstOrDefaultAsync(r => r.Name == rankName);
        }

        public void UpdateRank(Rank newRank)
        {
            DbContext.Rank.Update(newRank);
        }
    }
}
