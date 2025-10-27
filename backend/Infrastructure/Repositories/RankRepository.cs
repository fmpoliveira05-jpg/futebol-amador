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

        public Task DeleteRank(Rank rank)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRankById(Guid rankId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Rank>> GetAllRanksAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Rank> GetDefaultRankAsync() 
        {
            return await DbContext.Rank.FirstOrDefaultAsync(r => r.IdPreviousRank == null);
        }

        public Task<Rank> GetNextRankAsync(Rank currentRank)
        {
            throw new NotImplementedException();
        }

        public Task<Rank> GetPreviousRankAsync(Rank currentRank)
        {
            throw new NotImplementedException();
        }

        public Task<Rank> GetRankByIdAsync(Guid rankId)
        {
            throw new NotImplementedException();
        }

        public Task<Rank> GetRankByNameAsync(string rankName)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRank(Rank newRank)
        {
            throw new NotImplementedException();
        }
    }
}
