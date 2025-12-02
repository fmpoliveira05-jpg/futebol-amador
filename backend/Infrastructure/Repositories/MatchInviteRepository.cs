using Application.DTOs.Filters;
using Application.DTOs.MatchInvites;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
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

        public void DeleteMatchInvite(MatchInvite matchInvite)
        {
            context.MatchInvite.Remove(matchInvite);
        }

        public async Task<MatchInvite?> GetMatchInviteById(Guid id)
        {
            return await context.MatchInvite.FirstOrDefaultAsync(mi => mi.Id == id);
        }

        public async Task<MatchInvite?> GetMatchInvite(Guid idSender, Guid idReceiver, DateTime gameDate)
        {
            return await context.MatchInvite.FirstOrDefaultAsync(mi => mi.IdSender == idSender
                                                                    && mi.IdReceiver == idReceiver
                                                                    && mi.GameDate == gameDate);
        }

        public async Task<MatchInvite?> GetMatchInviteWithPitchByTeams(Guid idSender, Guid idReceiver)
        {
            return await context.MatchInvite
                .Include(mi => mi.Sender)
                .Include(mi => mi.Receiver)
                .Include(mi => mi.Pitch)
                .FirstOrDefaultAsync(mi => mi.IdSender == idSender && mi.IdReceiver == idReceiver);
        }

        public async Task<List<InfoMatchInviteDto>> GetAllMatchInviteReceiverById(Guid idReceiver)
        {
            var query = await context.MatchInvite
                .Where(mi => mi.IdReceiver == idReceiver)
                .Include(mi => mi.Sender)
                .Include(mi => mi.Pitch)
                .Select(mi => new InfoMatchInviteDto
                {
                    Id = mi.Id,

                    Sender = new TeamDto
                    {
                        IdTeam = mi.IdSender,
                        Name = mi.Sender.Name
                    },

                    Receiver = new TeamDto
                    {
                        IdTeam = mi.IdReceiver,
                        Name = mi.Receiver.Name
                    },

                    GameDate = mi.GameDate,
                    NamePitch = mi.Pitch.Name
                }).ToListAsync();

            return query;
        }

        public async Task<List<InfoMatchInviteDto>> GetAllMatchInvitesTeamWithFilters(Guid idReceiver, FilterMatchInvitesDto filter)
        {
            var senderName = filter.SenderName;
            var minDate = filter.MinDate;
            var maxDate = filter.MaxDate;

            var query = context.MatchInvite.AsQueryable();

            query = query.Where(mi => mi.IdReceiver == idReceiver);

            if (!string.IsNullOrEmpty(senderName))
            {
                query = query.Where(mi => mi.Sender.Name.ToUpper().Contains(senderName.ToUpper()));
            }

            if (minDate != null)
            {
                query = query.Where(mi => DateOnly.FromDateTime(mi.GameDate) >= minDate);
            }

            if (maxDate != null)
            {
                query = query.Where(mi => DateOnly.FromDateTime(mi.GameDate) <= maxDate);
            }

            var list = await query
                .Select(mi => new InfoMatchInviteDto
                {
                    Id = mi.Id,

                    Sender = new TeamDto
                    {
                        IdTeam = mi.IdSender,
                        Name = mi.Sender.Name
                    },

                    Receiver = new TeamDto
                    {
                        IdTeam = mi.IdReceiver,
                        Name = mi.Receiver.Name
                    },

                    GameDate = mi.GameDate,
                    NamePitch = mi.Pitch.Name
                }).ToListAsync();

            return list;
        }
    }
}
