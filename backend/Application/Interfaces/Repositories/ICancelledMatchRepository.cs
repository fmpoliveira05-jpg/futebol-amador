using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ICancelledMatchRepository
    {
        public Task AddCancelledMatch(CancelledMatch cancelledMatch);
    }
}
