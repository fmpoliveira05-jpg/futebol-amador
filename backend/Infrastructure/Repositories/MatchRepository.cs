using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Repositorys;
using Domain.Entities;
using Domain.Enums;
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
            await context.Match.AddAsync(match);
        }

        public async Task<Matches?> GetMatchById(Guid idMatch)
        {
            return await context.Match
                .Include(m => m.Teams)
                .FirstOrDefaultAsync(match => match.Id == idMatch);
        }

        public async Task<Matches?> GetMatchWithListPlayerById(Guid idMatch)
        {
            return await context.Match
                .Include(m => m.Teams).ThenInclude(ts => ts.Team).ThenInclude(t => t.Members)
                .FirstOrDefaultAsync(match => match.Id == idMatch);
        }

        public async Task<Matches?> GetScheduledMatchById(Guid idMatch) 
        {
            return await context.Match
                    .Include(m => m.Teams)
                    .FirstOrDefaultAsync(match => match.Id == idMatch && match.MatchStatus == MatchStatus.SCHEDULED);
        }

        public async Task<Matches?> GetMatchValideToCancelById(Guid idMatch)
        {
            return await context.Match
                .Include(m => m.Teams)
                .FirstOrDefaultAsync(match => match.Id == idMatch
                                    && match.MatchStatus == MatchStatus.SCHEDULED || match.MatchStatus == MatchStatus.POST_PONED);
        }

        public async Task<Matches?> GetMatchWitchPitchById(Guid idMatch)
        {
            return await context.Match
                .Include(m => m.Teams)
                .Include(m => m.Pitch)
                .FirstOrDefaultAsync(match => match.Id == idMatch);
        }

        public async Task<Matches?> GetMatchProxim12HoursMatchs(Guid idTeam, DateTime gameDate)
        {
            const int totalHours = 12;
            const int totalMinutes = 60 * totalHours;

            var query = await context.Match
                .Include(m => m.Teams)
                .Where(m => m.Teams.Any(t => t.IdTeam == idTeam)
                    && EF.Functions.DateDiffMinute(m.MatchDate, gameDate) <= totalMinutes
                    && EF.Functions.DateDiffMinute(m.MatchDate, gameDate) >= -totalMinutes
                    && (m.MatchStatus == MatchStatus.SCHEDULED || m.MatchStatus == MatchStatus.POST_PONED))
                .FirstOrDefaultAsync();

            return query; 
        }

        /***
         Busca todos os match agendados de e finalizados de uma equipa
         Calendario

        Nota depois fazer um com filtros (onde tem um var para cada um e depois se for usado ou não é alterado o valor recebido)
        Criar DTO para os filtros
         */
        public async Task<List<MatchDto>> GetAllMatchesTeam(Guid idTeam)
        {
            var query = await context.Match
                .Include(m => m.Pitch)
                .Include(m => m.Teams).ThenInclude(ts => ts.Team)
                .Where(m => m.MatchStatus == MatchStatus.SCHEDULED || m.MatchStatus == MatchStatus.DONE)
                .Where(m => m.Teams.Any(ts => ts.IdTeam == idTeam) && m.Teams.Any(ts => ts.IdTeam != idTeam))
                .Select(m => new MatchDto { 
                    IdMatch = m.Id,
                    GameDate = m.MatchDate,
                    NameTeam = m.Teams.FirstOrDefault(ts => ts.IdTeam == idTeam).Team.Name,
                    NameOpponent = m.Teams.FirstOrDefault(ts => ts.IdTeam != idTeam).Team.Name,
                    NamePitch = m.Pitch.Name
                })
                .ToListAsync();

            return query;
        }

        public async Task<List<InfoPostPoneMatch>> GetAllMatchPostPoneReceiverById(Guid idReceiver)
        {
            var query = (from m in context.Match
                         join ppm in context.PostPoneMatch on m.Id equals ppm.IdMatch

                         where m.MatchStatus == MatchStatus.POST_PONED //Procurar matchs adiadas
                            && m.Teams.Any(tm => tm.IdTeam == idReceiver) //Match adiadas que o recetor participa
                            && ppm.IdTeamPostPone != idReceiver //E que ele não adiou

                         let receiverTeam = m.Teams.FirstOrDefault(tm => tm.IdTeam == idReceiver)
                         let opponentTeam = m.Teams.FirstOrDefault(tm => tm.IdTeam != idReceiver)

                         select new InfoPostPoneMatch
                         {
                             IdMatch = m.Id,
                             PostPoneDate = ppm.PostPoneDate,
                             IdTeam = idReceiver,
                             nameTeam = receiverTeam.Team.Name,
                             IdOpponent = opponentTeam.Team.Id,
                             nameOpponent = opponentTeam.Team.Name
                         })
                         .ToListAsync();

            return await query;
        }
    }
}
