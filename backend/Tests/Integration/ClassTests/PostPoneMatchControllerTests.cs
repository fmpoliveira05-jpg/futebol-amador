using Api.IntegrationTests.Fixtures;
using Api.IntegrationTests.Helpers;
using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace Api.IntegrationTests.Controllers
{
    [TestFixture]
    public class PostPoneMatchControllerTests
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public void Setup()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task GET_ListPostPoneMatchTeam_ReturnsOk()
        {
            var idTeam = Guid.NewGuid();
            var expected = new List<InfoPostPoneMatch>
            {
                new() { IdMatch = Guid.NewGuid(), IdTeam = idTeam, IdOpponent = Guid.NewGuid(), nameTeam = "Team A", nameOpponent = "Team B" }
            };

            _factory.MatchServiceMock
                .Setup(s => s.GetListPostPoneMatchTeam(idTeam))
                .ReturnsAsync(expected);

            _factory.AuthorizationServiceMock
                .Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idTeam))
                .Returns(Task.CompletedTask);

            var response = await _client.GetAsync($"/api/Team/{idTeam}/PostPoneMatch");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.ReadResponseAsync<List<InfoPostPoneMatch>>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result![0].nameTeam, Is.EqualTo("Team A"));
        }

        [Test]
        public async Task POST_AcceptPostponeMatch_ReturnsOk()
        {
            var idTeam = Guid.NewGuid();
            var dto = new AcceptRefusePostPoneDto { IdMatch = Guid.NewGuid(), IdOpponent = Guid.NewGuid() };

            var expected = new MatchDto
            {
                IdMatch = dto.IdMatch,
                NameTeam = "Team A",
                NameOpponent = "Team B"
            };

            _factory.AuthorizationServiceMock
                .Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idTeam))
                .Returns(Task.CompletedTask);

            _factory.MatchServiceMock
                .Setup(m => m.AcceptPostPoneMatch(idTeam, dto))
                .ReturnsAsync(expected);

            var response = await _client.PostAsJsonAsync($"/api/Team/{idTeam}/PostPoneMatch/AcceptPostponeMatch", dto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.ReadResponseAsync<MatchDto>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.NameTeam, Is.EqualTo("Team A"));
            Assert.That(result.NameOpponent, Is.EqualTo("Team B"));
        }

        [Test]
        public async Task DELETE_RejectPostponeMatch_ReturnsOk()
        {
            var idTeam = Guid.NewGuid();
            var dto = new AcceptRefusePostPoneDto { IdMatch = Guid.NewGuid(), IdOpponent = Guid.NewGuid() };

            _factory.AuthorizationServiceMock
                .Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idTeam))
                .Returns(Task.CompletedTask);

            _factory.MatchServiceMock
                .Setup(m => m.RejectPostPoneMatch(idTeam, dto))
                .Returns(Task.CompletedTask);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/Team/{idTeam}/PostPoneMatch/RejectPostponeMatch")
            {
                Content = JsonContent.Create(dto)
            };

            var response = await _client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GET_ListPostPoneMatchTeam_WithFilters_ReturnsFilteredResult()
        {
            var idTeam = Guid.NewGuid();
            var filter = new FilterPostPoneMatchDto { NameOpponent = "OpponentX" };
            var expected = new List<InfoPostPoneMatch>
            {
                new() { IdMatch = Guid.NewGuid(), nameOpponent = "OpponentX" }
            };

            _factory.AuthorizationServiceMock
                .Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idTeam))
                .Returns(Task.CompletedTask);

            _factory.MatchServiceMock
                .Setup(s => s.GetListPostPoneMatchTeamWithFilters(idTeam, It.IsAny<FilterPostPoneMatchDto>()))
                .ReturnsAsync(expected);

            var response = await _client.GetAsync($"/api/Team/{idTeam}/PostPoneMatch?NameOpponent=OpponentX");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.ReadResponseAsync<List<InfoPostPoneMatch>>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result![0].nameOpponent, Is.EqualTo("OpponentX"));
        }
    }
}