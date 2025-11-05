using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IRankRepository
    {
        Task<Rank> GetDefaultRankAsync();

        Task<List<Rank>> GetAllRanksAsync();

        Task<Rank> GetRankByIdAsync(Guid rankId);

        Task<Rank> GetRankByNameAsync(string rankName);

        Task<Rank> GetNextRankAsync(Rank currentRank);

        Task<Rank> GetPreviousRankAsync(Rank currentRank);

        void DeleteRank(Rank rank);

        void UpdateRank(Rank newRank);

        Task AddRankAsync(Rank rank);

        Task AddRankInOtherRankPlaceAsync(Rank rank, Rank nextRank);
    }
}
