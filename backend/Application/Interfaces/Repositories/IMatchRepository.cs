using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IMatchRepository
    {
        Task AddMatch(Matches match);

        Task<Matches?> GetMatchById(Guid idMatch);
    }
}
