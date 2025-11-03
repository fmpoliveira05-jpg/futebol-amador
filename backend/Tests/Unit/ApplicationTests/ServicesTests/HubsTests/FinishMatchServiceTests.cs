using Application.DTOs.Match;
using Application.Hubs;
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
    public class FinishMatchServiceTests
    {
        #region Variables
        private Mock<IMatchRepository> mockMatchRepository;
        private Mock<IUnityOfWork> mockUnitOfWork;
        private Mock<IFinishMatchValidator> mockValidator;
        private Mock<IGeralHubValidator> mockGeralValidator;
        private IMemoryCache cache;
        private ManagerFinishMatchService service;

        private Guid matchId;
        private Guid teamId1, teamId2;
        private Guid adminId1, adminId2;
        private string connId1, connId2;
        private Rank goldRank, bronzeRank, silverRank;
        private Matches testMatch;
        private ResultMatchDto resultDtoTeam1;
        private ResultMatchDto resultDtoTeam2Match;
        private ResultMatchDto resultDtoTeam2Mismatch;
        #endregion

        #region Setup
        [SetUp]
        public void Setup()
        {
            mockMatchRepository = new Mock<IMatchRepository>();
            mockUnitOfWork = new Mock<IUnityOfWork>();
            mockValidator = new Mock<IFinishMatchValidator>();
            mockGeralValidator = new Mock<IGeralHubValidator>();

            cache = new MemoryCache(new MemoryCacheOptions());

            service = new ManagerFinishMatchService(
                mockMatchRepository.Object,
                mockUnitOfWork.Object,
                mockValidator.Object,
                mockGeralValidator.Object,
                cache
            );

            matchId = Guid.NewGuid();
            teamId1 = Guid.NewGuid();
            teamId2 = Guid.NewGuid();
            adminId1 = Guid.NewGuid();
            adminId2 = Guid.NewGuid();
            connId1 = "conn1";
            connId2 = "conn2";

            resultDtoTeam1 = new ResultMatchDto
            {
                IdTeam = teamId1,
                IdOpponent = teamId2,
                NumGoalsTeam = 2,
                NumGoalsOpponent = 1
            };

            resultDtoTeam2Match = new ResultMatchDto
            {
                IdTeam = teamId2,
                IdOpponent = teamId1,
                NumGoalsTeam = 1,
                NumGoalsOpponent = 2
            };

            resultDtoTeam2Mismatch = new ResultMatchDto
            {
                IdTeam = teamId2,
                IdOpponent = teamId1,
                NumGoalsTeam = 3,
                NumGoalsOpponent = 0
            };

            goldRank = new Rank
            {
                Id = Guid.Parse("BA8A024E-AD69-4D24-BB51-EB6310BD3046"),
                PointsToPromotion = 40
            };

            bronzeRank = new Rank
            {
                Id = Guid.Parse("DBCA347B-6C0F-4AB9-AB30-BE4610B210D3"),
                PointsToPromotion = 20
            };

            silverRank = new Rank
            {
                Id = Guid.Parse("54AE9AF9-0F7F-4D31-AB56-F16F69EE9B4D"),
                WinPoints = 4,
                DrawPoints = 2,
                LosePoints = -2,
                PointsToPromotion = 30,
                NextRank = goldRank,
                PreviousRank = bronzeRank
            };

            var admin1 = new Player { Id = adminId1, IsAdmin = true };
            var admin2 = new Player { Id = adminId2, IsAdmin = true };

            var team1 = new Team { Id = teamId1, Rank = silverRank, Members = new List<Player> { admin1 }, CurrentPoints = 28 };
            var team2 = new Team { Id = teamId2, Rank = silverRank, Members = new List<Player> { admin2 }, CurrentPoints = 21 };

            var teamStat1 = new TeamStatistics { IdTeam = teamId1, Team = team1 };
            var teamStat2 = new TeamStatistics { IdTeam = teamId2, Team = team2 };

            testMatch = new Matches
            {
                Id = matchId,
                MatchStatus = MatchStatus.IN_PROGRESS,
                Teams = new List<TeamStatistics> { teamStat1, teamStat2 }
            };

            mockMatchRepository.Setup(r => r.GetMatchWithListPlayerById(matchId))
                .ReturnsAsync(testMatch);

            mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);
        }
        #endregion

        #region Methods Suportt
        [TearDown]
        public void TearDown()
        {
            cache.Dispose();
        }

        private static string GetHubCacheKey(Guid matchId)
        {
            return ModelConstants.FinishMatchHubConst.PrefixHubCache + matchId;
        }

        #endregion

        #region Tests

        #region Tests JoinHub

        #region Tests Join Admins
        [Test(Description = "Testa se o primeiro admin a entrar no hub é marcado como 'IsFirstAdmin' e é adicionado à cache.")]
        public async Task JoinHubAsync_FirstAdminJoins_ReturnsIsFirstAdminTrueAndAddsToCache()
        {
            var hubCacheKey = GetHubCacheKey(matchId);
            var result = await service.JoinHubAsync(matchId, resultDtoTeam1, adminId1, connId1);

            Assert.That(result.IsFirstAdmin, Is.True);
            Assert.That(result.MatchFinish, Is.False);
            Assert.That(result.IdTeam, Is.EqualTo(teamId1));
            Assert.That(result.ResultMatch, Is.EqualTo(resultDtoTeam1));

            Assert.That(cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryHubFinishMatch> cachedHub), Is.True);
            Assert.That(cachedHub, Is.Not.Null);
            Assert.That(cachedHub.Count, Is.EqualTo(1));
            Assert.That(cachedHub.ContainsKey(teamId1), Is.True);
            Assert.That(cachedHub[teamId1].ConnectionId, Is.EqualTo(connId1));

            mockValidator.Verify(v =>
                v.ValidateJoinMatch(
                    It.IsAny<TeamStatistics>(),
                    teamId1,
                    It.IsAny<ConcurrentDictionary<Guid, EntryHubFinishMatch>>()
                ), Times.Once);

            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Testa se o segundo admin a entrar com um resultado diferente não finaliza a partida e não limpa a cache.")]
        public async Task JoinHubAsync_SecondAdminJoins_ResultsMismatch_DoesNotFinalizeMatch()
        {
            var hubCacheKey = GetHubCacheKey(matchId);

            var hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();
            var firstAdminResult = new JoinFinishMatch { IdTeam = teamId1, ResultMatch = resultDtoTeam1 };
            var firstEntry = new EntryHubFinishMatch { ConnectionId = connId1, Result = firstAdminResult };
            hub.TryAdd(teamId1, firstEntry);
            cache.Set(hubCacheKey, hub);

            var result = await service.JoinHubAsync(matchId, resultDtoTeam2Mismatch, adminId2, connId2);

            Assert.That(result.IsFirstAdmin, Is.False);
            Assert.That(result.MatchFinish, Is.True);
            Assert.That(result.IsCoincides, Is.Not.True);
            Assert.That(result.FirstAdminConnectionId, Is.EqualTo(connId1));

            mockValidator.Verify(v =>
                v.ValidateJoinMatch(
                    It.IsAny<TeamStatistics>(),
                    teamId2,
                    It.IsAny<ConcurrentDictionary<Guid, EntryHubFinishMatch>>()
                ), Times.Once);

            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);

            Assert.That(cache.TryGetValue(hubCacheKey, out _), Is.True);
        }

        [Test(Description = "Testa o 'happy path' onde o segundo admin entra com um resultado coincidente, a partida é finalizada, a BD é atualizada e a cache é limpa.")]
        public async Task JoinHubAsync_SecondAdminJoins_ResultsMatch_FinalizesMatchUpdatesAndClearsCache()
        {
            var hubCacheKey = GetHubCacheKey(matchId);
            var hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();
            var firstAdminResult = new JoinFinishMatch { IdTeam = teamId1, ResultMatch = resultDtoTeam1 };
            var firstEntry = new EntryHubFinishMatch { ConnectionId = connId1, Result = firstAdminResult };
            hub.TryAdd(teamId1, firstEntry);
            cache.Set(hubCacheKey, hub);

            var result = await service.JoinHubAsync(matchId, resultDtoTeam2Match, adminId2, connId2);

            Assert.That(result.IsFirstAdmin, Is.False);
            Assert.That(result.MatchFinish, Is.True);
            Assert.That(result.IsCoincides, Is.True);
            Assert.That(result.FirstAdminConnectionId, Is.EqualTo(connId1));

            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);

            Assert.That(cache.TryGetValue(hubCacheKey, out _), Is.False);

            var teamStat1 = testMatch.Teams.First(t => t.IdTeam == teamId1);
            var teamStat2 = testMatch.Teams.First(t => t.IdTeam == teamId2);

            Assert.That(testMatch.MatchStatus, Is.EqualTo(MatchStatus.DONE));
            Assert.That(teamStat1.MatchResult, Is.EqualTo(MatchResult.WIN));
            Assert.That(teamStat2.MatchResult, Is.EqualTo(MatchResult.LOSE));

            Assert.That(teamStat1.Team.CurrentPoints, Is.EqualTo(32));
            Assert.That(teamStat1.Team.Rank.Id, Is.EqualTo(goldRank.Id));

            Assert.That(teamStat2.Team.CurrentPoints, Is.EqualTo(19));
            Assert.That(teamStat2.Team.Rank.Id, Is.EqualTo(bronzeRank.Id));

            mockValidator.Verify(v => v.ValidateOpponentTeam(teamStat1), Times.Once);
        }
        #endregion

        #region Tests Validations and Fails
        [Test(Description = "Testa se o método falha (lança ArgumentException) quando os parâmetros de entrada básicos são inválidos.")]
        public void JoinHubAsync_InvalidInputVariables_ThrowsArgumentException()
        {
            mockValidator.Setup(v => v.ValidateVariableJoinMatch(Guid.Empty, null, Guid.Empty, null))
                .Throws(new ArgumentException("Test Exception"));

            Assert.ThrowsAsync<ArgumentException>(() =>
                service.JoinHubAsync(Guid.Empty, null, Guid.Empty, null));

            mockValidator.Verify(v =>
                v.ValidateVariableJoinMatch(Guid.Empty, null, Guid.Empty, null), Times.Once);
        }

        [Test(Description = "Testa se o método falha (lança ArgumentException) se a partida (matchId) não for encontrada no repositório.")]
        public void JoinHubAsync_MatchNotFound_ThrowsArgumentException()
        {
            mockMatchRepository.Setup(r => r.GetMatchWithListPlayerById(matchId))
                .ReturnsAsync((Matches)null);

            mockValidator.Setup(v => v.ValidateMatchJoinMatch(null))
                .Throws(new ArgumentException("A match não existe"));

            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                service.JoinHubAsync(matchId, resultDtoTeam1, adminId1, connId1));

            Assert.That(ex.Message, Is.EqualTo("A match não existe"));
            mockValidator.Verify(v => v.ValidateMatchJoinMatch(null), Times.Once);
        }

        [Test(Description = "Testa se o método falha (lança ArgumentException) quando o utilizador que tenta entrar no hub não é admin de nenhuma equipa na partida.")]
        public void JoinHubAsync_UserIsNotAdmin_ThrowsArgumentException()
        {
            var nonAdminId = Guid.NewGuid();

            Assert.ThrowsAsync<NullReferenceException>(() =>
        service.JoinHubAsync(matchId, resultDtoTeam1, nonAdminId, connId1));
        }

        [Test(Description = "Testa se o método falha (lança InvalidOperationException) se um admin da mesma equipa já estiver no hub.")]
        public void JoinHubAsync_AdminAlreadyInHub_ThrowsInvalidOperationException()
        {
            var hubCacheKey = GetHubCacheKey(matchId);
            var hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();
            var entry = new EntryHubFinishMatch { ConnectionId = connId1, Result = new JoinFinishMatch() };
            hub.TryAdd(teamId1, entry);
            cache.Set(hubCacheKey, hub);

            mockValidator.Setup(v => v.ValidateJoinMatch(It.IsAny<TeamStatistics>(), teamId1, hub))
                .Throws(new InvalidOperationException("Já existe um admin desta equipa"));

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.JoinHubAsync(matchId, resultDtoTeam1, adminId1, "another_connection"));

            Assert.That(ex.Message, Is.EqualTo("Já existe um admin desta equipa"));
        }

        [Test(Description = "Testa se o método falha (lança InvalidOperationException) se a partida começou há menos de 90 minutos.")]
        public void JoinHubAsync_MatchNotYet90Minutes_ThrowsInvalidOperationException()
        {
            testMatch.TimeStart = DateTime.UtcNow.AddMinutes(-80); 

            var expectedMessage = "Ainda não passaram 90 minutos";
            mockValidator.Setup(v => v.ValidateMatchJoinMatch(testMatch))
                .Throws(new InvalidOperationException(expectedMessage)); 

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.JoinHubAsync(matchId, resultDtoTeam1, adminId1, connId1)
            );

            Assert.That(ex.Message, Does.StartWith(expectedMessage));

            mockMatchRepository.Verify(r => r.GetMatchWithListPlayerById(matchId), Times.Once);
            mockValidator.Verify(v => v.ValidateMatchJoinMatch(testMatch), Times.Once);

            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
            Assert.That(cache.TryGetValue(GetHubCacheKey(matchId), out _), Is.False);
        }

        #endregion

        #endregion

        #region Tests UpdateResult

        #region Tests admins
        [Test(Description = "Testa o 'happy path' onde um admin atualiza o seu resultado, este agora coincide, e a partida é finalizada.")]
        public async Task UpdateResult_ResultNowMatches_FinalizesMatchUpdatesAndClearsCache()
        {
            var hubCacheKey = GetHubCacheKey(matchId);
            var hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();
            var admin1Entry = new EntryHubFinishMatch { ConnectionId = connId1, Result = new JoinFinishMatch { IdTeam = teamId1, ResultMatch = resultDtoTeam1 } };
            var admin2Entry_Old = new EntryHubFinishMatch { ConnectionId = connId2, Result = new JoinFinishMatch { IdTeam = teamId2, ResultMatch = resultDtoTeam2Mismatch } };

            hub.TryAdd(teamId1, admin1Entry);
            hub.TryAdd(teamId2, admin2Entry_Old);
            cache.Set(hubCacheKey, hub);

            var result = await service.UpdateResult(matchId, resultDtoTeam2Match, adminId2, connId2);

            Assert.That(result.IsCoincides, Is.True);
            Assert.That(result.IdTeam, Is.EqualTo(teamId2));

            mockValidator.Verify(v => v.ValidateVariableJoinMatch(matchId, resultDtoTeam2Match, adminId2, connId2), Times.Once);
            mockValidator.Verify(v => v.ValidateUpdateResult(It.IsAny<TeamStatistics>(), teamId2, hub), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);

            Assert.That(cache.TryGetValue(hubCacheKey, out _), Is.False);
        }

        [Test(Description = "Testa quando um admin atualiza o seu resultado, mas este ainda não coincide com o do outro admin.")]
        public async Task UpdateResult_ResultStillMismatches_UpdatesCacheButDoesNotFinalize()
        {
            var hubCacheKey = GetHubCacheKey(matchId);
            var hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();
            var admin1Entry = new EntryHubFinishMatch { ConnectionId = connId1, Result = new JoinFinishMatch { IdTeam = teamId1, ResultMatch = resultDtoTeam1 } };
            var admin2Entry_Old = new EntryHubFinishMatch { ConnectionId = connId2, Result = new JoinFinishMatch { IdTeam = teamId2, ResultMatch = resultDtoTeam2Mismatch } };

            hub.TryAdd(teamId1, admin1Entry);
            hub.TryAdd(teamId2, admin2Entry_Old);
            cache.Set(hubCacheKey, hub);

            var newMismatchDto = new ResultMatchDto { IdTeam = teamId2, IdOpponent = teamId1, NumGoalsTeam = 10, NumGoalsOpponent = 10 };
            var result = await service.UpdateResult(matchId, newMismatchDto, adminId2, connId2);

            Assert.That(result.IsCoincides, Is.Not.True);

            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);

            Assert.That(cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryHubFinishMatch> cachedHub), Is.True);
            Assert.That(cachedHub[teamId2].Result.ResultMatch, Is.EqualTo(newMismatchDto));
            Assert.That(cachedHub[teamId1].Result.ResultMatch, Is.EqualTo(resultDtoTeam1));
        }
        #endregion

        #region Tests Fails and Validations

        [Test(Description = "Testa se UpdateResult falha se o admin que tenta atualizar não estiver no hub.")]
        public void UpdateResult_AdminNotInHub_ThrowsException()
        {
            var hubCacheKey = GetHubCacheKey(matchId);
            var hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();
            var admin1Entry = new EntryHubFinishMatch { ConnectionId = connId1, Result = new JoinFinishMatch { IdTeam = teamId1, ResultMatch = resultDtoTeam1 } };
            
            hub.TryAdd(teamId1, admin1Entry);
            cache.Set(hubCacheKey, hub);

            mockValidator.Setup(v => v.ValidateUpdateResult(It.IsAny<TeamStatistics>(), teamId2, hub))
                .Throws(new InvalidOperationException("Este admin não está no hub"));

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateResult(matchId, resultDtoTeam2Match, adminId2, connId2));

            Assert.That(ex.Message, Is.EqualTo("Este admin não está no hub"));
            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Testa se UpdateResult falha se o hub estiver vazio (ninguém entrou).")]
        public void UpdateResult_HubIsEmpty_ThrowsException()
        {
            mockValidator.Setup(v => v.ValidateUpdateResult(It.IsAny<TeamStatistics>(), teamId1, It.Is<ConcurrentDictionary<Guid, EntryHubFinishMatch>>(h => h.Count == 0)))
                .Throws(new InvalidOperationException("O hub está vazio."));

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateResult(matchId, resultDtoTeam1, adminId1, connId1));

            Assert.That(ex.Message, Is.EqualTo("O hub está vazio."));
        }

        [Test(Description = "Testa se UpdateResult falha se o utilizador não for um admin (NullReference).")]
        public void UpdateResult_UserIsNotAdmin_ThrowsNullReferenceException()
        {
            var nonAdminId = Guid.NewGuid();

            Assert.ThrowsAsync<NullReferenceException>(() =>
                service.UpdateResult(matchId, resultDtoTeam1, nonAdminId, connId1));
        }
        #endregion
       
        #endregion

        #region Tests LeaveHub

        [Test(Description = "Testa se LeaveHubAsync falha (lança Exceção) se os IDs forem inválidos.")]
        public void LeaveHubAsync_InvalidIds_ThrowsException()
        {
            var invalidMatchId = Guid.Empty;
            var invalidTeamId = Guid.Empty;

            mockGeralValidator
                .Setup(v => v.ValidateIdMatchLeaveMatch(invalidMatchId, invalidTeamId))
                .Throws(new ArgumentException("IDs inválidos"));

            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                service.LeaveHubAsync(invalidMatchId, invalidTeamId, "conn1"));

            Assert.That(ex.Message, Is.EqualTo("IDs inválidos"));
        }

        [Test(Description = "Testa se LeaveHubAsync retorna false se o hub (matchId) não for encontrado na cache.")]
        public async Task LeaveHubAsync_HubNotFoundInCache_ReturnsFalse()
        {
            var result = await service.LeaveHubAsync(matchId, teamId1, "conn1");

            Assert.That(result, Is.False);

            mockGeralValidator.Verify(v => v.ValidateIdMatchLeaveMatch(matchId, teamId1), Times.Once);
        }

        [Test(Description = "Testa se LeaveHubAsync retorna false se o hub for encontrado, mas a equipa (teamId) não estiver no hub.")]
        public async Task LeaveHubAsync_TeamNotInHub_ReturnsFalse()
        {
            var hubCacheKey = GetHubCacheKey(matchId);
            var hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();

            hub.TryAdd(teamId2, new EntryHubFinishMatch { ConnectionId = connId2, Result = new JoinFinishMatch() });
            cache.Set(hubCacheKey, hub);

            var result = await service.LeaveHubAsync(matchId, teamId1, "conn1");

            Assert.That(result, Is.False);
            Assert.That(cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryHubFinishMatch> cachedHub), Is.True);
            Assert.That(cachedHub.Count, Is.EqualTo(1));
            Assert.That(cachedHub.ContainsKey(teamId2), Is.True);
        }

        [Test(Description = "Testa se, quando o último admin sai, o hub é completamente removido da cache.")]
        public async Task LeaveHubAsync_LastAdminLeaves_RemovesHubFromCacheAndReturnsTrue()
        {
            var hubCacheKey = GetHubCacheKey(matchId);
            var hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();

            hub.TryAdd(teamId1, new EntryHubFinishMatch { ConnectionId = connId1, Result = new JoinFinishMatch() });
            cache.Set(hubCacheKey, hub);
            
            var result = await service.LeaveHubAsync(matchId, teamId1, "conn1");

            Assert.That(result, Is.True);
            Assert.That(cache.TryGetValue(hubCacheKey, out _), Is.False);
        }

        [Test(Description = "Testa se, quando um admin sai mas outro permanece, a cache é atualizada (não removida).")]
        public async Task LeaveHubAsync_OneAdminLeaves_UpdatesCacheWithRemainingAdminAndReturnsTrue()
        {
            var hubCacheKey = GetHubCacheKey(matchId);
            var hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();

            hub.TryAdd(teamId1, new EntryHubFinishMatch { ConnectionId = connId1, Result = new JoinFinishMatch() });
            hub.TryAdd(teamId2, new EntryHubFinishMatch { ConnectionId = connId2, Result = new JoinFinishMatch() });
            cache.Set(hubCacheKey, hub);

            var result = await service.LeaveHubAsync(matchId, teamId1, "conn1");

            Assert.That(result, Is.True);
            Assert.That(cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryHubFinishMatch> cachedHub), Is.True);
            Assert.That(cachedHub.Count, Is.EqualTo(1));
            Assert.That(cachedHub.ContainsKey(teamId2), Is.True);
            Assert.That(cachedHub.ContainsKey(teamId1), Is.False);
        }
        #endregion

        #region Tests HandleDisconnectAsync

        [Test(Description = "Testa se HandleDisconnectAsync retorna false se o matchId for nulo.")]
        public async Task HandleDisconnectAsync_NullMatchId_ReturnsFalse()
        {
            Guid? nullMatchId = null;
            Guid? validTeamId = teamId1;

            var result = await service.HandleDisconnectAsync(nullMatchId, validTeamId, connId1);

            Assert.That(result, Is.False);
        }

        [Test(Description = "Testa se HandleDisconnectAsync retorna false se o teamId for nulo.")]
        public async Task HandleDisconnectAsync_NullTeamId_ReturnsFalse()
        {
            Guid? validMatchId = matchId;
            Guid? nullTeamId = null;

            var result = await service.HandleDisconnectAsync(validMatchId, nullTeamId, connId1);

            Assert.That(result, Is.False);
        }

        [Test(Description = "Testa se HandleDisconnectAsync retorna false se ambos os IDs forem nulos.")]
        public async Task HandleDisconnectAsync_BothIdsNull_ReturnsFalse()
        {
            Guid? nullMatchId = null;
            Guid? nullTeamId = null;

            var result = await service.HandleDisconnectAsync(nullMatchId, nullTeamId, connId1);

            Assert.That(result, Is.False);
        }

        [Test(Description = "Testa se, com IDs válidos, HandleDisconnectAsync chama LeaveHubAsync e retorna true (se o 'leave' for bem-sucedido).")]
        public async Task HandleDisconnectAsync_ValidIds_AndLeaveSucceeds_ReturnsTrue()
        {
            var hubCacheKey = GetHubCacheKey(matchId);
            var hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();
            hub.TryAdd(teamId1, new EntryHubFinishMatch { ConnectionId = connId1, Result = new JoinFinishMatch() });
            cache.Set(hubCacheKey, hub);

            Guid? validMatchId = matchId;
            Guid? validTeamId = teamId1;

            var result = await service.HandleDisconnectAsync(validMatchId, validTeamId, connId1);

            Assert.That(result, Is.True);
            Assert.That(cache.TryGetValue(hubCacheKey, out _), Is.False);
        }

        [Test(Description = "Testa se, com IDs válidos, HandleDisconnectAsync chama LeaveHubAsync e retorna false (se o 'leave' falhar).")]
        public async Task HandleDisconnectAsync_ValidIds_AndLeaveFails_ReturnsFalse()
        {
            Guid? validMatchId = matchId;
            Guid? validTeamId = teamId1;

            var result = await service.HandleDisconnectAsync(validMatchId, validTeamId, connId1);

            Assert.That(result, Is.False);
        }

        #endregion

        #endregion
    }
}