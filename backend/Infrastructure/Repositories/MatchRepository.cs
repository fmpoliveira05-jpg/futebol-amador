using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.Pitch;
using Application.DTOs.PostPoneGame;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Google.Api;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class MatchRepository : IMatchRepository
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
                    .ThenInclude(ts => ts.Team)
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
                    .FirstOrDefaultAsync(match => match.Id == idMatch
                                        && match.MatchStatus == MatchStatus.SCHEDULED);
        }

        public async Task<Matches?> GetMatchInProgressByIdAsync(Guid idMatch)
        {
            return await context.Match
                                .Include(m => m.Teams)
                                .FirstOrDefaultAsync(match => match.Id == idMatch
                                                    && match.MatchStatus == MatchStatus.IN_PROGRESS);
        }

        public async Task<Matches?> GetMatchToCancelById(Guid idMatch)
        {
            return await context.Match
                .Include(m => m.Teams).Include(ts => ts.Teams)
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
         */
        public async Task<List<InfoMatchCalendar>> GetAllMatchesTeam(Guid idTeam)
        {
            var query = await (from m in context.Match
                               join pitch in context.Pitch on m.idPitch equals pitch.Id

                               where (m.MatchStatus == MatchStatus.SCHEDULED
                                  || m.MatchStatus == MatchStatus.DONE)
                                  && m.Teams.Any(tm => tm.IdTeam == idTeam)
                                  && m.Teams.Any(tm => tm.IdTeam != idTeam)

                               let myTeam = m.Teams.FirstOrDefault(tm => tm.IdTeam == idTeam)
                               let opponentTeam = m.Teams.FirstOrDefault(tm => tm.IdTeam != idTeam)

                               select new InfoMatchCalendar
                               {
                                   IdMatch = m.Id,
                                   MatchStatus = m.MatchStatus,
                                   GameDate = m.MatchDate,
                                   MatchResult = myTeam.MatchResult,
                                   Result = m.MatchStatus == MatchStatus.DONE
                                      ? (myTeam.NumGoals + " - " + opponentTeam.NumGoals)
                                      : "x-x",
                                   Team = new TeamDto
                                   {
                                       IdTeam = idTeam,
                                       Name = myTeam.Team.Name
                                   },
                                   Opponent = new TeamDto
                                   {
                                       IdTeam = opponentTeam.IdTeam,
                                       Name = opponentTeam.Team.Name
                                   },
                                   pitchGame = new PitchDto
                                   {
                                       Name = pitch.Name,
                                       Address = pitch.Address
                                   }
                               })
                         .ToListAsync();

            return query;
        }

        public async Task<List<InfoMatchCalendar>> GetAllMatchesTeamWithFilters(Guid idTeam, FilterCalendarDto filter)
        {
            var query = context.Match
                .Where(m => m.Teams.Any(tm => tm.IdTeam == idTeam)
                       && m.Teams.Any(tm => tm.IdTeam != idTeam));

            if (filter.IsRealized.HasValue)
            {
                if (filter.IsRealized.Value)
                {
                    query = query.Where(m => m.MatchStatus == MatchStatus.DONE);
                }
                else
                {
                    query = query.Where(m => m.MatchStatus == MatchStatus.SCHEDULED);
                }
            }
            else
            {
                query = query.Where(m => m.MatchStatus == MatchStatus.SCHEDULED ||
                                         m.MatchStatus == MatchStatus.DONE);
            }

            if (filter.IsRanqued.HasValue)
            {
                if (filter.IsRanqued.Value)
                {
                    query = query.Where(m => m.IsCompetive == true);
                }
                else
                {
                    query = query.Where(m => m.IsCompetive == false);
                }
            }


            if (filter.IsHome.HasValue)
            {
                if (filter.IsHome.Value)
                {
                    query = query.Where(m => m.idPitch == m.Teams
                                        .FirstOrDefault(tm => tm.IdTeam == idTeam).Team.IdPitch);
                }
                else
                {
                    query = query.Where(m => m.idPitch == m.Teams
                                        .FirstOrDefault(tm => tm.IdTeam != idTeam).Team.IdPitch);
                }
            }

            if (filter.MinDate.HasValue)
            {
                query = query.Where(m => DateOnly.FromDateTime(m.MatchDate) >= filter.MinDate.Value);
            }

            if (filter.MaxDate.HasValue)
            {
                query = query.Where(m => DateOnly.FromDateTime(m.MatchDate) <= filter.MaxDate.Value);
            }

            if (!string.IsNullOrEmpty(filter.NameOpponent))
            {
                var upperOpponent = filter.NameOpponent.ToUpper();
                query = query.Where(m => m.Teams.FirstOrDefault(tm => tm.IdTeam != idTeam)
                                    .Team.Name.ToUpper().Contains(upperOpponent));
            }

            var list = await query
                .Select(m => new
                {
                    //Declaração de variavies
                    Match = m,
                    MyTeam = m.Teams.FirstOrDefault(tm => tm.IdTeam == idTeam),
                    OpponentTeam = m.Teams.FirstOrDefault(tm => tm.IdTeam != idTeam),
                    Pitch = m.Pitch
                })
                .Select(x => new InfoMatchCalendar
                {
                    IdMatch = x.Match.Id,
                    MatchStatus = x.Match.MatchStatus,
                    GameDate = x.Match.MatchDate,
                    MatchResult = x.MyTeam.MatchResult,
                    Result = x.Match.MatchStatus == MatchStatus.DONE
                           ? (x.MyTeam.NumGoals + " - " + x.OpponentTeam.NumGoals)
                           : "x-x",
                    Team = new TeamDto
                    {
                        IdTeam = idTeam,
                        Name = x.MyTeam.Team.Name
                    },
                    Opponent = new TeamDto
                    {
                        IdTeam = x.OpponentTeam.IdTeam,
                        Name = x.OpponentTeam.Team.Name
                    },
                    pitchGame = new PitchDto
                    {
                        Name = x.Pitch.Name,
                        Address = x.Pitch.Address
                    }
                })
                .ToListAsync();

            return list;
        }

        public async Task<List<InfoPostPoneMatch>> GetAllMatchPostPoneReceiverById(Guid idReceiver)
        {
            var query = (from m in context.Match
                         join ppm in context.PostPoneMatch on m.Id equals ppm.IdMatch

                         where m.MatchStatus == MatchStatus.POST_PONED
                            && m.Teams.Any(tm => tm.IdTeam == idReceiver)
                            && m.Teams.Any(tm => tm.IdTeam != idReceiver)
                            && ppm.IdTeamPostPone == idReceiver

                         let receiverTeam = m.Teams.FirstOrDefault(tm => tm.IdTeam == idReceiver)
                         let opponentTeam = m.Teams.FirstOrDefault(tm => tm.IdTeam != idReceiver)

                         select new InfoPostPoneMatch
                         {
                             IdMatch = m.Id,
                             GameDate = m.MatchDate,
                             PostPoneDate = ppm.PostPoneDate,
                             IdTeam = idReceiver,
                             nameTeam = receiverTeam.Team.Name,
                             IdOpponent = opponentTeam.Team.Id,
                             nameOpponent = opponentTeam.Team.Name
                         })
                         .ToListAsync();

            return await query;
        }

        public async Task<List<InfoPostPoneMatch>> GetAllMatchPostPoneReceiverByIdWithFilters(Guid idReceiver, FilterPostPoneMatchDto filter)
        {
            var query = context.PostPoneMatch
                            .Include(ppm => ppm.Team)
                            .Include(ppm => ppm.Match).ThenInclude(m => m.Teams)
                            .Where(ppm => ppm.Match.MatchStatus == MatchStatus.POST_PONED
                                && ppm.IdTeamPostPone == idReceiver
                                && ppm.Match.Teams.Any(tm => tm.IdTeam == idReceiver)
                                && ppm.Match.Teams.Any(tm => tm.IdTeam != idReceiver));

            if (!string.IsNullOrEmpty(filter.NameOpponent))
            {
                var upperOpponent = filter.NameOpponent.ToUpper();
                query = query.Where(ppm => ppm.Match.Teams.FirstOrDefault(tm => tm.IdTeam != idReceiver)
                                    .Team.Name.ToUpper().Contains(upperOpponent));
            }

            if (filter.IsHome.HasValue)
            {
                if (filter.IsHome.Value)
                {
                    query = query.Where(ppm => ppm.Match.idPitch == ppm.Match.Teams
                                        .FirstOrDefault(tm => tm.IdTeam == idReceiver).Team.IdPitch);
                }
                else
                {
                    query = query.Where(ppm => ppm.Match.idPitch == ppm.Match.Teams
                                        .FirstOrDefault(tm => tm.IdTeam != idReceiver).Team.IdPitch);
                }
            }

            if (filter.MinDateGame.HasValue)
            {
                query = query.Where(ppm => DateOnly.FromDateTime(ppm.Match.MatchDate) >= filter.MinDateGame.Value);
            }

            if (filter.MaxDateGame.HasValue)
            {
                query = query.Where(ppm => DateOnly.FromDateTime(ppm.Match.MatchDate) <= filter.MaxDateGame.Value);
            }

            if (filter.MinDatePostPoneGame.HasValue)
            {
                query = query.Where(ppm => DateOnly.FromDateTime(ppm.PostPoneDate) >= filter.MinDatePostPoneGame.Value);
            }

            if (filter.MaxDatePostPoneGame.HasValue)
            {
                query = query.Where(ppm => DateOnly.FromDateTime(ppm.PostPoneDate) <= filter.MaxDatePostPoneGame.Value);
            }

            var list = await query
                .Select(ppm => new
                {
                    PostPoneMatch = ppm,
                    Match = ppm.Match,
                    MyTeam = ppm.Match.Teams.FirstOrDefault(tm => tm.IdTeam == idReceiver).Team,
                    OpponentTeam = ppm.Match.Teams.FirstOrDefault(tm => tm.IdTeam != idReceiver).Team
                })
                .Select(x => new InfoPostPoneMatch
                {
                    IdMatch = x.Match.Id,
                    GameDate = x.Match.MatchDate,
                    PostPoneDate = x.PostPoneMatch.PostPoneDate,
                    IdTeam = idReceiver,
                    nameTeam = x.MyTeam.Name,
                    IdOpponent = x.OpponentTeam.Id,
                    nameOpponent = x.OpponentTeam.Name,
                })
                .ToListAsync();

            return list;
        }

        public async Task<List<InfoMatchCalendar>> GetMatchesByDateAsync(DateTime date)
        {
            return await context.Match
                .Where(m => m.MatchDate.Date == date.Date)
                .Include(m => m.Teams)
                    .ThenInclude(ts => ts.Team)
                .Include(m => m.Pitch)
                .Select(m => new InfoMatchCalendar
                {
                    IdMatch = m.Id,
                    MatchStatus = m.MatchStatus,
                    GameDate = m.MatchDate,

                    Team = new TeamDto
                    {
                        IdTeam = m.Teams.First().Team.Id,
                        Name = m.Teams.First().Team.Name,
                    },

                    Opponent = new TeamDto
                    {
                        IdTeam = m.Teams.Skip(1).First().Team.Id,
                        Name = m.Teams.Skip(1).First().Team.Name,
                    },

                    pitchGame = new PitchDto
                    {
                        Name = m.Pitch.Name,
                        Address = m.Pitch.Address
                    }
                })
                .ToListAsync();
        }

    }
}
