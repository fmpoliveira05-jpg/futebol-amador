using Application.Competition;
using Application.DTOs.Competition;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Application.Services.Competition
{
    /// <summary>
    /// Ligas e épocas: classificação calculada a partir dos jogos, sorteio a duas voltas, inscrições,
    /// fecho da época com troféu, subidas e descidas (ver docs/novas-funcionalidades.md, D1 a D5).
    /// </summary>
    public class LeagueService : ILeagueService
    {
        /// <summary>Quantos dias antes do início o sorteio é feito automaticamente.</summary>
        public const int DrawDaysBeforeStart = 7;

        private readonly ICompetitionRepository repo;
        private readonly IPlayerAuthorizationService authorization;
        private readonly IUnityOfWork unitOfWork;
        private readonly INotificationService notifications;
        private readonly TimeProvider clock;
        private readonly ILogger<LeagueService> logger;
        private readonly Random random;

        public LeagueService(ICompetitionRepository repo, IPlayerAuthorizationService authorization, IUnityOfWork unitOfWork,
            INotificationService notifications, TimeProvider clock, ILogger<LeagueService> logger)
            : this(repo, authorization, unitOfWork, notifications, clock, logger, Random.Shared)
        {
        }

        /// <summary>Construtor com o gerador aleatório do sorteio (para os testes).</summary>
        public LeagueService(ICompetitionRepository repo, IPlayerAuthorizationService authorization, IUnityOfWork unitOfWork,
            INotificationService notifications, TimeProvider clock, ILogger<LeagueService> logger, Random random)
        {
            this.repo = repo;
            this.authorization = authorization;
            this.unitOfWork = unitOfWork;
            this.notifications = notifications;
            this.clock = clock;
            this.logger = logger;
            this.random = random;
        }

        private DateTime Now => clock.GetUtcNow().UtcDateTime;

        #region Consultas

        public async Task<List<LeagueDto>> GetLeaguesAsync()
        {
            var leagues = await repo.GetLeaguesAsync();
            var counts = await repo.CountTeamsByLeagueAsync();
            return leagues.Select(l => ToDto(l, counts.GetValueOrDefault(l.Id))).ToList();
        }

        public async Task<StandingsDto?> GetStandingsAsync(Guid? leagueId, Guid? seasonId)
        {
            var league = leagueId.HasValue ? await repo.GetLeagueAsync(leagueId.Value) : await repo.GetTopLeagueAsync();
            if (league == null)
            {
                if (leagueId.HasValue)
                {
                    throw new NotFoundException("A liga não existe.");
                }

                return null;
            }

            var counts = await repo.CountTeamsByLeagueAsync();
            Season? season;
            if (seasonId.HasValue)
            {
                season = await repo.GetSeasonAsync(seasonId.Value);
                if (season == null || season.IdLeague != league.Id)
                {
                    throw new NotFoundException("A época não existe nesta liga.");
                }
            }
            else
            {
                var current = CurrentSeason(league);
                season = current == null ? null : await repo.GetSeasonAsync(current.Id);
            }

            List<Team> teams;
            List<FinishedMatch> finished = new();
            if (season != null && season.Teams.Count > 0)
            {
                teams = season.Teams.Select(st => st.Team).ToList();
                var matches = await repo.GetSeasonMatchesAsync(season.Id);
                finished = matches.Select(ToFinished).Where(f => f != null).Select(f => f!).ToList();
            }
            else
            {
                teams = await repo.GetTeamsOfLeagueAsync(league.Id);
            }

            var order = teams.OrderBy(t => t.Name, StringComparer.Create(new CultureInfo("pt-PT"), true)).ToList();
            var rows = StandingsCalculator.Calculate(order.Select(t => t.Id).ToList(), finished);
            var byId = teams.ToDictionary(t => t.Id);
            var above = await repo.GetAdjacentLeagueAsync(league.Level, above: true) != null;
            var below = await repo.GetAdjacentLeagueAsync(league.Level, above: false) != null;

            return new StandingsDto
            {
                League = ToDto(league, counts.GetValueOrDefault(league.Id)),
                Season = season == null ? null : ToDto(season),
                Rows = rows.Select((r, i) => new StandingRowDto
                {
                    Position = i + 1,
                    TeamId = r.TeamId,
                    TeamName = byId[r.TeamId].Name,
                    Icon = byId[r.TeamId].Icon,
                    Played = r.Played,
                    Won = r.Won,
                    Drawn = r.Drawn,
                    Lost = r.Lost,
                    GoalsFor = r.GoalsFor,
                    GoalsAgainst = r.GoalsAgainst,
                    GoalDifference = r.GoalDifference,
                    Points = r.Points,
                    Form = r.Form.ToList(),
                    Zone = StandingsCalculator.Zone(i + 1, rows.Count, league.PromotionSpots, league.RelegationSpots, above, below),
                }).ToList(),
            };
        }

        public async Task<List<FixtureRoundDto>> GetFixturesAsync(Guid seasonId)
        {
            var season = await repo.GetSeasonAsync(seasonId) ?? throw new NotFoundException("A época não existe.");
            var matches = await repo.GetSeasonMatchesAsync(season.Id);

            return matches
                .GroupBy(m => m.Round ?? 0)
                .OrderBy(g => g.Key)
                .Select(g => new FixtureRoundDto
                {
                    Round = g.Key,
                    Matches = g.OrderBy(m => m.MatchDate).Select(ToFixtureDto).Where(f => f != null).Select(f => f!).ToList(),
                })
                .ToList();
        }

        public async Task<List<TeamTitleDto>> GetTitlesAsync(Guid teamId)
        {
            var titles = await repo.GetTitlesAsync(teamId);
            return GroupTitles(titles);
        }

        /// <summary>Agrupa os títulos por troféu ("x3"), do troféu mais ganho para o menos ganho.</summary>
        public static List<TeamTitleDto> GroupTitles(IEnumerable<TeamTitle> titles) => titles
            .GroupBy(t => (t.TrophyName, t.LeagueName))
            .Select(g => new TeamTitleDto
            {
                TrophyName = g.Key.TrophyName,
                LeagueName = g.Key.LeagueName,
                Count = g.Count(),
                Seasons = g.OrderBy(t => t.WonAt).Select(t => t.SeasonName).ToList(),
            })
            .OrderByDescending(t => t.Count)
            .ThenBy(t => t.TrophyName)
            .ToList();

        #endregion

        #region Gestão (super administrador e administradores de equipa)

        public async Task<LeagueDto> CreateLeagueAsync(string? userId, CreateLeagueDto dto)
        {
            await RequireSuperAdmin(userId);

            if (await repo.LevelExistsAsync(dto.Level))
            {
                throw new ValidationException($"Já existe uma liga no escalão {dto.Level}.");
            }

            var league = new League
            {
                Name = dto.Name.Trim(),
                Level = dto.Level,
                PromotionSpots = dto.PromotionSpots,
                RelegationSpots = dto.RelegationSpots,
                SeasonDurationDays = dto.SeasonDurationDays,
                TrophyName = dto.TrophyName.Trim(),
            };

            await repo.AddLeagueAsync(league);
            await unitOfWork.SaveChangesAsync();
            return ToDto(league, 0);
        }

        public async Task<SeasonDto> CreateSeasonAsync(string? userId, Guid leagueId, CreateSeasonDto dto)
        {
            await RequireSuperAdmin(userId);
            var league = await repo.GetLeagueAsync(leagueId) ?? throw new NotFoundException("A liga não existe.");

            if (await repo.GetOpenSeasonAsync(leagueId) != null)
            {
                throw new BusinessRuleException("Esta liga já tem uma época por terminar.");
            }

            var season = await CreateSeasonInternal(league, DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc), dto.Name);
            await unitOfWork.SaveChangesAsync();
            return ToDto(season);
        }

        public async Task<SeasonDto> RegisterTeamAsync(string? userId, Guid leagueId, Guid teamId)
        {
            await authorization.UserAuthorizationIsAdminTeamById(userId!, teamId);

            var league = await repo.GetLeagueAsync(leagueId) ?? throw new NotFoundException("A liga não existe.");
            var season = await repo.GetOpenSeasonAsync(leagueId);
            if (season == null || season.Status != SeasonStatus.REGISTRATION)
            {
                throw new BusinessRuleException("Esta liga não tem inscrições abertas.");
            }

            var registrations = await repo.GetOpenRegistrationsOfTeamAsync(teamId);
            if (registrations.Any(r => r.Season.Status == SeasonStatus.IN_PROGRESS))
            {
                throw new BusinessRuleException("A equipa está a meio de uma época noutra liga.");
            }

            if (registrations.Any(r => r.IdSeason == season.Id))
            {
                throw new BusinessRuleException("A equipa já está inscrita nesta época.");
            }

            foreach (var other in registrations)
            {
                repo.RemoveSeasonTeam(other);
            }

            var team = await repo.GetTeamWithPitchAsync(teamId) ?? throw new NotFoundException("A equipa não existe.");
            team.IdLeague = league.Id;
            await repo.AddSeasonTeamAsync(new SeasonTeam { IdSeason = season.Id, IdTeam = teamId });
            await unitOfWork.SaveChangesAsync();

            // Depois de gravar, o EF já juntou a nova inscrição a season.Teams.
            return ToDto(season);
        }

        public async Task<SeasonDto> StartSeasonAsync(string? userId, Guid seasonId, StartSeasonDto dto)
        {
            await RequireSuperAdmin(userId);
            var kickoff = ParseKickoff(dto.KickoffTime);
            var season = await StartInternal(seasonId, kickoff, manual: true);
            return ToDto(season);
        }

        public async Task<SeasonDto> CloseSeasonAsync(string? userId, Guid seasonId)
        {
            await RequireSuperAdmin(userId);
            var season = await CloseInternal(seasonId);
            return ToDto(season);
        }

        public async Task<int> RunScheduledTasksAsync(CancellationToken ct = default)
        {
            var done = 0;

            foreach (var s in await repo.GetSeasonsToStartAsync(Now.AddDays(DrawDaysBeforeStart)))
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    await StartInternal(s.Id, ParseKickoff(null), manual: false);
                    done++;
                }
                catch (BusinessRuleException ex)
                {
                    // Por exemplo, menos de duas equipas inscritas: tenta-se outra vez na próxima volta.
                    logger.LogInformation("Época {Season} ainda não pode começar: {Motivo}", s.Id, ex.Message);
                }
            }

            foreach (var s in await repo.GetSeasonsToCloseAsync(Now))
            {
                ct.ThrowIfCancellationRequested();
                await CloseInternal(s.Id);
                done++;
            }

            return done;
        }

        #endregion

        #region Sorteio e fecho

        private async Task<Season> StartInternal(Guid seasonId, TimeSpan kickoff, bool manual)
        {
            var season = await repo.GetSeasonAsync(seasonId) ?? throw new NotFoundException("A época não existe.");
            if (season.Status != SeasonStatus.REGISTRATION)
            {
                throw new BusinessRuleException("A época já começou ou já terminou.");
            }

            var teams = season.Teams.Select(st => st.Team).DistinctBy(t => t.Id).ToList();
            if (teams.Count < 2)
            {
                throw new BusinessRuleException("São precisas pelo menos duas equipas inscritas para sortear o calendário.");
            }

            // A primeira jornada nunca fica para daqui a menos de dois dias: dá tempo para os onzes.
            var earliest = Now.Date.AddDays(2);
            var firstRound = season.StartDate.Date < earliest ? earliest : season.StartDate.Date;
            var fixtures = FixtureGenerator.DoubleRoundRobin(teams.Select(t => t.Id).ToList(), random);
            var totalRounds = fixtures.Max(f => f.Round);
            var byId = teams.ToDictionary(t => t.Id);

            var matches = fixtures.Select(f =>
            {
                var home = byId[f.HomeTeamId];
                var away = byId[f.AwayTeamId];
                var date = FixtureGenerator.RoundDate(firstRound, f.Round, totalRounds, season.League.SeasonDurationDays, kickoff);
                var match = new Matches(date, true, home.IdPitch, new List<TeamStatistics> { new(home), new(away) })
                {
                    IdSeason = season.Id,
                    Round = f.Round,
                    IdHomeTeam = home.Id,
                };
                return match;
            }).ToList();

            await repo.AddMatchesAsync(matches);
            season.StartDate = firstRound;
            season.EndDate = firstRound.AddDays(season.League.SeasonDurationDays);
            season.Status = SeasonStatus.IN_PROGRESS;
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Sorteio da época {Season} ({Liga}): {Jogos} jogos em {Jornadas} jornadas ({Modo}).",
                season.Name, season.League.Name, matches.Count, totalRounds, manual ? "manual" : "automático");

            foreach (var team in teams)
            {
                await Notify(team.Id, "Calendário da liga sorteado",
                    $"O calendário da {season.League.Name} {season.Name} já saiu: {totalRounds} jornadas a começar a {firstRound:dd/MM}.");
            }

            return season;
        }

        private async Task<Season> CloseInternal(Guid seasonId)
        {
            var season = await repo.GetSeasonAsync(seasonId) ?? throw new NotFoundException("A época não existe.");
            if (season.Status != SeasonStatus.IN_PROGRESS)
            {
                throw new BusinessRuleException("Só se pode fechar uma época a decorrer.");
            }

            var league = season.League;
            var teams = season.Teams.Select(st => st.Team).ToList();
            var matches = await repo.GetSeasonMatchesAsync(season.Id);
            var finished = matches.Select(ToFinished).Where(f => f != null).Select(f => f!).ToList();
            var order = teams.OrderBy(t => t.Name, StringComparer.Ordinal).Select(t => t.Id).ToList();
            var table = StandingsCalculator.Calculate(order, finished);

            // 1. Troféu para o campeão (se se jogou alguma coisa).
            if (table.Count > 0 && table[0].Played > 0)
            {
                await repo.AddTitleAsync(new TeamTitle
                {
                    IdTeam = table[0].TeamId,
                    IdSeason = season.Id,
                    IdLeague = league.Id,
                    TrophyName = league.TrophyName,
                    LeagueName = league.Name,
                    SeasonName = season.Name,
                    WonAt = Now,
                });
            }

            // Os jogos da época que ficaram por jogar deixam de estar marcados.
            foreach (var m in matches.Where(m => m.MatchStatus is MatchStatus.SCHEDULED or MatchStatus.POST_PONED))
            {
                m.MatchStatus = MatchStatus.CANCELED;
            }

            season.Status = SeasonStatus.FINISHED;
            season.EndDate = season.EndDate > Now ? Now : season.EndDate;

            // 2. Subidas e descidas.
            var above = await repo.GetAdjacentLeagueAsync(league.Level, above: true);
            var below = await repo.GetAdjacentLeagueAsync(league.Level, above: false);
            var byId = teams.ToDictionary(t => t.Id);
            var movedUp = new List<Team>();
            var movedDown = new List<Team>();

            for (var i = 0; i < table.Count; i++)
            {
                var zone = StandingsCalculator.Zone(i + 1, table.Count, league.PromotionSpots, league.RelegationSpots, above != null, below != null);
                var team = byId[table[i].TeamId];
                if (zone == "PROMOTION")
                {
                    team.IdLeague = above!.Id;
                    movedUp.Add(team);
                }
                else if (zone == "RELEGATION")
                {
                    team.IdLeague = below!.Id;
                    movedDown.Add(team);
                }
                else
                {
                    team.IdLeague = league.Id;
                }
            }

            await unitOfWork.SaveChangesAsync();

            // 3. As equipas que mudaram de liga entram nas inscrições abertas da nova liga, se houver.
            await RegisterInOpenSeason(above, movedUp);
            await RegisterInOpenSeason(below, movedDown);

            // 4. A época seguinte desta liga fica logo em inscrições, uma semana depois.
            if (await repo.GetOpenSeasonAsync(league.Id) == null)
            {
                await CreateSeasonInternal(league, Now.Date.AddDays(DrawDaysBeforeStart + 7), null);
            }

            await unitOfWork.SaveChangesAsync();

            if (table.Count > 0 && table[0].Played > 0)
            {
                await Notify(table[0].TeamId, "Campeões!", $"A equipa ganhou a {league.Name} {season.Name} e recebeu a {league.TrophyName}.");
            }

            foreach (var t in movedUp)
            {
                await Notify(t.Id, "Subida de divisão", $"A equipa sobe para a {above!.Name}.");
            }

            foreach (var t in movedDown)
            {
                await Notify(t.Id, "Descida de divisão", $"A equipa desce para a {below!.Name}.");
            }

            return season;
        }

        private async Task RegisterInOpenSeason(League? league, List<Team> teams)
        {
            if (league == null || teams.Count == 0)
            {
                return;
            }

            var open = await repo.GetOpenSeasonAsync(league.Id);
            if (open == null || open.Status != SeasonStatus.REGISTRATION)
            {
                return; // entram automaticamente quando a próxima época dessa liga for criada
            }

            foreach (var team in teams.Where(t => open.Teams.All(st => st.IdTeam != t.Id)))
            {
                await repo.AddSeasonTeamAsync(new SeasonTeam { IdSeason = open.Id, IdTeam = team.Id });
            }
        }

        private async Task<Season> CreateSeasonInternal(League league, DateTime startDate, string? name)
        {
            var season = new Season
            {
                IdLeague = league.Id,
                League = league,
                Name = string.IsNullOrWhiteSpace(name) ? PlayerStatsCalculator.SportsSeason(startDate) : name.Trim(),
                Status = SeasonStatus.REGISTRATION,
                StartDate = startDate,
                EndDate = startDate.AddDays(league.SeasonDurationDays),
            };

            await repo.AddSeasonAsync(season);
            foreach (var team in await repo.GetTeamsOfLeagueAsync(league.Id))
            {
                // O EF junta a inscrição a season.Teams sozinho (a época já está a ser seguida).
                await repo.AddSeasonTeamAsync(new SeasonTeam { IdSeason = season.Id, IdTeam = team.Id });
            }

            return season;
        }

        #endregion

        #region Auxiliares

        private async Task RequireSuperAdmin(string? userId)
        {
            if (!await repo.IsSuperAdminAsync(userId))
            {
                throw new ForbiddenException("Só um super administrador pode gerir as ligas.");
            }
        }

        private static TimeSpan ParseKickoff(string? value)
        {
            var text = string.IsNullOrWhiteSpace(value) ? ModelConstants.LeagueConst.DefaultKickoff : value;
            if (!TimeSpan.TryParseExact(text, @"hh\:mm", CultureInfo.InvariantCulture, out var kickoff))
            {
                throw new ValidationException("A hora de início deve estar no formato HH:mm.");
            }

            return kickoff;
        }

        private async Task Notify(Guid teamId, string title, string body)
        {
            try
            {
                await notifications.SendTeamAsync(teamId.ToString(), title, body);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao notificar a equipa {Team}.", teamId);
            }
        }

        /// <summary>Época mostrada por omissão: a que está a decorrer, senão a das inscrições, senão a última.</summary>
        public static Season? CurrentSeason(League league) =>
            league.Seasons.FirstOrDefault(s => s.Status == SeasonStatus.IN_PROGRESS)
            ?? league.Seasons.Where(s => s.Status == SeasonStatus.REGISTRATION).OrderByDescending(s => s.StartDate).FirstOrDefault()
            ?? league.Seasons.OrderByDescending(s => s.StartDate).FirstOrDefault();

        private static FinishedMatch? ToFinished(Matches m)
        {
            if (m.MatchStatus != MatchStatus.DONE || m.Teams.Count != 2)
            {
                return null;
            }

            var home = m.Teams.FirstOrDefault(t => t.IdTeam == m.IdHomeTeam) ?? m.Teams.First();
            var away = m.Teams.First(t => t != home);
            return new FinishedMatch(home.IdTeam, away.IdTeam, home.NumGoals, away.NumGoals, m.MatchDate);
        }

        private static FixtureMatchDto? ToFixtureDto(Matches m)
        {
            if (m.Teams.Count != 2)
            {
                return null;
            }

            var home = m.Teams.FirstOrDefault(t => t.IdTeam == m.IdHomeTeam) ?? m.Teams.First();
            var away = m.Teams.First(t => t != home);
            var done = m.MatchStatus == MatchStatus.DONE;
            return new FixtureMatchDto
            {
                IdMatch = m.Id,
                Date = m.MatchDate,
                Status = m.MatchStatus,
                HomeTeamId = home.IdTeam,
                HomeTeamName = home.Team?.Name ?? "",
                AwayTeamId = away.IdTeam,
                AwayTeamName = away.Team?.Name ?? "",
                HomeGoals = done ? home.NumGoals : null,
                AwayGoals = done ? away.NumGoals : null,
            };
        }

        private static LeagueDto ToDto(League l, int teamCount)
        {
            var current = CurrentSeason(l);
            return new LeagueDto
            {
                Id = l.Id,
                Name = l.Name,
                Level = l.Level,
                PromotionSpots = l.PromotionSpots,
                RelegationSpots = l.RelegationSpots,
                SeasonDurationDays = l.SeasonDurationDays,
                TrophyName = l.TrophyName,
                TeamCount = teamCount,
                CurrentSeason = current == null ? null : ToDto(current),
            };
        }

        private static SeasonDto ToDto(Season s) => new()
        {
            Id = s.Id,
            LeagueId = s.IdLeague,
            Name = s.Name,
            Status = s.Status,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            TeamCount = s.Teams.Count,
        };

        #endregion
    }
}
