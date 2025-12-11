using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.Pitch;
using Application.DTOs.PostPoneGame;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repositório principal para a entidade [Matches].
    /// 
    /// Esta classe é responsável por todas as operações de persistência, consulta e agregação de dados
    /// de partidas, incluindo lógica complexa de carregamento de entidades relacionadas e filtragem.
    /// </summary>
    public class MatchRepository : IMatchRepository
    {
        /// <summary>
        /// O contexto da base de dados ([AmateurFootballContext]) injetado.
        /// Utilizado para aceder às tabelas.
        /// </summary>
        private readonly AmateurFootballContext context;

        /// <summary>
        /// Construtor da classe [MatchRepository].
        /// </summary>
        /// <param name="context">O contexto da base de dados (Db Context) injetado via Dependency Injection.</param>
        public MatchRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Adiciona um novo registo de partida à base de dados.
        /// </summary>
        /// <param name="match">A entidade [Matches] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        public async Task AddMatch(Matches match)
        {
            await context.Match.AddAsync(match);
        }

        /// <summary>
        /// Obtém uma partida pelo seu ID, carregando as estatísticas e os dados básicos das equipas.
        /// </summary>
        /// <remarks>
        /// Utiliza Eager Loading com `.Include` e `.ThenInclude` para carregar: Match -> TeamStatistics -> Team.
        /// </remarks>
        /// <param name="idMatch">O ID (GUID) da partida.</param>
        /// <returns>A entidade [Matches] completa ou null se não for encontrada.</returns>
        public async Task<Matches?> GetMatchById(Guid idMatch)
        {
            return await context.Match
                .Include(m => m.Teams)
                    .ThenInclude(ts => ts.Team)
                .FirstOrDefaultAsync(match => match.Id == idMatch);
        }

        /// <summary>
        /// Obtém uma partida pelo seu ID, carregando toda a hierarquia de dados, incluindo a lista de membros de cada equipa.
        /// </summary>
        /// <remarks>
        /// Utiliza carregamento profundo: Match -> TeamStatistics -> Team -> Members (da Equipa).
        /// </remarks>
        /// <param name="idMatch">O ID (GUID) da partida.</param>
        /// <returns>A entidade [Matches] com todos os membros das equipas participantes carregados.</returns>
        public async Task<Matches?> GetMatchWithListPlayerById(Guid idMatch)
        {
            return await context.Match
                .Include(m => m.Teams).ThenInclude(ts => ts.Team).ThenInclude(t => t.Members)
                .FirstOrDefaultAsync(match => match.Id == idMatch);
        }

        /// <summary>
        /// Obtém uma partida apenas se o seu estado atual for [MatchStatus.SCHEDULED].
        /// </summary>
        /// <param name="idMatch">O ID da partida.</param>
        /// <returns>A entidade [Matches] ou null se o estado não for "SCHEDULED".</returns>
        public async Task<Matches?> GetScheduledMatchById(Guid idMatch)
        {
            return await context.Match
                    .Include(m => m.Teams)
                    .FirstOrDefaultAsync(match => match.Id == idMatch
                                        && match.MatchStatus == MatchStatus.SCHEDULED);
        }

        /// <summary>
        /// Obtém uma partida apenas se o seu estado atual for [MatchStatus.IN_PROGRESS].
        /// </summary>
        /// <param name="idMatch">O ID da partida.</param>
        /// <returns>A entidade [Matches] ou null se o estado não for "IN_PROGRESS".</returns>
        public async Task<Matches?> GetMatchInProgressByIdAsync(Guid idMatch)
        {
            return await context.Match
                                .Include(m => m.Teams)
                                .FirstOrDefaultAsync(match => match.Id == idMatch
                                                    && match.MatchStatus == MatchStatus.IN_PROGRESS);
        }

        /// <summary>
        /// Obtém uma partida que pode ser cancelada (estado SCHEDULED ou POST_PONED).
        /// </summary>
        /// <remarks>
        /// Carrega as estatísticas e os dados básicos das equipas participantes.
        /// </remarks>
        /// <param name="idMatch">O ID da partida.</param>
        /// <returns>A entidade [Matches] ou null se o estado for DONE, IN_PROGRESS ou CANCELED.</returns>
        public async Task<Matches?> GetMatchToCancelById(Guid idMatch)
        {
            return await context.Match
                .Include(m => m.Teams).ThenInclude(ts => ts.Team)
                .FirstOrDefaultAsync(match => match.Id == idMatch
                                    && (match.MatchStatus == MatchStatus.SCHEDULED || match.MatchStatus == MatchStatus.POST_PONED));
        }

        /// <summary>
        /// Obtém uma partida pelo seu ID, carregando as entidades [Pitch] e [Teams].
        /// </summary>
        /// <param name="idMatch">O ID da partida.</param>
        /// <returns>A entidade [Matches] com o local carregado.</returns>
        public async Task<Matches?> GetMatchWitchPitchById(Guid idMatch)
        {
            return await context.Match
                .Include(m => m.Teams)
                .Include(m => m.Pitch)
                .FirstOrDefaultAsync(match => match.Id == idMatch);
        }

        /// <summary>
        /// Verifica se uma equipa tem alguma partida agendada (SCHEDULED ou POST_PONED) dentro de um intervalo de 12 horas da [gameDate] fornecida.
        /// </summary>
        /// <remarks>
        /// Utiliza a função nativa do SQL Server [EF.Functions.DateDiffMinute] para calcular o intervalo de tempo na base de dados.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa a verificar.</param>
        /// <param name="gameDate">A data e hora de referência para a comparação.</param>
        /// <returns>A partida [Matches] encontrada ou null se não houver conflito nas 12h.</returns>
        public async Task<Matches?> GetMatchProxim12HoursMatchs(Guid idTeam, DateTime gameDate)
        {
            var minDate = gameDate.AddHours(-12);
            var maxDate = gameDate.AddHours(12);

            var query = await context.Match
                    .Include(m => m.Teams)
                    .Where(m => m.Teams.Any(t => t.IdTeam == idTeam)
                         && m.MatchDate >= minDate
                         && m.MatchDate <= maxDate
                         && (m.MatchStatus == MatchStatus.SCHEDULED || m.MatchStatus == MatchStatus.POST_PONED))
                    .FirstOrDefaultAsync();

            return query;
        }

        /// <summary>
        /// Obtém o calendário de uma equipa, incluindo partidas agendadas e finalizadas.
        /// </summary>
        /// <remarks>
        /// Utiliza LINQ to Entities (sintaxe de consulta) para agregar dados de [Match], [Pitch] e [TeamStatistics]
        /// e projetar o resultado no DTO [InfoMatchCalendar].
        /// </remarks>
        /// <param name="idTeam">O ID da equipa cujo calendário se pretende.</param>
        /// <returns>Uma lista de objetos [InfoMatchCalendar] para o frontend.</returns>
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
                                   IsCompetitive = m.IsCompetive,
                                   Team = new TeamStatisticsDto
                                   {
                                       IdTeam = idTeam,
                                       Name = myTeam.Team.Name,
                                       NumGoals = myTeam.NumGoals
                                   },
                                   Opponent = new TeamStatisticsDto
                                   {
                                       IdTeam = opponentTeam.IdTeam,
                                       Name = opponentTeam.Team.Name,
                                       NumGoals = opponentTeam.NumGoals
                                   },
                                   PitchGame = new PitchDto
                                   {
                                       Name = pitch.Name,
                                       Address = pitch.Address
                                   },
                                   IsHome = m.idPitch == myTeam.Team.IdPitch
                               })
                         .ToListAsync();

            return query;
        }

        /// <summary>
        /// Obtém o calendário de uma equipa, aplicando filtros complexos.
        /// </summary>
        /// <remarks>
        /// Este método constrói a consulta dinamicamente, permitindo filtrar por:
        /// - Status do jogo (Realizado, Agendado ou Ambos).
        /// - Tipo de jogo (Competitivo vs Casual).
        /// - Local (Casa/Fora).
        /// - Intervalo de datas.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa cujo calendário se pretende.</param>
        /// <param name="filter">O DTO contendo os critérios de filtragem (opcional).</param>
        /// <returns>Uma lista de objetos [InfoMatchCalendar] filtrados.</returns>
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
                    IsCompetitive = x.Match.IsCompetive,
                    Team = new TeamStatisticsDto
                    {
                        IdTeam = idTeam,
                        Name = x.MyTeam.Team.Name,
                        NumGoals = x.MyTeam.NumGoals
                    },
                    Opponent = new TeamStatisticsDto
                    {
                        IdTeam = x.OpponentTeam.IdTeam,
                        Name = x.OpponentTeam.Team.Name,
                        NumGoals = x.OpponentTeam.NumGoals
                    },
                    PitchGame = new PitchDto
                    {
                        Name = x.Pitch.Name,
                        Address = x.Pitch.Address
                    },
                    IsHome = x.Match.idPitch == x.MyTeam.Team.IdPitch

                })
                .ToListAsync();

            return list;
        }

        /// <summary>
        /// Obtém a lista de pedidos de adiamento recebidos por uma equipa.
        /// </summary>
        /// <remarks>
        /// Utiliza uma consulta LINQ to Entities para carregar os pedidos de adiamento associados a partidas
        /// onde a equipa é o recetor do pedido.
        /// </remarks>
        /// <param name="idReceiver">O ID da equipa que recebeu o pedido.</param>
        /// <returns>Uma lista de [InfoPostPoneMatch] com os detalhes dos pedidos pendentes.</returns>
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
                             Team = new TeamDto
                             {
                                 IdTeam = idReceiver,
                                 Name = receiverTeam.Team.Name
                             },
                             Opponent = new TeamDto{
                                IdTeam = opponentTeam.Team.Id,
                                Name = opponentTeam.Team.Name
                             }
                         })
                         .ToListAsync();

            return await query;
        }

        /// <summary>
        /// Obtém a lista de pedidos de adiamento recebidos por uma equipa, aplicando filtros complexos.
        /// </summary>
        /// <remarks>
        /// Carrega os dados de [PostPoneMatch] com Eager Loading para [Match] e [Team].
        /// </remarks>
        /// <param name="idReceiver">O ID da equipa que recebeu o pedido.</param>
        /// <param name="filter">O DTO com os critérios de filtragem (Nome, Datas de Jogo Original, Datas de Adiamento Proposta).</param>
        /// <returns>Uma lista de [InfoPostPoneMatch] filtrada.</returns>
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
                    Team = new TeamDto
                    {
                        IdTeam = idReceiver,
                        Name = x.MyTeam.Name
                    },
                    Opponent =
                    {
                        IdTeam = x.OpponentTeam.Id,
                        Name = x.OpponentTeam.Name,
                    }
                })
                .ToListAsync();

            return list;
        }

        /// <summary>
        /// Obtém a lista de partidas (jogos agendados/finalizados) que ocorrem numa data específica.
        /// </summary>
        /// <param name="date">A data de referência.</param>
        /// <returns>Uma lista de objetos [InfoMatchCalendar] para o frontend.</returns>
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
                    IsCompetitive = m.IsCompetive,
                    GameDate = m.MatchDate,
                    Team = new TeamStatisticsDto
                    {
                        IdTeam = m.Teams.First().Team.Id,
                        Name = m.Teams.First().Team.Name,
                        NumGoals = m.Teams.First().NumGoals,
                    },
                    Opponent = new TeamStatisticsDto
                    {
                        IdTeam = m.Teams.Skip(1).First().Team.Id,
                        Name = m.Teams.Skip(1).First().Team.Name,
                        NumGoals = m.Teams.Skip(1).First().NumGoals,
                    },
                    PitchGame = new PitchDto
                    {
                        Name = m.Pitch.Name,
                        Address = m.Pitch.Address
                    },
                    IsHome = m.idPitch == m.Teams.First().Team.IdPitch
                })
                .ToListAsync();
        }
    }
}