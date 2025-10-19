using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class MembershipRequestRepository : IMembershipRequestRepository
    {
        private readonly AmateurFootballContext context;

        public MembershipRequestRepository(AmateurFootballContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddMembershipRequest(MembershipRequests membershipRequest)
        {
            if (membershipRequest == null)
                throw new ArgumentNullException(nameof(membershipRequest), "O membershipRequest enviado está a nulo");

            await context.MembershipRequests.AddAsync(membershipRequest);
        }

        public Task DeleteMembershipRequest(MembershipRequests membershipRequest)
        {
            if (membershipRequest == null)
                throw new ArgumentNullException(nameof(membershipRequest), "O membershipRequest enviado está a nulo");

            context.MembershipRequests.Remove(membershipRequest);
            return Task.CompletedTask;
        }

        public async Task<MembershipRequests?> GetMembershipRequestById(Guid id)
        {
            if (id == Guid.Empty) return null;

            return await context.MembershipRequests
                .Include(m => m.Player)
                .Include(m => m.Team)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<MembershipRequests>> GetMembershipRequestsByPlayer(Guid idPlayer)
        {
            if (idPlayer == Guid.Empty) return Enumerable.Empty<MembershipRequests>();

            return await context.MembershipRequests
                .Where(m => m.IdPlayer == idPlayer)
                .Include(m => m.Team)
                .Include(m => m.Player)
                .ToListAsync();
        }

        public async Task<IEnumerable<MembershipRequests>> GetMembershipRequestsByTeam(Guid idTeam)
        {
            if (idTeam == Guid.Empty) return Enumerable.Empty<MembershipRequests>();

            return await context.MembershipRequests
                .Where(m => m.IdTeam == idTeam)
                .Include(m => m.Player)
                .Include(m => m.Team)
                .ToListAsync();
        }

        public async Task<MembershipRequests?> GetMembershipRequestByPlayerAndTeam(Guid idPlayer, Guid idTeam)
        {
            if (idPlayer == Guid.Empty || idTeam == Guid.Empty) return null;

            return await context.MembershipRequests
                .Include(m => m.Player)
                .Include(m => m.Team)
                .FirstOrDefaultAsync(m => m.IdPlayer == idPlayer && m.IdTeam == idTeam);
        }
    }
}