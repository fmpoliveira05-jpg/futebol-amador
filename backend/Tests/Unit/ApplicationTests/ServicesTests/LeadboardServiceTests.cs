using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Services;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Tests.Unit.ApplicationTests.ServicesTests
{
    [TestFixture]
    public class LeadboardServiceTests
    {
        #region Variables
        
        private Mock<ITeamRepository> _teamRepositoryMock = null!;
        private LeaderboardService _service = null!;

        #endregion

        #region SetUp
        [SetUp]
        public void SetUp()
        {
            _teamRepositoryMock = new Mock<ITeamRepository>();
            _service = new LeaderboardService(_teamRepositoryMock.Object);
        }

        #endregion

        #region Tests
        [Test]
        public async Task GetLeaderboardAsync_ShouldReturnMappedLeaderboard_WithCorrectPositions()
        {
            // Arrange
            var teams = new List<TeamLeaderboardDto>
            {
                new TeamLeaderboardDto { TeamName = "Alpha", CurrentPoints = 50, RankName = "Gold" },
                new TeamLeaderboardDto { TeamName = "Bravo", CurrentPoints = 40, RankName = "Silver" },
                new TeamLeaderboardDto { TeamName = "Charlie", CurrentPoints = 30, RankName = "Bronze" }
            };

            _teamRepositoryMock
                .Setup(r => r.GetTopTeamsAsync(It.IsAny<int>()))
                .ReturnsAsync(teams);

            // Act
            var result = await _service.GetLeaderboardAsync();

            // Assert 
            result.Should().NotBeNull("o resultado não deve ser null.");
            result.Should().HaveCount(3, "devem ser retornadas as 3 equipas fornecidas pelo repositório");

            result[0].Position.Should().Be(1);
            result[0].TeamName.Should().Be("Alpha");
            result[0].CurrentPoints.Should().Be(50);
            result[0].RankName.Should().Be("Gold");

            result[1].Position.Should().Be(2);
            result[1].TeamName.Should().Be("Bravo");
            result[1].RankName.Should().Be("Silver");

            result[2].Position.Should().Be(3);
            result[2].TeamName.Should().Be("Charlie");
            result[2].RankName.Should().Be("Bronze");
        }

        [Test]
        public async Task GetLeaderboardAsync_ShouldCallRepositoryWithTop100()
        {
            // Arrange
            int? capturedTop = null;
            _teamRepositoryMock
                .Setup(r => r.GetTopTeamsAsync(It.IsAny<int>()))
                .Callback<int>(t => capturedTop = t)
                .ReturnsAsync(new List<TeamLeaderboardDto>());

            // Act
            await _service.GetLeaderboardAsync();

            // Assert 
            capturedTop.Should().HaveValue("GetTopTeamsAsync deveria ter sido invocado e capturado o parâmetro top");
            capturedTop.Value.Should().Be(100, "o service chama o repositório com top = 100");
        }

        [Test]
        public async Task GetLeaderboardAsync_WhenRepositoryReturnsEmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            _teamRepositoryMock
                .Setup(r => r.GetTopTeamsAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<TeamLeaderboardDto>());

            // Act
            var result = await _service.GetLeaderboardAsync();

            //Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty("quando o repositório devolve lista vazia, o serviço deve devolver lista vazia");
        }
        #endregion
    }
}
