using Application.Interfaces.Repositories;
using Application.Interfaces.Validators.Hub;
using Application.Services.Hub;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using System.Collections.Concurrent;

namespace Tests.Unit.ApplicationTests.ServicesTests.HubsTests
{
    [TestFixture]
    public class StartMatchTests
    {
        #region Variables
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

        #endregion

        #region Setup
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
        #endregion

        #region Methods Suporrt
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

            var team1 = new Team
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

            var team2 = new Team
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

        private static string GetHubCacheKey(Guid matchId)
        {
            return ModelConstants.StartMatchHubConst.PrefixHubCache + matchId;
        }
        #endregion

        #region JoinHub Tests
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
            Assert.That(memoryCache.TryGetValue(GetHubCacheKey(matchId), out ConcurrentDictionary<Guid, string>? hub), Is.True);
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

        [Test(Description = "Lança exceção se o ID da partida estiver vazio")]
        public void JoinHubAsync_ShouldThrow_WhenMatchIdIsEmpty()
        {
            validatorMock.Setup(v => v.ValidateVariableJoinMatch(Guid.Empty, userId1, connectionId1))
                .Throws(new ArgumentException("O id da partida está null"));

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.JoinHubAsync(Guid.Empty, userId1, teamId1, connectionId1)
            );
        }

        [Test(Description = "Lança exceção se o ID do utilizador estiver vazio")]
        public void JoinHubAsync_ShouldThrow_WhenUserIdIsEmpty()
        {
            validatorMock.Setup(v => v.ValidateVariableJoinMatch(matchId, Guid.Empty, connectionId1))
                .Throws(new ArgumentException("O id do utilizador está a null"));

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.JoinHubAsync(matchId, Guid.Empty, teamId1, connectionId1)
            );
        }

        [Test(Description = "Lança exceção se o ID da conexão estiver nulo ou vazio")]
        public void JoinHubAsync_ShouldThrow_WhenConnectionIdIsNullOrEmpty()
        {
            validatorMock.Setup(v => v.ValidateVariableJoinMatch(matchId, userId1, ""))
                .Throws(new ArgumentException("A connection string está a null ou vazia"));

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.JoinHubAsync(matchId, userId1, teamId1, "")
            );
        }

        [Test(Description = "Lança exceção se a partida não for encontrada (repositório devolve nulo)")]
        public void JoinHubAsync_ShouldThrow_WhenMatchNotFound()
        {
            matchRepositoryMock.Setup(r => r.GetMatchWithListPlayerById(matchId))
                .ReturnsAsync((Matches?)null);

            validatorMock.Setup(v => v.ValidateMatchJoinMatch(null))
                .Throws(new ArgumentException("A partida não foi encontrada"));

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.JoinHubAsync(matchId, userId1, teamId1, connectionId1)
            );
        }

        [Test(Description = "Lança exceção se o utilizador que tenta entrar não for admin da equipa")]
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

        [Test(Description = "Lança exceção se uma equipa (admin) tentar entrar no hub duas vezes")]
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

        [Test(Description = "Lança exceção se o hub já estiver cheio (2 admins) e um terceiro tentar entrar")]
        public async Task JoinHubAsync_ShouldThrow_WhenHubAlreadyHasTwoAdmins()
        {
            var match = CreateMatch();
            matchRepositoryMock.Setup(r => r.GetMatchWithListPlayerById(matchId)).ReturnsAsync(match);

            var hub = new ConcurrentDictionary<Guid, string>();
            hub.TryAdd(Guid.NewGuid(), "conn1");
            hub.TryAdd(Guid.NewGuid(), "conn2");
            memoryCache.Set(GetHubCacheKey(matchId), hub);

            validatorMock.Setup(v => v.ValidateJoinMatch(It.IsAny<TeamStatistics>(), teamId1, hub))
                .Throws(new InvalidOperationException("Apenas dois admins podem aceder"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.JoinHubAsync(matchId, userId1, teamId1, connectionId1)
            );
        }

        [Test(Description = "Lança exceção se houver uma discrepância no ID da equipa durante a validação")]
        public void JoinHubAsync_ShouldThrow_WhenIdTeamMismatch()
        {
            var match = CreateMatch();
            matchRepositoryMock.Setup(r => r.GetMatchWithListPlayerById(matchId)).ReturnsAsync(match);

            validatorMock.Setup(v => v.ValidateJoinMatch(
                 It.IsAny<TeamStatistics>(), 
                 teamId1,
                 It.IsAny<ConcurrentDictionary<Guid, string>>()))
            .Throws(new InvalidOperationException("O id da team é diferente"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.JoinHubAsync(matchId, userId1, teamId1, connectionId1)
            );
        }

        [Test(Description = "Lança exceção se a equipa do admin tiver menos de 11 membros")]
        public void JoinHubAsync_ShouldThrow_WhenTeamHasLessThanElevenMembers()
        {
            var match = CreateMatch();
            var teamToModify = match.Teams.First(t => t.IdTeam == teamId1).Team;
            teamToModify.Members.Clear();
            for (int i = 0; i < 10; i++)
            {
                teamToModify.Members.Add(new Player
                {
                    Id = i == 0 ? userId1 : Guid.NewGuid(), 
                    IsAdmin = i == 0
                });
            }

            matchRepositoryMock.Setup(r => r.GetMatchWithListPlayerById(matchId)).ReturnsAsync(match);

            validatorMock.Setup(v => v.ValidateJoinMatch(
                    It.Is<TeamStatistics>(ts => ts.Team.Members.Count < 11), 
                    teamId1,
                    It.IsAny<ConcurrentDictionary<Guid, string>>()))
                .Throws(new InvalidOperationException("Só pode dar inicio a partida se a equipa tiver 11 jogadores"));

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.JoinHubAsync(matchId, userId1, teamId1, connectionId1)
            );

            Assert.That(ex.Message, Is.EqualTo("Só pode dar inicio a partida se a equipa tiver 11 jogadores"));

            validatorMock.Verify(v => v.ValidateJoinMatch(
                It.Is<TeamStatistics>(ts => ts.Team.Members.Count == 10),
                teamId1,
                It.IsAny<ConcurrentDictionary<Guid, string>>()), Times.Once);
        }

        #endregion

        #region Leave Tests
        [Test(Description = "Lança exceção se o matchId for inválido")]
        public void LeaveHubAsync_ShouldThrow_WhenMatchIdIsEmpty()
        {
            geralValidatorMock.Setup(v => v.ValidateIdMatchLeaveMatch(Guid.Empty, teamId1))
                .Throws(new ArgumentException("Match ID cannot be empty"));

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.LeaveHubAsync(Guid.Empty, teamId1, connectionId1)
            );
        }

        [Test(Description = "Lança exceção se o teamId for inválido")]
        public void LeaveHubAsync_ShouldThrow_WhenTeamIdIsEmpty()
        {
            geralValidatorMock.Setup(v => v.ValidateIdMatchLeaveMatch(matchId, Guid.Empty))
                .Throws(new ArgumentException("Team ID cannot be empty"));

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.LeaveHubAsync(matchId, Guid.Empty, connectionId1)
            );
        }

        [Test(Description = "Retorna 'false' se o hub não existir no cache")]
        public async Task LeaveHubAsync_WhenHubNotInCache_ShouldReturnFalse()
        {
            var result = await service.LeaveHubAsync(matchId, teamId1, connectionId1);

            Assert.That(result, Is.False);
        }

        [Test(Description = "Retorna 'false' se o hub existir mas a equipa não estiver lá")]
        public async Task LeaveHubAsync_WhenTeamNotInHub_ShouldReturnFalse()
        {
            var hubCacheKey = GetHubCacheKey(matchId);
            var hub = new ConcurrentDictionary<Guid, string>();
            hub.TryAdd(teamId2, connectionId2); 
            memoryCache.Set(hubCacheKey, hub);

            var result = await service.LeaveHubAsync(matchId, teamId1, connectionId1);

            Assert.That(result, Is.False);

            Assert.That(memoryCache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, string>? cachedHub), Is.True);
            Assert.That(cachedHub.Count, Is.EqualTo(1));
            Assert.That(cachedHub.ContainsKey(teamId2), Is.True);
        }

        [Test(Description = "Remove a equipa e atualiza o cache (quando o hub não fica vazio)")]
        public async Task LeaveHubAsync_WhenTeamIsNotLastInHub_ShouldRemoveTeamAndUpdateCache_AndReturnTrue()
        {
            var hubCacheKey = GetHubCacheKey(matchId);
            var hub = new ConcurrentDictionary<Guid, string>();
            hub.TryAdd(teamId1, connectionId1);
            hub.TryAdd(teamId2, connectionId2);
            memoryCache.Set(hubCacheKey, hub);

            var result = await service.LeaveHubAsync(matchId, teamId1, connectionId1);

            Assert.That(result, Is.True);

            Assert.That(memoryCache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, string>? updatedHub), Is.True);
            Assert.That(updatedHub, Is.Not.Null);
            Assert.That(updatedHub.Count, Is.EqualTo(1)); 
            Assert.That(updatedHub.ContainsKey(teamId1), Is.False); 
            Assert.That(updatedHub.ContainsKey(teamId2), Is.True); 
        }

        [Test(Description = "Remove a equipa e remove o hub do cache (quando o hub fica vazio)")]
        public async Task LeaveHubAsync_WhenTeamIsLastInHub_ShouldRemoveTeamAndHubFromCache_AndReturnTrue()
        {
            var hubCacheKey = GetHubCacheKey(matchId);
            var hub = new ConcurrentDictionary<Guid, string>();
            hub.TryAdd(teamId1, connectionId1);
            memoryCache.Set(hubCacheKey, hub);

            var result = await service.LeaveHubAsync(matchId, teamId1, connectionId1);

            Assert.That(result, Is.True);

            Assert.That(memoryCache.TryGetValue(hubCacheKey, out _), Is.False);
        }

        #endregion

        #region HandleDisconnect Tests

        [Test(Description = "Retorna 'false' se o matchId for nulo")]
        public async Task HandleDisconnectAsync_WhenMatchIdIsNull_ShouldReturnFalse()
        {
            var result = await service.HandleDisconnectAsync(null, teamId1, connectionId1);

            Assert.That(result, Is.False);
        }

        [Test(Description = "Retorna 'false' se o teamId for nulo")]
        public async Task HandleDisconnectAsync_WhenTeamIdIsNull_ShouldReturnFalse()
        {
            var result = await service.HandleDisconnectAsync(matchId, null, connectionId1);

            Assert.That(result, Is.False);
        }

        [Test(Description = "Retorna 'false' se ambos os IDs forem nulos")]
        public async Task HandleDisconnectAsync_WhenBothIdsAreNull_ShouldReturnFalse()
        {
            var result = await service.HandleDisconnectAsync(null, null, connectionId1);

            Assert.That(result, Is.False);
        }

        [Test(Description = "Passa a chamada para LeaveHubAsync e retorna 'true' em caso de sucesso")]
        public async Task HandleDisconnectAsync_WithValidIds_WhenLeaveIsSuccessful_ShouldReturnTrue()
        {
            var hubCacheKey = GetHubCacheKey(matchId);
            var hub = new ConcurrentDictionary<Guid, string>();
            hub.TryAdd(teamId1, connectionId1); 
            memoryCache.Set(hubCacheKey, hub);

            var result = await service.HandleDisconnectAsync(matchId, teamId1, connectionId1);

            Assert.That(result, Is.True);
            Assert.That(memoryCache.TryGetValue(hubCacheKey, out _), Is.False);
        }

        [Test(Description = "Passa a chamada para LeaveHubAsync e retorna 'false' se falhar (ex: hub não encontrado)")]
        public async Task HandleDisconnectAsync_WithValidIds_WhenLeaveFails_ShouldReturnFalse()
        {
            var result = await service.HandleDisconnectAsync(matchId, teamId1, connectionId1);

            Assert.That(result, Is.False);
        }
        #endregion
    }
}