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

        public async Task AddMembershipRequest(MembershipRequest membershipRequest)
        {
            if (membershipRequest == null)
                throw new ArgumentNullException(nameof(membershipRequest), "O pedido de adesão enviado é null");

            await context.MembershipRequests.AddAsync(membershipRequest);
        }

        public Task DeleteMembershipRequest(MembershipRequest membershipRequest)
        {
            if (membershipRequest == null)
                throw new ArgumentNullException(nameof(membershipRequest), "O pedido de adesão enviado é null");

            context.MembershipRequests.Remove(membershipRequest);
            return Task.CompletedTask;
        }

        public async Task<MembershipRequest?> GetMembershipRequestById(Guid id)
        {
            if (id == Guid.Empty) return null;

            return await context.MembershipRequests
                .Include(m => m.Player)
                .Include(m => m.Team)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<MembershipRequest>> GetMembershipRequestsByPlayer(string idPlayer)
        {
            if (idPlayer == string.Empty) return Enumerable.Empty<MembershipRequest>();

            return await context.MembershipRequests
                .Where(m => m.IdPlayer == idPlayer)
                .Include(m => m.Team)
                .Include(m => m.Player)
                .ToListAsync();
        }

        public async Task<IEnumerable<MembershipRequest>> GetMembershipRequestsByTeam(Guid idTeam)
        {
            if (idTeam == Guid.Empty) return Enumerable.Empty<MembershipRequest>();

            return await context.MembershipRequests
                .Where(m => m.IdTeam == idTeam)
                .Include(m => m.Player)
                .Include(m => m.Team)
                .ToListAsync();
        }

        public async Task<MembershipRequest?> GetMembershipRequestByPlayerAndTeam(string idPlayer, Guid idTeam)
        {
            if (idPlayer == string.Empty || idTeam == Guid.Empty) return null;

            return await context.MembershipRequests
                .Include(m => m.Player)
                .Include(m => m.Team)
                .FirstOrDefaultAsync(m => m.IdPlayer == idPlayer && m.IdTeam == idTeam);
        }
    }
}