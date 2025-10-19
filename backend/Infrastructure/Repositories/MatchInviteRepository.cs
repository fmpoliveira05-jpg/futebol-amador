using Application.Interfaces.Repositorys;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class MatchInviteRepository : IMatchInviteRepository
    {
        private readonly AmateurFootballContext context;

        public MatchInviteRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        public async Task AddMatchInvite(MatchInvite matchInvite)
        {
            if(matchInvite == null)
            {
                throw new ArgumentNullException("A matchInvite enviada está a nulo: ", nameof(matchInvite));
            }

            await context.MatchInvite.AddAsync(matchInvite);
        }

        public async Task DeleteMatchInvite(MatchInvite matchInvite)
        {
            if (matchInvite == null)
            {
                throw new ArgumentNullException("A matchInvite enviada está a nulo: ", nameof(matchInvite));
            }

            context.MatchInvite.Remove(matchInvite);
        }

        public async Task<MatchInvite?> GetMatchInviteById(Guid id)
        {
            return await context.MatchInvite.FirstOrDefaultAsync(mi => mi.Id == id);
        }

        public async Task<MatchInvite?> GetMatchInviteByTeams(Guid idSender, Guid idReceiver)
        {
            return await context.MatchInvite
                .FirstOrDefaultAsync(mi => mi.IdSender == idSender && mi.IdReceiver == idReceiver);
        }

    }
}
