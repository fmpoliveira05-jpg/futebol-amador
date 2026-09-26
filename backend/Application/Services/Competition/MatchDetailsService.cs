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

namespace Application.Services.Competition
{
    /// <summary>
    /// Onze inicial (com preenchimento automático), eventos do jogo e relatório
    /// (ver docs/novas-funcionalidades.md, D7 e D8).
    /// </summary>
    public class MatchDetailsService : IMatchDetailsService
    {
        private readonly IMatchDetailsRepository repo;
        private readonly ITeamRepository teams;
        private readonly IPlayerAuthorizationService authorization;
        private readonly IUnityOfWork unitOfWork;
        private readonly INotificationService notifications;
        private readonly TimeProvider clock;
        private readonly ILogger<MatchDetailsService> logger;

        public MatchDetailsService(IMatchDetailsRepository repo, ITeamRepository teams, IPlayerAuthorizationService authorization,
            IUnityOfWork unitOfWork, INotificationService notifications, TimeProvider clock, ILogger<MatchDetailsService> logger)
        {
            this.repo = repo;
            this.teams = teams;
            this.authorization = authorization;
            this.unitOfWork = unitOfWork;
            this.notifications = notifications;
            this.clock = clock;
            this.logger = logger;
        }

        private DateTime Now => clock.GetUtcNow().UtcDateTime;

        /// <summary>Até quando o onze pode ser alterado.</summary>
        public static DateTime Deadline(Matches match) => match.MatchDate.AddHours(-ModelConstants.LineupConst.DeadlineHours);

        public List<FormationDto> GetFormations() => Formations.All.Select(f => new FormationDto
        {
            Code = f.Code,
            Slots = f.Slots.Select(s => new FormationSlotDto
            {
                Slot = s.Slot,
                PositionCode = s.PositionCode,
                Role = s.Role,
                X = s.X,
                Y = s.Y,
            }).ToList(),
        }).ToList();

        #region Onze inicial

        public async Task<LineupDto> GetLineupAsync(string? userId, Guid matchId, Guid teamId)
        {
            var match = await repo.GetMatchWithTeamsAsync(matchId) ?? throw new NotFoundException("O jogo não existe.");
            var team = match.Teams.FirstOrDefault(t => t.IdTeam == teamId)?.Team
                       ?? throw new NotFoundException("A equipa não joga este jogo.");

            var isMember = userId != null && team.Members.Any(m => m.Id == userId);
            if (!isMember && !IsRevealed(match))
            {
                throw new ForbiddenException("O onze do adversário só é revelado depois do prazo.");
            }

            var lineup = await repo.GetLineupAsync(matchId, teamId);
            return ToDto(match, teamId, lineup);
        }

        public async Task<LineupDto> SaveLineupAsync(string? userId, Guid matchId, Guid teamId, SaveLineupDto dto)
        {
            await authorization.UserAuthorizationIsAdminTeamById(userId!, teamId);

            var match = await repo.GetMatchWithTeamsAsync(matchId) ?? throw new NotFoundException("O jogo não existe.");
            var team = match.Teams.FirstOrDefault(t => t.IdTeam == teamId)?.Team
                       ?? throw new NotFoundException("A equipa não joga este jogo.");

            if (match.MatchStatus != MatchStatus.SCHEDULED)
            {
                throw new BusinessRuleException("Só se define o onze de jogos marcados.");
            }

            if (Now >= Deadline(match))
            {
                throw new BusinessRuleException(
                    $"O prazo para definir o onze terminou ({ModelConstants.LineupConst.DeadlineHours} horas antes do jogo).");
            }

            var formation = Formations.Find(dto.Formation) ?? throw new ValidationException("Tática desconhecida.");
            var members = team.Members.ToDictionary(m => m.Id);
            ValidateLineup(dto, formation, members);

            var lineup = await repo.GetLineupAsync(matchId, teamId);
            if (lineup == null)
            {
                lineup = new MatchLineup { IdMatch = matchId, IdTeam = teamId, Formation = formation.Code };
                await repo.AddLineupAsync(lineup);
            }
            else
            {
                repo.RemoveSlots(lineup.Slots.ToList());
                lineup.Slots.Clear();
            }

            lineup.Formation = formation.Code;
            lineup.IsAutoFilled = false;
            lineup.UpdatedAt = Now;
            AddSlots(lineup, formation,
                dto.Starters.ToDictionary(s => s.Slot, s => s.PlayerId),
                dto.Bench);

            await unitOfWork.SaveChangesAsync();

            var saved = await repo.GetLineupAsync(matchId, teamId);
            return ToDto(match, teamId, saved);
        }

        public async Task<int> AutoFillMissingLineupsAsync(CancellationToken ct = default)
        {
            var now = Now;
            var matches = await repo.GetScheduledMatchesBetweenAsync(now, now.AddHours(ModelConstants.LineupConst.DeadlineHours));
            var filled = 0;

            foreach (var match in matches)
            {
                foreach (var stats in match.Teams)
                {
                    ct.ThrowIfCancellationRequested();
                    if (await repo.GetLineupAsync(match.Id, stats.IdTeam) != null)
                    {
                        continue;
                    }

                    var team = await teams.GetTeamForMemberManagementAsync(stats.IdTeam);
                    if (team == null || team.Members.Count == 0)
                    {
                        continue;
                    }

                    var previous = await repo.GetLastLineupAsync(team.Id, match.MatchDate);
                    var auto = LineupAutoFiller.Fill(
                        team.Members.Select(m => new SquadPlayer(m.Id, m.Name, m.Position, m.Status)).ToList(),
                        previous?.Formation,
                        previous?.Slots.Where(s => s.IsStarter).ToDictionary(s => s.Slot, s => s.PlayerId),
                        previous?.Slots.Where(s => !s.IsStarter).OrderBy(s => s.Slot).Select(s => s.PlayerId).ToList());

                    var lineup = new MatchLineup
                    {
                        IdMatch = match.Id,
                        IdTeam = team.Id,
                        Formation = auto.Formation,
                        IsAutoFilled = true,
                        UpdatedAt = now,
                    };
                    AddSlots(lineup, Formations.Find(auto.Formation)!, auto.Starters, auto.Bench);
                    await repo.AddLineupAsync(lineup);
                    await unitOfWork.SaveChangesAsync();
                    filled++;

                    try
                    {
                        await notifications.SendTeamAsync(team.Id.ToString(), "Onze preenchido automaticamente",
                            $"O prazo para o onze do jogo de {match.MatchDate:dd/MM HH:mm} terminou; foi preenchido automaticamente ({auto.Formation}).");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Falha ao notificar a equipa {Team}.", team.Id);
                    }
                }
            }

            return filled;
        }

        private static void ValidateLineup(SaveLineupDto dto, Formation formation, IReadOnlyDictionary<string, Player> members)
        {
            var expected = Math.Min(ModelConstants.LineupConst.Starters, members.Count);
            if (dto.Starters.Count != expected)
            {
                throw new ValidationException($"O onze tem de ter {expected} titulares.");
            }

            if (dto.Starters.Select(s => s.Slot).Distinct().Count() != dto.Starters.Count
                || dto.Starters.Any(s => s.Slot < 0 || s.Slot >= formation.Slots.Count))
            {
                throw new ValidationException("Há posições repetidas ou fora da tática.");
            }

            var all = dto.Starters.Select(s => s.PlayerId).Concat(dto.Bench).ToList();
            if (all.Distinct().Count() != all.Count)
            {
                throw new ValidationException("Um jogador não pode aparecer duas vezes no onze e no banco.");
            }

            if (dto.Bench.Count > ModelConstants.LineupConst.MaxBench)
            {
                throw new ValidationException($"O banco tem no máximo {ModelConstants.LineupConst.MaxBench} suplentes.");
            }

            if (all.Any(id => !members.ContainsKey(id)))
            {
                throw new ValidationException("Só podem ser escolhidos jogadores da equipa.");
            }
        }

        private static void AddSlots(MatchLineup lineup, Formation formation, IReadOnlyDictionary<int, string> starters, IEnumerable<string> bench)
        {
            foreach (var (slot, playerId) in starters)
            {
                lineup.Slots.Add(new LineupSlot
                {
                    IdLineup = lineup.Id,
                    PlayerId = playerId,
                    IsStarter = true,
                    Slot = slot,
                    PositionCode = formation.Slots[slot].PositionCode,
                });
            }

            var order = 0;
            foreach (var playerId in bench)
            {
                lineup.Slots.Add(new LineupSlot { IdLineup = lineup.Id, PlayerId = playerId, IsStarter = false, Slot = order++ });
            }
        }

        private bool IsRevealed(Matches match) => match.MatchStatus != MatchStatus.SCHEDULED || Now >= Deadline(match);

        private LineupDto ToDto(Matches match, Guid teamId, MatchLineup? lineup)
        {
            var dto = new LineupDto
            {
                MatchId = match.Id,
                TeamId = teamId,
                Deadline = Deadline(match),
                IsLocked = match.MatchStatus != MatchStatus.SCHEDULED || Now >= Deadline(match),
                Exists = lineup != null,
                Formation = lineup?.Formation,
                IsAutoFilled = lineup?.IsAutoFilled ?? false,
            };

            if (lineup == null)
            {
                return dto;
            }

            dto.Starters = lineup.Slots.Where(s => s.IsStarter).OrderBy(s => s.Slot).Select(ToPlayer).ToList();
            dto.Bench = lineup.Slots.Where(s => !s.IsStarter).OrderBy(s => s.Slot).Select(ToPlayer).ToList();
            return dto;
        }

        private static LineupPlayerDto ToPlayer(LineupSlot s) => new()
        {
            Slot = s.IsStarter ? s.Slot : null,
            PositionCode = s.PositionCode,
            PlayerId = s.PlayerId,
            PlayerName = s.Player?.Name ?? "",
            Position = s.Player?.Position ?? Position.MIDFIELDER,
        };

        #endregion

        #region Eventos e relatório

        public async Task ValidateEventsAsync(Guid matchId, Guid teamId, int teamGoals, MatchEventsDto? events)
        {
            if (events == null)
            {
                return;
            }

            var match = await repo.GetMatchWithTeamsAsync(matchId) ?? throw new NotFoundException("O jogo não existe.");
            var team = match.Teams.FirstOrDefault(t => t.IdTeam == teamId)?.Team
                       ?? throw new NotFoundException("A equipa não joga este jogo.");
            var lineup = await repo.GetLineupAsync(matchId, teamId);

            // Jogadores válidos: os do onze e do banco; sem onze, os membros atuais da equipa.
            var valid = lineup != null
                ? lineup.Slots.Select(s => s.PlayerId).ToHashSet()
                : team.Members.Select(m => m.Id).ToHashSet();

            ValidateEvents(events, teamGoals, valid, lineup?.Slots.Where(s => s.IsStarter).Select(s => s.PlayerId).ToHashSet());
        }

        /// <summary>Regras dos eventos de uma equipa (pública para os testes).</summary>
        public static void ValidateEvents(MatchEventsDto events, int teamGoals, IReadOnlySet<string> validPlayers, IReadOnlySet<string>? starters)
        {
            if (events.Fouls is < 0 or > 200)
            {
                throw new ValidationException("O número de faltas deve estar entre 0 e 200.");
            }

            if (events.Goals.Count > teamGoals)
            {
                throw new ValidationException($"Registaste {events.Goals.Count} golos, mas a equipa marcou {teamGoals}.");
            }

            bool Unknown(string? id) => id != null && !validPlayers.Contains(id);

            foreach (var g in events.Goals)
            {
                if (Unknown(g.ScorerId) || Unknown(g.AssistId))
                {
                    throw new ValidationException("Os marcadores e as assistências têm de ser jogadores da equipa neste jogo.");
                }

                if (g.ScorerId != null && g.ScorerId == g.AssistId)
                {
                    throw new ValidationException("Um jogador não pode assistir o próprio golo.");
                }
            }

            foreach (var c in events.Cards)
            {
                if (Unknown(c.PlayerId))
                {
                    throw new ValidationException("Os cartões têm de ser de jogadores da equipa neste jogo.");
                }
            }

            if (events.Cards.GroupBy(c => c.PlayerId).Any(g => g.Count(c => c.Type == CardType.RED) > 1 || g.Count(c => c.Type == CardType.YELLOW) > 2))
            {
                throw new ValidationException("Um jogador tem no máximo dois amarelos e um vermelho.");
            }

            var cameIn = new HashSet<string>();
            foreach (var s in events.Substitutions.OrderBy(s => s.Minute ?? 0))
            {
                if (Unknown(s.PlayerOutId) || Unknown(s.PlayerInId) || s.PlayerInId == s.PlayerOutId)
                {
                    throw new ValidationException("Cada substituição tem de ter dois jogadores diferentes da equipa neste jogo.");
                }

                if (starters != null && (starters.Contains(s.PlayerInId) || !cameIn.Add(s.PlayerInId)))
                {
                    throw new ValidationException("Quem entra numa substituição tem de vir do banco (e só entra uma vez).");
                }
            }
        }

        public async Task ApplyEventsAsync(Guid matchId, IReadOnlyDictionary<Guid, MatchEventsDto?> eventsByTeam)
        {
            var match = await repo.GetMatchWithTeamsAsync(matchId) ?? throw new NotFoundException("O jogo não existe.");
            var existing = await repo.GetEventsOfMatchAsync(matchId);

            foreach (var (teamId, events) in eventsByTeam)
            {
                if (events == null)
                {
                    continue;
                }

                repo.RemoveEvents(existing.Where(e => e.IdTeam == teamId));

                var stats = match.Teams.FirstOrDefault(t => t.IdTeam == teamId);
                if (stats != null)
                {
                    stats.Fouls = events.Fouls;
                }

                var list = new List<MatchEvent>();
                list.AddRange(events.Goals.Select(g => new MatchEvent
                {
                    IdMatch = matchId, IdTeam = teamId, Type = MatchEventType.GOAL, Minute = g.Minute,
                    PlayerId = g.ScorerId, RelatedPlayerId = g.AssistId,
                }));
                list.AddRange(events.Cards.Select(c => new MatchEvent
                {
                    IdMatch = matchId, IdTeam = teamId, Minute = c.Minute, PlayerId = c.PlayerId,
                    Type = c.Type == CardType.RED ? MatchEventType.RED_CARD : MatchEventType.YELLOW_CARD,
                }));
                list.AddRange(events.Substitutions.Select(s => new MatchEvent
                {
                    IdMatch = matchId, IdTeam = teamId, Type = MatchEventType.SUBSTITUTION, Minute = s.Minute,
                    PlayerId = s.PlayerOutId, RelatedPlayerId = s.PlayerInId,
                }));

                await repo.AddEventsAsync(list);
            }
        }

        public async Task<MatchReportDto> GetReportAsync(Guid matchId)
        {
            var match = await repo.GetMatchWithTeamsAsync(matchId) ?? throw new NotFoundException("O jogo não existe.");
            if (match.Teams.Count != 2)
            {
                throw new BusinessRuleException("O jogo não tem duas equipas.");
            }

            var home = match.Teams.FirstOrDefault(t => t.IdTeam == match.IdHomeTeam)
                       ?? match.Teams.FirstOrDefault(t => t.Team?.IdPitch == match.idPitch)
                       ?? match.Teams.First();
            var away = match.Teams.First(t => t != home);
            var events = await repo.GetEventsOfMatchAsync(matchId);
            var lineups = IsRevealed(match) ? await repo.GetLineupsOfMatchAsync(matchId) : new List<MatchLineup>();
            var names = await repo.GetPlayerNamesAsync(events.SelectMany(e => new[] { e.PlayerId, e.RelatedPlayerId }).Where(i => i != null)!);

            TeamReportDto Report(TeamStatistics ts)
            {
                var mine = events.Where(e => e.IdTeam == ts.IdTeam).ToList();
                var lineup = lineups.FirstOrDefault(l => l.IdTeam == ts.IdTeam);
                return new TeamReportDto
                {
                    TeamId = ts.IdTeam,
                    TeamName = ts.Team?.Name ?? "",
                    Goals = match.MatchStatus == MatchStatus.DONE ? ts.NumGoals : null,
                    Fouls = ts.Fouls,
                    YellowCards = mine.Count(e => e.Type == MatchEventType.YELLOW_CARD),
                    RedCards = mine.Count(e => e.Type == MatchEventType.RED_CARD),
                    Substitutions = mine.Count(e => e.Type == MatchEventType.SUBSTITUTION),
                    Lineup = lineup == null ? null : ToDto(match, ts.IdTeam, lineup),
                    Events = mine
                        .OrderBy(e => e.Minute ?? int.MaxValue)
                        .Select(e => new MatchEventViewDto
                        {
                            Type = e.Type.ToString(),
                            Minute = e.Minute,
                            PlayerId = e.PlayerId,
                            PlayerName = e.PlayerId != null ? names.GetValueOrDefault(e.PlayerId) : null,
                            RelatedPlayerId = e.RelatedPlayerId,
                            RelatedPlayerName = e.RelatedPlayerId != null ? names.GetValueOrDefault(e.RelatedPlayerId) : null,
                        })
                        .ToList(),
                };
            }

            return new MatchReportDto
            {
                MatchId = match.Id,
                Date = match.MatchDate,
                Status = match.MatchStatus,
                IsCompetitive = match.IsCompetive,
                LeagueName = match.Season?.League?.Name,
                Round = match.Round,
                PitchName = match.Pitch?.Name,
                Home = Report(home),
                Away = Report(away),
            };
        }

        #endregion
    }
}
