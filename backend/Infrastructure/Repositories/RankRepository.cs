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
        public async Task<Rank> GetDefaultRankAsync() 
        {
            return await DbContext.Rank.FirstOrDefaultAsync(r => r.Name == ModelConstants.GeneralConst.DefaultRankName);
        }
    }
}
