using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Tests.Unit.CompetitionTests
{
    /// <summary>
    /// Base de dados em memória com repositórios reais, para testar os serviços das ligas de ponta a ponta.
    /// A autorização é simulada: um jogador é administrador das equipas em que tem IsAdmin.
    /// </summary>
    public sealed class CompetitionTestDb : IDisposable
    {
        public AmateurFootballContext Db { get; }
        public TestClock Clock { get; } = new(new DateTimeOffset(2026, 9, 1, 10, 0, 0, TimeSpan.Zero));
        public Mock<INotificationService> Notifications { get; } = new();
        public Mock<IPlayerAuthorizationService> Authorization { get; } = new();

        public CompetitionTestDb()
        {
            var options = new DbContextOptionsBuilder<AmateurFootballContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            Db = new AmateurFootballContext(options);

            Authorization
                .Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns<string, Guid>(async (userId, teamId) =>
                {
                    var ok = await Db.Player.AnyAsync(p => p.Id == userId && p.IdTeam == teamId && p.IsAdmin);
                    if (!ok)
                    {
                        throw new Domain.Exceptions.ForbiddenException("Apenas administradores de equipa têm acesso a este recurso.");
                    }
                });
        }

        public CompetitionRepository Competition => new(Db);
        public TransferRepository Transfers => new(Db);
        public MatchDetailsRepository Details => new(Db);
        public TeamRepository Teams => new(Db);
        public PlayerRepository Players => new(Db);
        public UnityOfWork UnitOfWork => new(Db);

        public Player AddPlayer(string id, Position position = Position.MIDFIELDER, Team? team = null, bool admin = false,
            DateTime? adminSince = null)
        {
            var player = new Player(id, $"Jogador {id}", new DateOnly(1998, 5, 1), "Rua do Teste, 1", $"{id}@teste.pt",
                "+351912345678", position, 180, null)
            {
                CreationDate = new DateTime(2026, 1, 1),
                IsAdmin = admin,
                IsAdminLastChangedAt = admin ? adminSince ?? new DateTime(2026, 1, 1) : null,
                IdTeam = team?.Id,
                Team = team,
            };
            // Ao seguir o jogador, o EF junta-o a team.Members (não se acrescenta à mão, senão fica duas vezes).
            Db.Player.Add(player);
            return player;
        }

        public Team AddTeam(string name, League? league = null, Rank? rank = null)
        {
            rank ??= Db.Rank.Local.FirstOrDefault() ?? AddRank();
            var team = new Team(name, null, "", new Pitch($"Campo {name}", $"Rua {name}, 1"), rank)
            {
                IdLeague = league?.Id,
            };
            Db.Team.Add(team);
            return team;
        }

        public Rank AddRank(string name = "Divisão 4")
        {
            var rank = new Rank { Name = name, WinPoints = 3, DrawPoints = 1, LosePoints = 0, PointsToPromotion = 100 };
            Db.Rank.Add(rank);
            return rank;
        }

        public League AddLeague(string name, int level, int promotion = 1, int relegation = 1, int days = 120)
        {
            var league = new League
            {
                Name = name,
                Level = level,
                PromotionSpots = promotion,
                RelegationSpots = relegation,
                SeasonDurationDays = days,
                TrophyName = $"Taça {name}",
            };
            Db.League.Add(league);
            return league;
        }

        public void Dispose() => Db.Dispose();
    }

    /// <summary>Relógio controlado pelos testes.</summary>
    public sealed class TestClock : TimeProvider
    {
        private DateTimeOffset now;

        public TestClock(DateTimeOffset start)
        {
            now = start;
        }

        public override DateTimeOffset GetUtcNow() => now;

        public void Advance(TimeSpan by) => now = now.Add(by);

        public void Set(DateTime utc) => now = new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc));
    }
}
