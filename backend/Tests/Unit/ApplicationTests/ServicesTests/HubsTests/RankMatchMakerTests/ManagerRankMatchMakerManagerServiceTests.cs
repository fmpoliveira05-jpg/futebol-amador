using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators.Hub;
using Application.Services.Hub;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using System.Collections.Concurrent;

namespace Tests.Unit.ApplicationTests.ServicesTests.HubsTests.RankMatchMakerTests
{
    [TestFixture]
    public class ManagerRankMatchMakerManagerServiceTests
    {
        #region Variables

        private Mock<IMatchMakerService> serviceMatchMakerMock;
        private Mock<ITeamRepository> teamRepositoryMock;
        private Mock<IMatchRepository> matchRepositoryMock;
        private Mock<IUnityOfWork> unityOfWorkMock;
        private Mock<IRankMatchMakerValidator> validatorMock;
        private Mock<IMemoryCache> cacheMock;
        private ManagerRankMatchMakerService service;

        private const string GlobalHubKeysCacheKey = ModelConstants.ManagerRankMatchMakerServiceConst.GlobalHubKeysCacheKey;
        // CORRIGIDO: idPlayer agora é string
        private readonly string idPlayer = "player-id-123";
        private readonly Guid idTeam = Guid.NewGuid();
        private readonly string connectionId = "conn-123";
        private readonly TimeOnly hoursGame = new TimeOnly(10, 0, 0);
        private DateTime gameDate;
        #endregion

        #region SetUp
        [SetUp]
        public void Setup()
        {
            serviceMatchMakerMock = new Mock<IMatchMakerService>();
            teamRepositoryMock = new Mock<ITeamRepository>();
            matchRepositoryMock = new Mock<IMatchRepository>();
            unityOfWorkMock = new Mock<IUnityOfWork>();
            validatorMock = new Mock<IRankMatchMakerValidator>();
            cacheMock = new Mock<IMemoryCache>();

            service = new ManagerRankMatchMakerService(
                serviceMatchMakerMock.Object,
                teamRepositoryMock.Object,
                matchRepositoryMock.Object,
                unityOfWorkMock.Object,
                validatorMock.Object,
                cacheMock.Object
            );

            gameDate = new DateTime(DateOnly.FromDateTime(GetNextSunday(DateTime.UtcNow)), hoursGame);
        }
        #endregion

        #region Support Methods

        private Team CreateMockTeam(bool isMainPlayerAdmin, int memberCount = 11, int additionalAdminCount = 0)
        {
            var members = new List<Player>();

            if (memberCount < 1)
                throw new ArgumentException("O memberCount tem de ser pelo menos 1 (jogador principal).", nameof(memberCount));

            int totalAdmins = additionalAdminCount + (isMainPlayerAdmin ? 1 : 0);
            if (totalAdmins > 4)
                throw new ArgumentException("A equipa não pode ter mais de 4 administradores.", nameof(additionalAdminCount));

            if (memberCount < totalAdmins)
                throw new ArgumentException("O memberCount tem de ser igual ou superior ao número total de admins.", nameof(memberCount));

            if (memberCount < (additionalAdminCount + 1))
                throw new ArgumentException("O memberCount não é suficiente para o jogador principal e os admins adicionais.", nameof(memberCount));

            members.Add(new Player
            {
                Id = idPlayer, // Isto agora é uma string
                IsAdmin = isMainPlayerAdmin,
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25))
            });

            int remainingMembers = memberCount - 1;
            int remainingAdminsToCreate = additionalAdminCount;

            for (int i = 0; i < remainingMembers; i++)
            {
                bool makeThisOneAdmin = false;
                if (remainingAdminsToCreate > 0)
                {
                    makeThisOneAdmin = true;
                    remainingAdminsToCreate--;
                }

                members.Add(new Player
                {
                    // CORRIGIDO: Id de Player agora é string
                    Id = $"other-player-{Guid.NewGuid().ToString("N")}",
                    IsAdmin = makeThisOneAdmin,
                    DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-23))
                });
            }

            var rank = new Rank { Id = Guid.NewGuid(), Name = "Gold", PointsToPromotion = 1000, NextRank = new Rank { Name = "Platinum" }, PreviousRank = new Rank { Name = "Silver", PreviousRank = new Rank { PointsToPromotion = 200 } } };

            return new Team
            {
                Id = idTeam,
                Name = "Test Team",
                Members = members,
                CurrentPoints = 500,
                Rank = rank,
                IdRank = rank.Id,
                Pitch = new Pitch { Id = Guid.NewGuid(), Address = "Rua Exemplo,123,1,1000-001,Lisboa,Benfica,Lisboa" }
            };
        }

        private void SetupCacheTryGetValue<T>(object key, bool returns, T value)
        {
            object outValue = value;
            cacheMock.Setup(c => c.TryGetValue(key, out outValue))
                         .Returns(returns);
        }

        private static DateTime GetNextSunday(DateTime startDate)
        {
            int currentDayOfWeek = (int)startDate.DayOfWeek;
            int targetDayOfWeek = (int)DayOfWeek.Sunday;
            int daysToAdd = targetDayOfWeek - currentDayOfWeek;

            if (daysToAdd <= 0)
            {
                daysToAdd += 7;
            }

            return startDate.Date.AddDays(daysToAdd);
        }

        private static string GetHubCacheKey(Guid idTeam)
        {
            return ModelConstants.ManagerRankMatchMakerServiceConst.PrefixHubCache + idTeam;
        }

        private InfoTeamRankMatchMakerDto CreateMockTeamDto(Guid id, DateTime gameDate)
        {
            return new InfoTeamRankMatchMakerDto
            {
                IdTeam = id,
                GameDate = gameDate,
                AverageAge = 25,
                NumberPointsTeam = 500,
                City = "Lisbon"
            };
        }

        private EntryRankMatchMakerHub CreateMockEntry(Guid id, DateTime gameDate)
        {
            return new EntryRankMatchMakerHub
            {
                ConnectionId = "conn-" + id,
                Team = CreateMockTeamDto(id, gameDate)
            };
        }

        private Mock<ICacheEntry> CreateCacheEntryMock()
        {
            var cacheEntryMock = new Mock<ICacheEntry>();
            cacheEntryMock.SetupProperty(e => e.Value);
            cacheEntryMock.SetupProperty(e => e.AbsoluteExpiration);
            cacheEntryMock.SetupProperty(e => e.AbsoluteExpirationRelativeToNow);
            cacheEntryMock.SetupProperty(e => e.SlidingExpiration);
            cacheEntryMock.SetupProperty(e => e.Priority);
            cacheEntryMock.SetupGet(e => e.ExpirationTokens).Returns(new List<IChangeToken>());
            cacheEntryMock.SetupGet(e => e.PostEvictionCallbacks).Returns(new List<PostEvictionCallbackRegistration>());
            return cacheEntryMock;
        }

        #endregion

        #region Tests

        #region Tests JoinRankMatchMaker
        [Test(Description = "Testa o cenário 'feliz' onde um admin se junta, não encontra 'match' e é adicionado à cache.")]
        public async Task JoinRankMatchMaker_WhenNoMatchFound_AddsTeamToCacheAndReturnsEntry()
        {
            var team = CreateMockTeam(isMainPlayerAdmin: true, memberCount: 11);
            var hubCacheKey = GetHubCacheKey(idTeam);

            // idPlayer (string) é passado aqui
            validatorMock.Setup(v => v.ValidateVariableJoinRankMatchMaker(idPlayer, idTeam, hoursGame, connectionId));

            matchRepositoryMock.Setup(r => r.GetMatchProxim12HoursMatchs(idTeam, gameDate))
                .ReturnsAsync((Matches)null);

            validatorMock.Setup(v => v.ValidateHoursToMatch(It.IsAny<double>(), null));

            teamRepositoryMock.Setup(r => r.GetTeamWitchMemberRankAndPitchAsync(idTeam))
                .ReturnsAsync(team);

            validatorMock.Setup(v => v.ValidateTeamJoinRankMatchMaker(team));

            SetupCacheTryGetValue(hubCacheKey, false, (ConcurrentDictionary<Guid, EntryRankMatchMakerHub>)null);

            validatorMock.Setup(v => v.ValidateJoinRankMatchMaker(
                team,
                It.IsAny<float>(),
                It.IsAny<string>(),
                true,
                It.IsAny<ConcurrentDictionary<Guid, EntryRankMatchMakerHub>>()));

            SetupCacheTryGetValue(GlobalHubKeysCacheKey, false, (HashSet<string>)null);
            serviceMatchMakerMock.Setup(s => s.LogicMatchMakerJoinHub(It.IsAny<InfoTeamRankMatchMakerDto>(), It.IsAny<IEnumerable<InfoTeamRankMatchMakerDto>>(), gameDate))
                .Returns((Guid?)null);


            var hubCacheEntryMock = new Mock<ICacheEntry>();
            hubCacheEntryMock.SetupProperty(e => e.Value);
            hubCacheEntryMock.SetupProperty(e => e.AbsoluteExpiration);
            hubCacheEntryMock.SetupProperty(e => e.AbsoluteExpirationRelativeToNow);
            hubCacheEntryMock.SetupProperty(e => e.SlidingExpiration);
            hubCacheEntryMock.SetupProperty(e => e.Priority);
            hubCacheEntryMock.SetupGet(e => e.ExpirationTokens).Returns(new List<IChangeToken>());
            hubCacheEntryMock.SetupGet(e => e.PostEvictionCallbacks).Returns(new List<PostEvictionCallbackRegistration>());

            var globalKeysCacheEntryMock = new Mock<ICacheEntry>();
            globalKeysCacheEntryMock.SetupProperty(e => e.Value);
            globalKeysCacheEntryMock.SetupProperty(e => e.AbsoluteExpiration);
            globalKeysCacheEntryMock.SetupProperty(e => e.AbsoluteExpirationRelativeToNow);
            globalKeysCacheEntryMock.SetupProperty(e => e.SlidingExpiration);
            globalKeysCacheEntryMock.SetupProperty(e => e.Priority);
            globalKeysCacheEntryMock.SetupGet(e => e.ExpirationTokens).Returns(new List<IChangeToken>());
            globalKeysCacheEntryMock.SetupGet(e => e.PostEvictionCallbacks).Returns(new List<PostEvictionCallbackRegistration>());

            cacheMock.Setup(c => c.CreateEntry(hubCacheKey)).Returns(hubCacheEntryMock.Object);
            cacheMock.Setup(c => c.CreateEntry(GlobalHubKeysCacheKey)).Returns(globalKeysCacheEntryMock.Object);

            // idPlayer (string) é passado aqui
            var result = await service.JoinRankMatchMaker(idPlayer, idTeam, hoursGame, connectionId);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ConnectionId, Is.EqualTo(connectionId));
            Assert.That(result.Team.IdTeam, Is.EqualTo(idTeam));

            cacheMock.Verify(c => c.CreateEntry(hubCacheKey), Times.Once);
            cacheMock.Verify(c => c.CreateEntry(GlobalHubKeysCacheKey), Times.Once);
        }

        [Test(Description = "Testa se o serviço lança uma exceção se a equipa tiver menos de 11 jogadores.")]
        public void JoinRankMatchMaker_WhenTeamHasLessThan11Players_ThrowsInvalidOperationException()
        {
            var teamWith10Players = CreateMockTeam(isMainPlayerAdmin: true, memberCount: 10);
            var hubCacheKey = GetHubCacheKey(idTeam);

            // idPlayer (string) é passado aqui
            validatorMock.Setup(v => v.ValidateVariableJoinRankMatchMaker(idPlayer, idTeam, hoursGame, connectionId));

            matchRepositoryMock.Setup(r => r.GetMatchProxim12HoursMatchs(idTeam, gameDate))
                .ReturnsAsync((Matches)null);

            validatorMock.Setup(v => v.ValidateHoursToMatch(It.IsAny<double>(), null));

            teamRepositoryMock.Setup(r => r.GetTeamWitchMemberRankAndPitchAsync(idTeam))
                .ReturnsAsync(teamWith10Players);

            validatorMock.Setup(v => v.ValidateTeamJoinRankMatchMaker(teamWith10Players));

            SetupCacheTryGetValue(hubCacheKey, false, (ConcurrentDictionary<Guid, EntryRankMatchMakerHub>)null);

            validatorMock.Setup(v => v.ValidateJoinRankMatchMaker(
                    teamWith10Players,
                    It.IsAny<float>(),
                    It.IsAny<string>(),
                    true,
                    It.IsAny<ConcurrentDictionary<Guid, EntryRankMatchMakerHub>>()))
                .Throws(new InvalidOperationException("A equipa não pode jogar partidas rankeadas, porque ainda não tem no mínimo 11 jogadores"));

            // idPlayer (string) é passado aqui
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.JoinRankMatchMaker(idPlayer, idTeam, hoursGame, connectionId));

            Assert.That(exception.Message, Is.EqualTo("A equipa não pode jogar partidas rankeadas, porque ainda não tem no mínimo 11 jogadores"));
        }

        [Test(Description = "Testa se o serviço lança uma exceção se o jogador que tenta entrar no hub não for um admin da equipa.")]
        public void JoinRankMatchMaker_WhenPlayerIsNotAdmin_ThrowsNotFindException()
        {
            var team = CreateMockTeam(isMainPlayerAdmin: false, memberCount: 11, additionalAdminCount: 2);
            var hubCacheKey = GetHubCacheKey(idTeam);

            // idPlayer (string) é passado aqui
            validatorMock.Setup(v => v.ValidateVariableJoinRankMatchMaker(idPlayer, idTeam, hoursGame, connectionId));

            matchRepositoryMock.Setup(r => r.GetMatchProxim12HoursMatchs(idTeam, gameDate)).ReturnsAsync((Matches)null);

            validatorMock.Setup(v => v.ValidateHoursToMatch(It.IsAny<double>(), null));
            teamRepositoryMock.Setup(r => r.GetTeamWitchMemberRankAndPitchAsync(idTeam)).ReturnsAsync(team);

            validatorMock.Setup(v => v.ValidateTeamJoinRankMatchMaker(team));
            SetupCacheTryGetValue(hubCacheKey, false, (ConcurrentDictionary<Guid, EntryRankMatchMakerHub>)null);

            validatorMock.Setup(v => v.ValidateJoinRankMatchMaker(
                team,
                It.IsAny<float>(),
                It.IsAny<string>(),
                false,
                It.IsAny<ConcurrentDictionary<Guid, EntryRankMatchMakerHub>>()))
            .Throws(new NotFindException("O administrador que quer procurar uma partida ranqueada não existe"));

            // idPlayer (string) é passado aqui
            var exception = Assert.ThrowsAsync<NotFindException>(async () =>
                await service.JoinRankMatchMaker(idPlayer, idTeam, hoursGame, connectionId));

            Assert.That(exception.Message, Is.EqualTo("O administrador que quer procurar uma partida ranqueada não existe"));
        }

        [Test(Description = "Testa se o serviço lança uma exceção se um admin tentar entrar, mas outro admin da mesma equipa já estiver na cache.")]
        public void JoinRankMatchMaker_WhenTeamAlreadyInHub_ThrowsInvalidOperationException()
        {
            var team = CreateMockTeam(isMainPlayerAdmin: true, memberCount: 11);
            var hubCacheKey = GetHubCacheKey(idTeam);

            var existingHub = new ConcurrentDictionary<Guid, EntryRankMatchMakerHub>();
            existingHub.TryAdd(idTeam, new EntryRankMatchMakerHub());

            // idPlayer (string) é passado aqui
            validatorMock.Setup(v => v.ValidateVariableJoinRankMatchMaker(idPlayer, idTeam, hoursGame, connectionId));
            matchRepositoryMock.Setup(r => r.GetMatchProxim12HoursMatchs(idTeam, gameDate)).ReturnsAsync((Matches)null);

            validatorMock.Setup(v => v.ValidateHoursToMatch(It.IsAny<double>(), null));
            teamRepositoryMock.Setup(r => r.GetTeamWitchMemberRankAndPitchAsync(idTeam)).ReturnsAsync(team);

            validatorMock.Setup(v => v.ValidateTeamJoinRankMatchMaker(team));

            SetupCacheTryGetValue(hubCacheKey, true, existingHub);


            validatorMock.Setup(v => v.ValidateJoinRankMatchMaker(
                    team,
                    It.IsAny<float>(),
                    It.IsAny<string>(),
                    true,
                    existingHub))
                .Throws(new InvalidOperationException("Já existe um admin desta equipa a iniciar a partida"));

            // idPlayer (string) é passado aqui
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.JoinRankMatchMaker(idPlayer, idTeam, hoursGame, connectionId));

            Assert.That(exception.Message, Is.EqualTo("Já existe um admin desta equipa a iniciar a partida"));
        }

        [Test(Description = "Testa se a validação inicial de variáveis (ex: ID de jogador vazio) falha corretamente.")]
        public void JoinRankMatchMaker_WhenInputValidationFails_ThrowsArgumentException()
        {
            // CORRIGIDO: invalidPlayerId agora é string.Empty
            var invalidPlayerId = string.Empty;

            // Setup espera uma string
            validatorMock.Setup(v => v.ValidateVariableJoinRankMatchMaker(invalidPlayerId, idTeam, hoursGame, connectionId))
                .Throws(new ArgumentException("O id do jogador está vazio"));

            // Chamada de serviço com string
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.JoinRankMatchMaker(invalidPlayerId, idTeam, hoursGame, connectionId));

            Assert.That(exception.Message, Is.EqualTo("O id do jogador está vazio"));

            matchRepositoryMock.Verify(r => r.GetMatchProxim12HoursMatchs(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Never);
            teamRepositoryMock.Verify(r => r.GetTeamWitchMemberRankAndPitchAsync(It.IsAny<Guid>()), Times.Never);
        }
        #endregion

        #region Tests MatchMaker (BackgroundService)

        [Test(Description = "Testa se, quando são encontrados matches, o CreateMatch é chamado para cada par.")]
        public async Task MatchMaker_WhenMatchesAreFound_CreatesMatchesAndReturnsResult()
        {
            var criteria = new CriteriaMatchMaker();
            var gameDate = DateTime.UtcNow.Date;

            var teamEntry1 = CreateMockEntry(Guid.NewGuid(), gameDate);
            var teamEntry2 = CreateMockEntry(Guid.NewGuid(), gameDate);
            var teamEntry3 = CreateMockEntry(Guid.NewGuid(), gameDate);

            var team1 = new Team { Id = teamEntry1.Team.IdTeam, Pitch = new Pitch { Id = Guid.NewGuid() } };
            var team2 = new Team { Id = teamEntry2.Team.IdTeam, Pitch = new Pitch { Id = Guid.NewGuid() } };

            var key1 = GetHubCacheKey(teamEntry1.Team.IdTeam);
            var key2 = GetHubCacheKey(teamEntry2.Team.IdTeam);
            var key3 = GetHubCacheKey(teamEntry3.Team.IdTeam);
            var hubKeys = new HashSet<string> { key1, key2, key3 };

            var hub1 = new ConcurrentDictionary<Guid, EntryRankMatchMakerHub>();
            hub1.TryAdd(teamEntry1.Team.IdTeam, teamEntry1);

            var hub2 = new ConcurrentDictionary<Guid, EntryRankMatchMakerHub>();
            hub2.TryAdd(teamEntry2.Team.IdTeam, teamEntry2);

            var hub3 = new ConcurrentDictionary<Guid, EntryRankMatchMakerHub>();
            hub3.TryAdd(teamEntry3.Team.IdTeam, teamEntry3);

            SetupCacheTryGetValue(GlobalHubKeysCacheKey, true, hubKeys);
            SetupCacheTryGetValue(key1, true, hub1);
            SetupCacheTryGetValue(key2, true, hub2);
            SetupCacheTryGetValue(key3, true, hub3);

            var matchesFound = new Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub>
            {
                { teamEntry1, teamEntry2 }
            };
            serviceMatchMakerMock.Setup(s => s.LogicMatchMaker(It.IsAny<IEnumerable<EntryRankMatchMakerHub>>(), criteria))
                .Returns(matchesFound);

            teamRepositoryMock.Setup(r => r.GetTeamByIdAsync(teamEntry1.Team.IdTeam)).ReturnsAsync(team1);
            teamRepositoryMock.Setup(r => r.GetTeamByIdAsync(teamEntry2.Team.IdTeam)).ReturnsAsync(team2);
            unityOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await service.MatchMaker(criteria);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result, Is.EqualTo(matchesFound));

            serviceMatchMakerMock.Verify(s => s.LogicMatchMaker(It.IsAny<IEnumerable<EntryRankMatchMakerHub>>(), criteria), Times.Once);
            teamRepositoryMock.Verify(r => r.GetTeamByIdAsync(teamEntry1.Team.IdTeam), Times.Once);
            teamRepositoryMock.Verify(r => r.GetTeamByIdAsync(teamEntry2.Team.IdTeam), Times.Once);
            unityOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "Testa se, quando não são encontrados matches, o CreateMatch NUNCA é chamado.")]
        public async Task MatchMaker_WhenNoMatchesAreFound_DoesNotCreateMatches()
        {
            var criteria = new CriteriaMatchMaker();
            var teamEntry1 = CreateMockEntry(Guid.NewGuid(), gameDate);

            var key1 = GetHubCacheKey(teamEntry1.Team.IdTeam);
            var hubKeys = new HashSet<string> { key1 };
            var hub1 = new ConcurrentDictionary<Guid, EntryRankMatchMakerHub>();
            hub1.TryAdd(teamEntry1.Team.IdTeam, teamEntry1);

            SetupCacheTryGetValue(GlobalHubKeysCacheKey, true, hubKeys);
            SetupCacheTryGetValue(key1, true, hub1);

            var noMatchesFound = new Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub>();
            serviceMatchMakerMock.Setup(s => s.LogicMatchMaker(It.IsAny<IEnumerable<EntryRankMatchMakerHub>>(), criteria))
                .Returns(noMatchesFound);

            var result = await service.MatchMaker(criteria);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));

            serviceMatchMakerMock.Verify(s => s.LogicMatchMaker(It.IsAny<IEnumerable<EntryRankMatchMakerHub>>(), criteria), Times.Once);
            teamRepositoryMock.Verify(r => r.GetTeamByIdAsync(It.IsAny<Guid>()), Times.Never);
            unityOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Testa se, quando a cache está vazia, o método não falha e retorna um resultado vazio.")]
        public async Task MatchMaker_WhenCacheIsEmpty_ReturnsEmptyDictionary()
        {
            var criteria = new CriteriaMatchMaker();

            SetupCacheTryGetValue(GlobalHubKeysCacheKey, false, (HashSet<string>)null);

            var noMatchesFound = new Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub>();

            serviceMatchMakerMock.Setup(s => s.LogicMatchMaker(It.Is<IEnumerable<EntryRankMatchMakerHub>>(list => !list.Any()), criteria))
                .Returns(noMatchesFound);

            var result = await service.MatchMaker(criteria);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));

            serviceMatchMakerMock.Verify(s => s.LogicMatchMaker(It.Is<IEnumerable<EntryRankMatchMakerHub>>(list => !list.Any()), criteria), Times.Once);
            unityOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region Tests LeaveRankMatchMakerAsync

        [Test(Description = "Testa se o método falha se a validação inicial do ID da equipa falhar.")]
        public void LeaveRankMatchMakerAsync_InvalidTeamId_ThrowsArgumentException()
        {
            var invalidTeamId = Guid.Empty;
            validatorMock.Setup(v => v.ValidateLeaveRankMatchMaker(invalidTeamId))
                .Throws(new ArgumentException("O id da equipa está vazio"));

            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.LeaveRankMatchMakerAsync(invalidTeamId, connectionId));

            Assert.That(ex.Message, Is.EqualTo("O id da equipa está vazio"));
            cacheMock.Verify(c => c.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Never);
        }

        [Test(Description = "Testa se o método retorna false se o hub da equipa (hubCacheKey) não for encontrado na cache.")]
        public async Task LeaveRankMatchMakerAsync_HubNotFoundInCache_ReturnsFalse()
        {
            var hubCacheKey = GetHubCacheKey(idTeam);
            SetupCacheTryGetValue(hubCacheKey, false, (ConcurrentDictionary<Guid, EntryRankMatchMakerHub>)null);

            var result = await service.LeaveRankMatchMakerAsync(idTeam, connectionId);

            Assert.That(result, Is.False);
            validatorMock.Verify(v => v.ValidateLeaveRankMatchMaker(idTeam), Times.Once);
        }

        [Test(Description = "Testa se o método retorna false se o hub for encontrado, mas a equipa específica não estiver nesse hub.")]
        public async Task LeaveRankMatchMakerAsync_TeamNotInHub_ReturnsFalse()
        {
            var hubCacheKey = GetHubCacheKey(idTeam);
            var hub = new ConcurrentDictionary<Guid, EntryRankMatchMakerHub>();
            hub.TryAdd(Guid.NewGuid(), CreateMockEntry(Guid.NewGuid(), gameDate));

            SetupCacheTryGetValue(hubCacheKey, true, hub);

            var result = await service.LeaveRankMatchMakerAsync(idTeam, connectionId);

            Assert.That(result, Is.False);
        }

        [Test(Description = "Testa se, ao remover a última equipa do hub, o hub e a chave global são removidos da cache.")]
        public async Task LeaveRankMatchMakerAsync_LastTeamLeaves_RemovesHubAndGlobalKey_ReturnsTrue()
        {
            var hubCacheKey = GetHubCacheKey(idTeam);
            var globalKeys = new HashSet<string> { hubCacheKey, "outra-chave" };

            var hub = new ConcurrentDictionary<Guid, EntryRankMatchMakerHub>();
            hub.TryAdd(idTeam, CreateMockEntry(idTeam, gameDate)); // A única equipa no hub

            SetupCacheTryGetValue(hubCacheKey, true, hub);
            SetupCacheTryGetValue(GlobalHubKeysCacheKey, true, globalKeys);

            cacheMock.Setup(c => c.Remove(hubCacheKey));

            var globalKeyEntryMock = CreateCacheEntryMock();
            cacheMock.Setup(c => c.CreateEntry(GlobalHubKeysCacheKey)).Returns(globalKeyEntryMock.Object);

            var result = await service.LeaveRankMatchMakerAsync(idTeam, connectionId);

            Assert.That(result, Is.True);
            Assert.That(hub.IsEmpty, Is.True);
            Assert.That(globalKeys.Contains(hubCacheKey), Is.False);

            cacheMock.Verify(c => c.Remove(hubCacheKey), Times.Once);
            cacheMock.Verify(c => c.CreateEntry(hubCacheKey), Times.Never);
            cacheMock.Verify(c => c.CreateEntry(GlobalHubKeysCacheKey), Times.Once);
        }

        [Test(Description = "Testa se, ao remover uma equipa, o hub (não vazio) é atualizado na cache.")]
        public async Task LeaveRankMatchMakerAsync_TeamLeavesHubNotEmpty_UpdatesHub_ReturnsTrue()
        {
            var hubCacheKey = GetHubCacheKey(idTeam);
            var otherTeamId = Guid.NewGuid();
            var globalKeys = new HashSet<string> { hubCacheKey };

            var hub = new ConcurrentDictionary<Guid, EntryRankMatchMakerHub>();
            hub.TryAdd(idTeam, CreateMockEntry(idTeam, gameDate));
            hub.TryAdd(otherTeamId, CreateMockEntry(otherTeamId, gameDate));

            SetupCacheTryGetValue(hubCacheKey, true, hub);
            SetupCacheTryGetValue(GlobalHubKeysCacheKey, true, globalKeys);

            var hubKeyEntryMock = CreateCacheEntryMock();
            var globalKeyEntryMock = CreateCacheEntryMock();
            cacheMock.Setup(c => c.CreateEntry(hubCacheKey)).Returns(hubKeyEntryMock.Object);
            cacheMock.Setup(c => c.CreateEntry(GlobalHubKeysCacheKey)).Returns(globalKeyEntryMock.Object);

            var result = await service.LeaveRankMatchMakerAsync(idTeam, connectionId);

            Assert.That(result, Is.True);
            Assert.That(hub.IsEmpty, Is.False);
            Assert.That(hub.ContainsKey(otherTeamId), Is.True);

            cacheMock.Verify(c => c.Remove(hubCacheKey), Times.Never);

            cacheMock.Verify(c => c.CreateEntry(hubCacheKey), Times.Exactly(2));
            cacheMock.Verify(c => c.CreateEntry(GlobalHubKeysCacheKey), Times.Once);
        }

        #endregion

        #region Tests HandleDisconnectAsync

        [Test(Description = "Testa se HandleDisconnectAsync retorna false imediatamente se o teamId for nulo.")]
        public async Task HandleDisconnectAsync_WhenTeamIdIsNull_ReturnsFalse()
        {
            Guid? nullTeamId = null;

            var result = await service.HandleDisconnectAsync(nullTeamId, connectionId);

            Assert.That(result, Is.False);

            validatorMock.Verify(v => v.ValidateLeaveRankMatchMaker(It.IsAny<Guid>()), Times.Never);
            cacheMock.Verify(c => c.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Never);
        }

        [Test(Description = "Testa se, com um ID válido, o método chama LeaveRankMatchMakerAsync e retorna true (se a saída for bem-sucedida).")]
        public async Task HandleDisconnectAsync_WhenTeamIdIsValidAndLeaveSucceeds_ReturnsTrue()
        {
            var hubCacheKey = GetHubCacheKey(idTeam);
            var globalKeys = new HashSet<string> { hubCacheKey };
            var hub = new ConcurrentDictionary<Guid, EntryRankMatchMakerHub>();
            hub.TryAdd(idTeam, CreateMockEntry(idTeam, gameDate));

            SetupCacheTryGetValue(hubCacheKey, true, hub);
            SetupCacheTryGetValue(GlobalHubKeysCacheKey, true, globalKeys);
            cacheMock.Setup(c => c.Remove(hubCacheKey));

            var globalKeyEntryMock = CreateCacheEntryMock();
            cacheMock.Setup(c => c.CreateEntry(GlobalHubKeysCacheKey)).Returns(globalKeyEntryMock.Object);

            var result = await service.HandleDisconnectAsync(idTeam, connectionId);

            Assert.That(result, Is.True);

            validatorMock.Verify(v => v.ValidateLeaveRankMatchMaker(idTeam), Times.Once);
            cacheMock.Verify(c => c.Remove(hubCacheKey), Times.Once);
            cacheMock.Verify(c => c.CreateEntry(GlobalHubKeysCacheKey), Times.Once);
        }

        [Test(Description = "Testa se, com um ID válido, o método chama LeaveRankMatchMakerAsync e retorna false (se a saída falhar).")]
        public async Task HandleDisconnectAsync_WhenTeamIdIsValidAndLeaveFails_ReturnsFalse()
        {
            var hubCacheKey = GetHubCacheKey(idTeam);
            SetupCacheTryGetValue(hubCacheKey, false, (ConcurrentDictionary<Guid, EntryRankMatchMakerHub>)null);

            var result = await service.HandleDisconnectAsync(idTeam, connectionId);

            Assert.That(result, Is.False);

            validatorMock.Verify(v => v.ValidateLeaveRankMatchMaker(idTeam), Times.Once);
        }

        #endregion

        #endregion
    }
}