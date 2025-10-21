using Application.DTOs.MatchInvites;
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
            await context.MatchInvite.AddAsync(matchInvite);
        }

        public async Task DeleteMatchInvite(MatchInvite matchInvite)
        {
            context.MatchInvite.Remove(matchInvite);
        }

        public async Task<MatchInvite?> GetMatchInviteById(Guid id)
        {
            return await context.MatchInvite.FirstOrDefaultAsync(mi => mi.Id == id);
        }

        public async Task<List<InfoMatchInviteDTO>> GetAllMatchInviteReceiverById(Guid idReceiver)
        {
            var query = await context.MatchInvite
                .Where(mi => mi.IdReceiver == idReceiver)
                .Include(mi => mi.Sender)
                .Include(mi => mi.Pitch)
                .Select(mi => new InfoMatchInviteDTO
                {
                    Id = mi.Id,
                    IdSender = mi.IdSender,
                    NameSender = mi.Sender.Name,
                    IdReceiver = mi.IdReceiver,
                    NameReceiver = mi.Receiver.Name,
                    GameDate = mi.GameDate,
                    NamePitch = mi.Pitch.Name
                }).ToListAsync();

            return query;
        }

        public async Task<MatchInvite?> GetMatchInvite(SendMatchInviteDTO dto)
        {
            return await context.MatchInvite.FirstOrDefaultAsync(mi => mi.IdSender == dto.IdSender
                                                                    && mi.IdReceiver == dto.IdReceiver
                                                                    && mi.GameDate == dto.GameDate);
        }

        public async Task<MatchInvite?> GetMatchInviteByTeams(Guid idSender, Guid idReceiver)
        {
            return await context.MatchInvite
                .FirstOrDefaultAsync(mi => mi.IdSender == idSender && mi.IdReceiver == idReceiver);
        }

    }
}
