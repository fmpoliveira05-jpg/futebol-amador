using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.MatchInvites;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Net.Http.Json;
using Tests.Integration;

namespace Tests.Integration.MatchInvite
{
    [TestFixture]
    public class MatchInviteIntegrationTests
    {
        private ApiTestAppFactory _factory = null!;
        private HttpClient _client;

        [SetUp]
        public void Setup()
        {
            _factory = new ApiTestAppFactory();
            _client = _factory.CreateClient(new()
            {
                BaseAddress = new Uri("http://localhost")
            });
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task SendMatchInvite_Returns_Ok_With_InfoMatchInviteDto()
        {
            // Arrange
            var idTeam = Guid.NewGuid();
            var dto = new SendMatchInviteDto
            {
                IdReceiver = Guid.NewGuid(),
                GameDate = DateTime.Now.AddDays(1),
                namePitch = "Pitch A"
            };

            var expectedResponse = new InfoMatchInviteDto
            {
                Id = Guid.NewGuid(),
                IdSender = idTeam,
                NameSender = "Sender Team",
                IdReceiver = dto.IdReceiver,
                NameReceiver = "Receiver Team",
                GameDate = dto.GameDate,
                NamePitch = dto.namePitch
            };

            var mockMatchInviteService = new Mock<IMatchInviteService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockMatchInviteService
    .Setup(s => s.SendMatchInvite(idTeam, It.IsAny<SendMatchInviteDto>()))
    .ReturnsAsync(expectedResponse);


            mockAuthorizationService.Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idTeam))
                .Returns(Task.CompletedTask);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing registrations first
                    var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IMatchInviteService));
                    if (serviceDescriptor != null) services.Remove(serviceDescriptor);

                    var authDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IPlayerAuthorizationService));
                    if (authDescriptor != null) services.Remove(authDescriptor);

                    services.AddSingleton(mockMatchInviteService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            // Add authentication header
            client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await client.PostAsJsonAsync($"/api/MatchInvite/{idTeam}/match-invites", dto);

            // Debug: Check response content
            var responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseContent))
            {
                Assert.Fail("Response content is empty");
            }

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<InfoMatchInviteDto>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(expectedResponse.Id));
            Assert.That(result.IdSender, Is.EqualTo(expectedResponse.IdSender));
            Assert.That(result.NameSender, Is.EqualTo(expectedResponse.NameSender));
            Assert.That(result.IdReceiver, Is.EqualTo(expectedResponse.IdReceiver));
            Assert.That(result.NameReceiver, Is.EqualTo(expectedResponse.NameReceiver));
            Assert.That(result.GameDate, Is.EqualTo(expectedResponse.GameDate));
            Assert.That(result.NamePitch, Is.EqualTo(expectedResponse.NamePitch));

            mockMatchInviteService.Verify(s => s.SendMatchInvite(idTeam, It.IsAny<SendMatchInviteDto>()), Times.Once);
            mockAuthorizationService.Verify(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idTeam), Times.Once);
        }

        [Test]
        public async Task AcceptMatchInvite_Returns_Ok_With_MatchDto()
        {
            // Arrange
            var idTeam = Guid.NewGuid();
            var idMatchInvite = Guid.NewGuid();
            var expectedMatchDto = new MatchDto
            {
                IdMatch = Guid.NewGuid(),
                GameDate = DateTime.Now.AddDays(1),
                NameTeam = "Team A",
                NameOpponent = "Team B",
                NamePitch = "Pitch A"
            };

            var mockMatchInviteService = new Mock<IMatchInviteService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockMatchInviteService.Setup(s => s.AcceptMatchInvite(idTeam, idMatchInvite))
                .ReturnsAsync(expectedMatchDto);

            mockAuthorizationService.Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idTeam))
                .Returns(Task.CompletedTask);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IMatchInviteService));
                    if (serviceDescriptor != null) services.Remove(serviceDescriptor);

                    var authDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IPlayerAuthorizationService));
                    if (authDescriptor != null) services.Remove(authDescriptor);

                    services.AddSingleton(mockMatchInviteService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await client.PostAsJsonAsync($"/api/MatchInvite/{idTeam}/AcceptMatchInvite", idMatchInvite);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<MatchDto>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IdMatch, Is.EqualTo(expectedMatchDto.IdMatch));
            Assert.That(result.GameDate, Is.EqualTo(expectedMatchDto.GameDate));
            Assert.That(result.NameTeam, Is.EqualTo(expectedMatchDto.NameTeam));
            Assert.That(result.NameOpponent, Is.EqualTo(expectedMatchDto.NameOpponent));
            Assert.That(result.NamePitch, Is.EqualTo(expectedMatchDto.NamePitch));

            mockMatchInviteService.Verify(s => s.AcceptMatchInvite(idTeam, idMatchInvite), Times.Once);
            mockAuthorizationService.Verify(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idTeam), Times.Once);
        }

        [Test]
        public async Task RefuseMatchInvite_Returns_Ok()
        {
            // Arrange
            var idTeam = Guid.NewGuid();
            var idMatchInvite = Guid.NewGuid();

            var mockMatchInviteService = new Mock<IMatchInviteService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockMatchInviteService.Setup(s => s.RefuseMatchInvites(idTeam, idMatchInvite))
                .Returns(Task.CompletedTask);

            mockAuthorizationService.Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idTeam))
                .Returns(Task.CompletedTask);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IMatchInviteService));
                    if (serviceDescriptor != null) services.Remove(serviceDescriptor);

                    var authDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IPlayerAuthorizationService));
                    if (authDescriptor != null) services.Remove(authDescriptor);

                    services.AddSingleton(mockMatchInviteService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/MatchInvite/{idTeam}/RefuseMatchInvite")
            {
                Content = JsonContent.Create(idMatchInvite)
            };
            var response = await client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            mockMatchInviteService.Verify(s => s.RefuseMatchInvites(idTeam, idMatchInvite), Times.Once);
            mockAuthorizationService.Verify(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idTeam), Times.Once);
        }

        [Test]
        public async Task NegociateMatchInvite_Returns_Ok_With_InfoMatchInviteDto()
        {
            // Arrange
            var idTeam = Guid.NewGuid();
            var dto = new SendMatchInviteDto
            {
                IdReceiver = Guid.NewGuid(),
                GameDate = DateTime.Now.AddDays(1),
                namePitch = "Pitch A"
            };

            var expectedResponse = new InfoMatchInviteDto
            {
                Id = Guid.NewGuid(),
                IdSender = idTeam,
                NameSender = "Sender Team",
                IdReceiver = dto.IdReceiver,
                NameReceiver = "Receiver Team",
                GameDate = dto.GameDate,
                NamePitch = dto.namePitch
            };

            var mockMatchInviteService = new Mock<IMatchInviteService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockMatchInviteService
                .Setup(s => s.NegociateMatchInvite(idTeam, It.IsAny<SendMatchInviteDto>()))
                .ReturnsAsync(expectedResponse);


            mockAuthorizationService.Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idTeam))
                .Returns(Task.CompletedTask);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IMatchInviteService));
                    if (serviceDescriptor != null) services.Remove(serviceDescriptor);

                    var authDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IPlayerAuthorizationService));
                    if (authDescriptor != null) services.Remove(authDescriptor);

                    services.AddSingleton(mockMatchInviteService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await client.PutAsJsonAsync($"/api/MatchInvite/{idTeam}/Negociate", dto);

            // Debug: Check response content
            var responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseContent))
            {
                Assert.Fail("Response content is empty");
            }

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<InfoMatchInviteDto>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(expectedResponse.Id));
            Assert.That(result.IdSender, Is.EqualTo(expectedResponse.IdSender));
            Assert.That(result.NameSender, Is.EqualTo(expectedResponse.NameSender));
            Assert.That(result.IdReceiver, Is.EqualTo(expectedResponse.IdReceiver));
            Assert.That(result.NameReceiver, Is.EqualTo(expectedResponse.NameReceiver));
            Assert.That(result.GameDate, Is.EqualTo(expectedResponse.GameDate));
            Assert.That(result.NamePitch, Is.EqualTo(expectedResponse.NamePitch));

            mockMatchInviteService.Verify(s => s.NegociateMatchInvite(idTeam, It.IsAny<SendMatchInviteDto>()), Times.Once);
            mockAuthorizationService.Verify(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idTeam), Times.Once);
        }

        [Test]
        public async Task GetAllMatchInvitesTeam_WithoutFilters_Returns_Ok_With_ListOfInfoMatchInviteDto()
        {
            // Arrange
            var idTeam = Guid.NewGuid();
            var expectedList = new List<InfoMatchInviteDto>
            {
                new InfoMatchInviteDto
                {
                    Id = Guid.NewGuid(),
                    IdSender = Guid.NewGuid(),
                    NameSender = "Sender 1",
                    IdReceiver = idTeam,
                    NameReceiver = "Receiver Team",
                    GameDate = DateTime.Now.AddDays(1),
                    NamePitch = "Pitch A"
                },
                new InfoMatchInviteDto
                {
                    Id = Guid.NewGuid(),
                    IdSender = Guid.NewGuid(),
                    NameSender = "Sender 2",
                    IdReceiver = idTeam,
                    NameReceiver = "Receiver Team",
                    GameDate = DateTime.Now.AddDays(2),
                    NamePitch = "Pitch B"
                }
            };

            var mockMatchInviteService = new Mock<IMatchInviteService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockMatchInviteService.Setup(s => s.GetAllMatchInvitesTeam(idTeam))
                .ReturnsAsync(expectedList);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IMatchInviteService));
                    if (serviceDescriptor != null) services.Remove(serviceDescriptor);

                    var authDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IPlayerAuthorizationService));
                    if (authDescriptor != null) services.Remove(authDescriptor);

                    services.AddSingleton(mockMatchInviteService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await client.GetAsync($"/api/MatchInvite/{idTeam}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<InfoMatchInviteDto>>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(expectedList[0].Id));
            Assert.That(result[1].Id, Is.EqualTo(expectedList[1].Id));

            mockMatchInviteService.Verify(s => s.GetAllMatchInvitesTeam(idTeam), Times.Once);
        }

        [Test]
        public async Task GetAllMatchInvitesTeam_WithFilters_Returns_Ok_With_ListOfInfoMatchInviteDto()
        {
            // Arrange
            var idTeam = Guid.NewGuid();
            var filter = new FilterMatchInvitesDto
            {
                SenderName = "Sender",
                MinDate = DateOnly.FromDateTime(DateTime.Now),
                MaxDate = DateOnly.FromDateTime(DateTime.Now).AddDays(7)
            };

            var expectedList = new List<InfoMatchInviteDto>
            {
                new InfoMatchInviteDto
                {
                    Id = Guid.NewGuid(),
                    IdSender = Guid.NewGuid(),
                    NameSender = "Sender 1",
                    IdReceiver = idTeam,
                    NameReceiver = "Receiver Team",
                    GameDate = DateTime.Now.AddDays(1),
                    NamePitch = "Pitch A"
                }
            };

            var mockMatchInviteService = new Mock<IMatchInviteService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockMatchInviteService.Setup(s => s.GetAllMatchInvitesTeamWithFilters(idTeam, It.IsAny<FilterMatchInvitesDto>()))
                .ReturnsAsync(expectedList);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IMatchInviteService));
                    if (serviceDescriptor != null) services.Remove(serviceDescriptor);

                    var authDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IPlayerAuthorizationService));
                    if (authDescriptor != null) services.Remove(authDescriptor);

                    services.AddSingleton(mockMatchInviteService.Object);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act - Use proper query string formatting
            var queryString = $"?SenderName={Uri.EscapeDataString(filter.SenderName)}&MinDate={filter.MinDate:yyyy-MM-dd}&MaxDate={filter.MaxDate:yyyy-MM-dd}";
            var response = await client.GetAsync($"/api/MatchInvite/{idTeam}{queryString}");

            // Debug: Check response content
            var responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseContent))
            {
                Assert.Fail("Response content is empty");
            }

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<InfoMatchInviteDto>>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(expectedList[0].Id));

            mockMatchInviteService.Verify(s => s.GetAllMatchInvitesTeamWithFilters(idTeam, It.IsAny<FilterMatchInvitesDto>()), Times.Once);
        }
    }
}