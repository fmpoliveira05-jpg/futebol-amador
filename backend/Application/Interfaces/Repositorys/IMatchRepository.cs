using Domain.Entities;

namespace Application.Interfaces.Repositorys
{
    public interface IMatchRepository
    {
        Task AddMatch(Matches match);
    }
}
