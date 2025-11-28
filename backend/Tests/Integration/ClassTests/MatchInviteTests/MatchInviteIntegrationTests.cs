using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.MatchInvites;
using Application.DTOs.Team;
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
            var idSender = Guid.NewGuid();
            var idReceiver = Guid.NewGuid();
            var gameDate = DateTime.UtcNow.AddDays(1); 

            var dto = new SendMatchInviteDto
            {
                IdSender = idSender, 
                IdReceiver = idReceiver,
                GameDate = gameDate,
                homePitch = true 
            };

            var expectedResponse = new InfoMatchInviteDto
            {
                Id = Guid.NewGuid(),
                Sender = new TeamDto
                {
                    IdTeam = idSender,
                    Name = "Sender Team"
                },
                Receiver = new TeamDto
                {
                    IdTeam = idReceiver,
                    Name = "Receiver Team",
                },
                GameDate = gameDate,
                NamePitch = "Pitch A" 
            };

            var mockMatchInviteService = new Mock<IMatchInviteService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockMatchInviteService
                .Setup(s => s.SendMatchInvite(idSender, It.IsAny<SendMatchInviteDto>()))
                .ReturnsAsync(expectedResponse);

            mockAuthorizationService
                .Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idSender))
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
            var response = await client.PostAsJsonAsync($"/api/MatchInvite/{idSender}/match-invites", dto);

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
            Assert.That(result.Sender.IdTeam, Is.EqualTo(expectedResponse.Sender.IdTeam)); 
            Assert.That(result.NamePitch, Is.EqualTo(expectedResponse.NamePitch));

            mockMatchInviteService.Verify(s => s.SendMatchInvite(idSender, It.IsAny<SendMatchInviteDto>()), Times.Once);
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
            var idSender = Guid.NewGuid();
            var idReceiver = Guid.NewGuid();
            var dto = new SendMatchInviteDto
            {
                IdSender = idSender,
                IdReceiver = idReceiver,
                GameDate = DateTime.UtcNow.AddDays(1),
                homePitch = false
            };

            var expectedResponse = new InfoMatchInviteDto
            {
                Id = Guid.NewGuid(),
                Sender = new TeamDto { IdTeam = idSender, Name = "Sender" },
                Receiver = new TeamDto { IdTeam = idReceiver, Name = "Receiver" },
                GameDate = dto.GameDate,
                NamePitch = "Pitch B"
            };

            var mockMatchInviteService = new Mock<IMatchInviteService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockMatchInviteService
                .Setup(s => s.NegociateMatchInvite(idSender, It.IsAny<SendMatchInviteDto>()))
                .ReturnsAsync(expectedResponse);

            mockAuthorizationService
                .Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idSender))
                .Returns(Task.CompletedTask);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.Remove(services.First(d => d.ServiceType == typeof(IMatchInviteService)));
                    services.AddSingleton(mockMatchInviteService.Object);

                    services.Remove(services.First(d => d.ServiceType == typeof(IPlayerAuthorizationService)));
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            // Rota: [HttpPut("{idTeam}/Negociate")]
            var response = await client.PutAsJsonAsync($"/api/MatchInvite/{idSender}/Negociate", dto);

            // Assert
            if (!response.IsSuccessStatusCode) Assert.Fail(await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadFromJsonAsync<InfoMatchInviteDto>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.NamePitch, Is.EqualTo(expectedResponse.NamePitch));
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
                    Sender = new TeamDto
                    {
                        IdTeam = Guid.NewGuid(),
                        Name = "Sender 1"
                    },
                    Receiver = new TeamDto
                    {
                        IdTeam = idTeam,
                        Name = "Receiver Team"
                    },
                    GameDate = DateTime.UtcNow.AddDays(1),
                    NamePitch = "Pitch A"
                },
                new InfoMatchInviteDto
                {
                    Id = Guid.NewGuid(),
                    Sender = new TeamDto
                    {
                        IdTeam = Guid.NewGuid(),
                        Name = "Sender 2"
                    },
                    Receiver = new TeamDto
                    {
                        IdTeam = idTeam,
                        Name = "Receiver Team"
                    },
                    GameDate = DateTime.UtcNow.AddDays(2),
                    NamePitch = "Pitch B"
                }
            };

            var mockMatchInviteService = new Mock<IMatchInviteService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            mockMatchInviteService
                .Setup(s => s.GetAllMatchInvitesTeam(idTeam))
                .ReturnsAsync(expectedList);

            mockAuthorizationService
                .Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idTeam))
                .Returns(Task.CompletedTask);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remover serviços existentes para evitar conflitos
                    var matchService = services.FirstOrDefault(d => d.ServiceType == typeof(IMatchInviteService));
                    if (matchService != null) services.Remove(matchService);
                    services.AddSingleton(mockMatchInviteService.Object);

                    var authService = services.FirstOrDefault(d => d.ServiceType == typeof(IPlayerAuthorizationService));
                    if (authService != null) services.Remove(authService);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await client.GetAsync($"/api/MatchInvite/{idTeam}");

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Request failed: {response.StatusCode} - {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<List<InfoMatchInviteDto>>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));

            // Verificar IDs e propriedades aninhadas
            Assert.That(result[0].Id, Is.EqualTo(expectedList[0].Id));
            Assert.That(result[0].Sender.Name, Is.EqualTo(expectedList[0].Sender.Name));

            Assert.That(result[1].Id, Is.EqualTo(expectedList[1].Id));
            Assert.That(result[1].NamePitch, Is.EqualTo(expectedList[1].NamePitch));

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
                    Sender = new TeamDto
                    {
                        IdTeam = Guid.NewGuid(),
                        Name = "Sender 1"
                    },
                    Receiver = new TeamDto
                    {
                        IdTeam = idTeam,
                        Name = "Receiver Team"
                    },
                    GameDate = DateTime.UtcNow.AddDays(1),
                    NamePitch = "Pitch A"
                }            
            };

            var mockMatchInviteService = new Mock<IMatchInviteService>();
            var mockAuthorizationService = new Mock<IPlayerAuthorizationService>();

            // Configurar o mock para aceitar qualquer filtro (ou o filtro específico se preferires Match.Is)
            mockMatchInviteService
                .Setup(s => s.GetAllMatchInvitesTeamWithFilters(idTeam, It.IsAny<FilterMatchInvitesDto>()))
                .ReturnsAsync(expectedList);

            mockAuthorizationService
                .Setup(a => a.UserAuthorizationIsAdminTeamById(It.IsAny<string>(), idTeam))
                .Returns(Task.CompletedTask);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var matchService = services.FirstOrDefault(d => d.ServiceType == typeof(IMatchInviteService));
                    if (matchService != null) services.Remove(matchService);
                    services.AddSingleton(mockMatchInviteService.Object);

                    var authService = services.FirstOrDefault(d => d.ServiceType == typeof(IPlayerAuthorizationService));
                    if (authService != null) services.Remove(authService);
                    services.AddSingleton(mockAuthorizationService.Object);
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            // Construção correta da Query String
            var queryString = $"?SenderName={Uri.EscapeDataString(filter.SenderName)}&MinDate={filter.MinDate:yyyy-MM-dd}&MaxDate={filter.MaxDate:yyyy-MM-dd}";
            var response = await client.GetAsync($"/api/MatchInvite/{idTeam}{queryString}");

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Request failed: {response.StatusCode} - {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<List<InfoMatchInviteDto>>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(expectedList[0].Id));
            Assert.That(result[0].Sender.Name, Is.EqualTo(expectedList[0].Sender.Name));

            mockMatchInviteService.Verify(s => s.GetAllMatchInvitesTeamWithFilters(idTeam, It.IsAny<FilterMatchInvitesDto>()), Times.Once);
        }
    }
}