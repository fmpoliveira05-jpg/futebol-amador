using Application.Interfaces.Repositorys;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class MatchRepository: IMatchRepository
    {
        private readonly AmateurFootballContext context;

        public MatchRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        public async Task AddMatch(Matches match)
        {
            if(match == null) { 
                throw new ArgumentNullException("A match enviada está a nulo: ", nameof(match)); 
            }

            await context.Match.AddAsync(match);
        }
    }
}
