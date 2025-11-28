using Api.BackGroundServices;
using Application.DTOs.Match;
using Application.DTOs.Pitch;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Hub;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Reflection;

namespace Tests.Unit.ApplicationTests.BackGroundServiceTests
{
    public class NotificationBackGroundServiceTests
    {
        [Test(Description = "CheckAndNotifyMatchesAsync deve chamar SendTeamAsync para cada equipa de cada match.")]
        public async Task CheckAndNotifyMatchesAsync_CallsNotificationService()
        {
            // Arrange
            var mockMatchRepo = new Mock<IMatchRepository>();
            var mockNotificationService = new Mock<INotificationService>();
            var mockLogger = new Mock<ILogger<NotificationBackGroundService>>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();

            var teamAId = Guid.NewGuid();
            var teamBId = Guid.NewGuid();

            var info = new InfoMatchCalendar
            {
                IdMatch = Guid.NewGuid(),
                MatchStatus = MatchStatus.SCHEDULED,
                GameDate = DateTime.UtcNow,
                MatchResult = MatchResult.UNPLAYED,
                IsCompetitive = true,
                IsHome = true,
                // --- CORREÇÃO AQUI: Usar TeamStatisticsDto e nomes corretos ---
                Team = new TeamStatisticsDto
                {
                    IdTeam = teamAId,
                    Name = "Team A",
                    NumGoals = 0,
                },
                Opponent = new TeamStatisticsDto
                {
                    IdTeam = teamBId,
                    Name = "Team B",
                    NumGoals = 0,
                },
                PitchGame = new PitchDto // Nome correto da propriedade é PascalCase
                {
                    Name = "Pitch X",
                    Address = "Rua Y"
                }
            };
            var matchesList = new List<InfoMatchCalendar> { info };

            mockMatchRepo
                .Setup(r => r.GetMatchesByDateAsync(It.IsAny<DateTime>()))
                .Returns(Task.FromResult(matchesList));

            var service = new NotificationBackGroundService(scopeFactoryMock.Object, mockLogger.Object);

            // Act: invocar via reflection o método privado CheckAndNotifyMatchesAsync
            var method = typeof(NotificationBackGroundService).GetMethod("CheckAndNotifyMatchesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "Método privado CheckAndNotifyMatchesAsync não encontrado por reflection.");

            var task = (Task)method.Invoke(service, new object[] { CancellationToken.None, mockMatchRepo.Object, mockNotificationService.Object })!;
            await task;

            var sendCalls = mockNotificationService.Invocations
                .Count(i => string.Equals(i.Method.Name, nameof(INotificationService.SendTeamAsync), StringComparison.Ordinal));

            Assert.That(sendCalls, Is.EqualTo(2), "Esperava duas chamadas a SendTeamAsync (uma para cada equipa).");

            var argsList = mockNotificationService.Invocations
                .Where(i => string.Equals(i.Method.Name, nameof(INotificationService.SendTeamAsync), StringComparison.Ordinal))
                .Select(i => i.Arguments.Count > 0 ? i.Arguments[0]?.ToString() : null)  // ← alterado de Length para Count
                .Where(a => a != null)
                .ToList();

            Assert.That(argsList, Does.Contain(teamAId.ToString()));
            Assert.That(argsList, Does.Contain(teamBId.ToString()));
        }
    }
}