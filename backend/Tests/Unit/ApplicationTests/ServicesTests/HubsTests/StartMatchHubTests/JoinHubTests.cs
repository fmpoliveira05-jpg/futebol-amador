using Application.Interfaces.Repositories;
using Application.Interfaces.Validators.Hub;
using Application.Services.Hub;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using System.Collections.Concurrent;

namespace Tests.Unit.ApplicationTests.ServicesTests.HubsTests.StartMatchHubTests
{
    [TestFixture]
    public class JoinHubTests
    {
        private Mock<IMatchRepository> matchRepositoryMock;
        private Mock<IUnityOfWork> unityOfWorkMock;
        private Mock<IStartMatchHubValidator> validatorMock;
        private Mock<IGeralHubValidator> geralValidatorMock;
        private IMemoryCache memoryCache;
        private ManagerStartMatchService service;

        private Guid matchId;
        private Guid teamId1;
        private Guid teamId2;
        private Guid userId1;
        private Guid userId2;
        private string connectionId1;
        private string connectionId2;

        [SetUp]
        public void Setup()
        {
            matchRepositoryMock = new Mock<IMatchRepository>();
            unityOfWorkMock = new Mock<IUnityOfWork>();
            validatorMock = new Mock<IStartMatchHubValidator>();
            geralValidatorMock = new Mock<IGeralHubValidator>();
            memoryCache = new MemoryCache(new MemoryCacheOptions());

            service = new ManagerStartMatchService(
                matchRepositoryMock.Object,
                unityOfWorkMock.Object,
                validatorMock.Object,
                geralValidatorMock.Object,
                memoryCache
            );

            matchId = Guid.NewGuid();
            teamId1 = Guid.NewGuid();
            teamId2 = Guid.NewGuid();
            userId1 = Guid.NewGuid();
            userId2 = Guid.NewGuid();
            connectionId1 = "conn-1";
            connectionId2 = "conn-2";
        }

        private Matches CreateMatch()
        {
            var now = DateTime.UtcNow;
            const string validAddress = "Rua Central,123,2º,4000-200,Porto,Santo Ildefonso,Porto";

            var diamondRank = new Rank
            {
                Id = Guid.Parse("61C8C987-D08B-458E-92B2-1011F426986A"),
                Name = "Diamond",
                WinPoints = 6,
                DrawPoints = -2,
                LosePoints = -4
            };

            var silverRank = new Rank
            {
                Id = Guid.Parse("54AE9AF9-0F7F-4031-AB56-F16F69EE984D"),
                Name = "Silver",
                WinPoints = 4,
                DrawPoints = 2,
                LosePoints = -2
            };

            var team1Players = new List<Player>();
            for (int i = 0; i < 11; i++)
            {
                team1Players.Add(new Player
                {
                    Id = i == 0 ? userId1 : Guid.NewGuid(),
                    IsAdmin = i == 0
                });
            }

            var team2Players = new List<Player>();
            for (int i = 0; i < 11; i++)
            {
                team2Players.Add(new Player
                {
                    Id = i == 0 ? userId2 : Guid.NewGuid(),
                    IsAdmin = i == 0
                });
            }

            var team1 = new Teams
            {
                Id = teamId1,
                Name = "Team One",
                Description = "Primeira equipa de teste",
                IdPitch = Guid.NewGuid(),
                Pitch = new Pitch
                {
                    Id = Guid.NewGuid(),
                    Name = "Campo A",
                    Address = validAddress
                },
                DataFoundation = now.AddYears(-2),
                CurrentPoints = 0,
                IdRank = diamondRank.Id,
                Rank = diamondRank,
                IdCalendar = Guid.NewGuid(),
                Calendar = new Calendar { Id = Guid.NewGuid() },
                Members = team1Players
            };

            var team2 = new Teams
            {
                Id = teamId2,
                Name = "Team Two",
                Description = "Segunda equipa de teste",
                IdPitch = Guid.NewGuid(),
                Pitch = new Pitch
                {
                    Id = Guid.NewGuid(),
                    Name = "Campo B",
                    Address = validAddress
                },
                DataFoundation = now.AddYears(-1),
                CurrentPoints = 0,
                IdRank = silverRank.Id,
                Rank = silverRank,
                IdCalendar = Guid.NewGuid(),
                Calendar = new Calendar { Id = Guid.NewGuid() },
                Members = team2Players
            };

            return new Matches
            {
                Id = matchId,
                MatchStatus = MatchStatus.SCHEDULED,
                Teams = new List<TeamStatistics>
                {
                    new TeamStatistics
                    {
                        IdTeam = teamId1,
                        Team = team1
                    },
                    new TeamStatistics
                    {
                        IdTeam = teamId2,
                        Team = team2
                    }
                }
            };
        }

        [Test(Description = "1º admin entra e cria o hub")]
        public async Task JoinHubAsync_FirstAdmin_ShouldCreateHubAndReturnIsFirstAdminTrue()
        {
            var match = CreateMatch();
            matchRepositoryMock.Setup(r => r.GetMatchWithListPlayerById(matchId)).ReturnsAsync(match);

            validatorMock.SetupAllProperties();

            var result = await service.JoinHubAsync(matchId, userId1, teamId1, connectionId1);

            Assert.That(result.IsFirstAdmin, Is.True);
            Assert.That(result.MatchStarted, Is.False);
            Assert.That(result.TeamId, Is.EqualTo(teamId1));
            Assert.That(memoryCache.TryGetValue($"hub-{matchId}", out ConcurrentDictionary<Guid, string>? hub), Is.True);
            Assert.That(hub.ContainsKey(teamId1), Is.True);
            Assert.That(hub[teamId1], Is.EqualTo(connectionId1));
        }

        [Test(Description = "2º admin entra e a partida é iniciada")]
        public async Task JoinHubAsync_SecondAdmin_ShouldStartMatchAndReturnIsFirstAdminFalse()
        {
            var match = CreateMatch();
            matchRepositoryMock.Setup(r => r.GetMatchWithListPlayerById(matchId)).ReturnsAsync(match);

            await service.JoinHubAsync(matchId, userId1, teamId1, connectionId1);
            var result = await service.JoinHubAsync(matchId, userId2, teamId2, connectionId2);

            Assert.That(result.IsFirstAdmin, Is.False);
            Assert.That(result.MatchStarted, Is.True);
            Assert.That(result.FirstAdminConnectionId, Is.EqualTo(connectionId1));
            Assert.That(match.MatchStatus, Is.EqualTo(MatchStatus.IN_PROGRESS));
            unityOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void JoinHubAsync_ShouldThrow_WhenMatchIdIsEmpty()
        {
            // ARRANGE: Diz ao mock para lançar a exceção quando for chamado com Guid.Empty
            validatorMock.Setup(v => v.ValidateVariableJoinMatch(Guid.Empty, userId1, connectionId1))
                .Throws(new ArgumentException("O id da partida está null"));

            // ACT & ASSERT
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.JoinHubAsync(Guid.Empty, userId1, teamId1, connectionId1)
            );
        }

        [Test]
        public void JoinHubAsync_ShouldThrow_WhenUserIdIsEmpty()
        {
            // ARRANGE: Diz ao mock para lançar a exceção quando for chamado com Guid.Empty
            validatorMock.Setup(v => v.ValidateVariableJoinMatch(matchId, Guid.Empty, connectionId1))
                .Throws(new ArgumentException("O id do utilizador está a null"));

            // ACT & ASSERT
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.JoinHubAsync(matchId, Guid.Empty, teamId1, connectionId1)
            );
        }

        [Test]
        public void JoinHubAsync_ShouldThrow_WhenConnectionIdIsNullOrEmpty()
        {
            // ARRANGE: Diz ao mock para lançar a exceção quando for chamado com string vazia
            validatorMock.Setup(v => v.ValidateVariableJoinMatch(matchId, userId1, ""))
                .Throws(new ArgumentException("A connection string está a null ou vazia"));

            // ACT & ASSERT
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.JoinHubAsync(matchId, userId1, teamId1, "")
            );
        }

        [Test]
        public void JoinHubAsync_ShouldThrow_WhenMatchNotFound()
        {
            // ARRANGE 1: Configura o repositório para devolver null
            matchRepositoryMock.Setup(r => r.GetMatchWithListPlayerById(matchId))
                .ReturnsAsync((Matches?)null);

            // ARRANGE 2: Diz ao mock do validador para lançar a exceção quando receber null
            validatorMock.Setup(v => v.ValidateMatchJoinMatch(null))
                .Throws(new ArgumentException("A partida não foi encontrada"));

            // ACT & ASSERT
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.JoinHubAsync(matchId, userId1, teamId1, connectionId1)
            );
        }

        [Test]
        public void JoinHubAsync_ShouldThrow_WhenUserIsNotAdmin()
        {
            var match = CreateMatch();
            foreach (var player in match.Teams.First().Team.Members)
                player.IsAdmin = false;

            matchRepositoryMock.Setup(r => r.GetMatchWithListPlayerById(matchId)).ReturnsAsync(match);
            validatorMock.Setup(v => v.ValidateJoinMatch(null, teamId1, It.IsAny<ConcurrentDictionary<Guid, string>>()))
                .Throws(new ArgumentException("A equipa do admin não foi encontrada"));

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.JoinHubAsync(matchId, userId1, teamId1, connectionId1)
            );
        }

        [Test]
        public async Task JoinHubAsync_ShouldThrow_WhenTeamAlreadyInHub()
        {
            var match = CreateMatch();
            matchRepositoryMock.Setup(r => r.GetMatchWithListPlayerById(matchId)).ReturnsAsync(match);

            await service.JoinHubAsync(matchId, userId1, teamId1, connectionId1);

            validatorMock.Setup(v => v.ValidateJoinMatch(It.IsAny<TeamStatistics>(), teamId1, It.IsAny<ConcurrentDictionary<Guid, string>>()))
                .Throws(new InvalidOperationException("Já existe um admin desta equipa a iniciar a partida"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.JoinHubAsync(matchId, userId1, teamId1, "conn-dup")
            );
        }

        [Test]
        public async Task JoinHubAsync_ShouldThrow_WhenHubAlreadyHasTwoAdmins()
        {
            var match = CreateMatch();
            matchRepositoryMock.Setup(r => r.GetMatchWithListPlayerById(matchId)).ReturnsAsync(match);

            var hub = new ConcurrentDictionary<Guid, string>();
            hub.TryAdd(Guid.NewGuid(), "conn1");
            hub.TryAdd(Guid.NewGuid(), "conn2");
            memoryCache.Set($"hub-{matchId}", hub);

            validatorMock.Setup(v => v.ValidateJoinMatch(It.IsAny<TeamStatistics>(), teamId1, hub))
                .Throws(new InvalidOperationException("Apenas dois admins podem aceder"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.JoinHubAsync(matchId, userId1, teamId1, connectionId1)
            );
        }

        [Test]
        public void JoinHubAsync_ShouldThrow_WhenIdTeamMismatch()
        {
            var match = CreateMatch();
            matchRepositoryMock.Setup(r => r.GetMatchWithListPlayerById(matchId)).ReturnsAsync(match);

            validatorMock.Setup(v => v.ValidateJoinMatch(
                 It.IsAny<TeamStatistics>(), // <-- AQUI ESTÁ A MUDANÇA
                 teamId1,
                 It.IsAny<ConcurrentDictionary<Guid, string>>()))
            .Throws(new InvalidOperationException("O id da team é diferente"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.JoinHubAsync(matchId, userId1, teamId1, connectionId1)
            );
        }
    }
}