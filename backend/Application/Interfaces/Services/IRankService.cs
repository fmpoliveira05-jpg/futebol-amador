using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    internal interface IRankService
    {
        //ACABAR DE VER ISTO TAMBÉM
        Task<Rank> GetDefaultRankAsync();
        Task<List<Rank>> GetAllRanksAsync();
        Task<Rank> GetRankByIdAsync(Guid rankId);
        Task<Rank> GetRankByNameAsync(string rankName);
        Task<Rank> GetNextRankAsync(Rank currentRank);
        Task<Rank> GetPreviousRankAsync(Rank currentRank);
        Task DeleteRank(Rank rank);
        Task DeleteRankById(Guid rankId);
        Task<Rank> UpdateRank(Rank newRank);
        Task AddRankAsync(Rank rank);
        Task AddRankInOtherRankPlaceAsync(Rank rank, Rank nextRank);
        Task ChangePreviousRank(Rank rank, Rank newPreviousRank);

    }
}
