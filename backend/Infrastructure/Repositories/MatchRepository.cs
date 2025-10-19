using Application.Interfaces.Repositorys;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
            if (match == null) { 
                throw new ArgumentNullException("A match enviada está a nulo: ", nameof(match)); 
            }

            await context.Match.AddAsync(match);
        }

        public async Task<Matches?> GetMatchById(Guid idMatch)
        {
            if (idMatch == Guid.Empty) {
                throw new ArgumentNullException("O id da match não pode estar a nulo", nameof(idMatch));
            }

            return await context.Match.FirstOrDefaultAsync(match => match.Id == idMatch);
        }
    }
}
