using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class CancelledMatchRepository: ICancelledMatchRepository
    {
        private readonly AmateurFootballContext context;

        public CancelledMatchRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        public async Task AddCancelledMatch(CancelledMatch cancelledMatch)
        {
            await context.CancelledMatch.AddAsync(cancelledMatch);
        }
    }
}
