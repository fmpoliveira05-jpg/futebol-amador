using Application.DTOs.Chat;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration.ClassTests.ChatIntegrationTests
{
    [TestFixture]
    internal class ChatControllerIntegrationTests
    {
        private ApiTestAppFactory _factory = null;
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

        #region CreateChatRoom Tests

        [Test]
        public async Task CreateChatRoom_Returns_RoomId_When_Authenticated()
        {
            // Arrange
            var request = new CreateChatRoomDto
            {
                RoomName = "Test Room",
                MemberIds = new List<string> { "user1", "user2" }
            };
            var expectedRoomId = "test-room-id";

            var mockChatService = new Mock<IChatRoomService>();
            mockChatService.Setup(s => s.CreateRoomAsync(It.IsAny<CreateChatRoomDto>(), It.IsAny<string>()))
                .ReturnsAsync(expectedRoomId);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IChatRoomService));
                    services.AddSingleton<IChatRoomService>(mockChatService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.PostAsJsonAsync("/api/Chat/create-room", request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.That(result, Is.Not.Null);

            mockChatService.Verify(s => s.CreateRoomAsync(It.IsAny<CreateChatRoomDto>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task CreateChatRoom_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var request = new CreateChatRoomDto
            {
                RoomName = "Test Room",
                MemberIds = new List<string> { "user1", "user2" }
            };

            var mockChatService = new Mock<IChatRoomService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IChatRoomService));
                    services.AddSingleton<IChatRoomService>(mockChatService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.PostAsJsonAsync("/api/Chat/create-room", request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task CreateChatRoom_Returns_BadRequest_When_InvalidModel()
        {
            // Arrange
            var invalidRequest = new CreateChatRoomDto
            {
                RoomName = "", // Nome vazio - inválido
                MemberIds = null // Lista nula - inválida
            };

            var mockChatService = new Mock<IChatRoomService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IChatRoomService));
                    services.AddSingleton<IChatRoomService>(mockChatService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.PostAsJsonAsync("/api/Chat/create-room", invalidRequest);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        #endregion

        #region GetMyRooms Tests

        [Test]
        public async Task GetMyRooms_Returns_Rooms_When_Authenticated()
        {
            // Arrange
            var expectedRooms = new List<ChatRoomDto>
            {
                new ChatRoomDto
                {
                    RoomId = "room1",
                    RoomName = "Room 1",
                    MemberIds = new List<string> { "user1", "user2" }
                },
                new ChatRoomDto
                {
                    RoomId = "room2",
                    RoomName = "Room 2",
                    MemberIds = new List<string> { "user1", "user3" }
                }
            };

            var mockChatService = new Mock<IChatRoomService>();
            mockChatService.Setup(s => s.GetMyRoomsAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedRooms);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IChatRoomService));
                    services.AddSingleton<IChatRoomService>(mockChatService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.GetAsync("/api/Chat/my-rooms");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<List<ChatRoomDto>>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].RoomId, Is.EqualTo("room1"));
                Assert.That(result[0].RoomName, Is.EqualTo("Room 1"));
                Assert.That(result[1].RoomId, Is.EqualTo("room2"));
                Assert.That(result[1].RoomName, Is.EqualTo("Room 2"));
            });

            mockChatService.Verify(s => s.GetMyRoomsAsync(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task GetMyRooms_Returns_EmptyList_When_NoRooms()
        {
            // Arrange
            var expectedRooms = new List<ChatRoomDto>();

            var mockChatService = new Mock<IChatRoomService>();
            mockChatService.Setup(s => s.GetMyRoomsAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedRooms);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IChatRoomService));
                    services.AddSingleton<IChatRoomService>(mockChatService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.GetAsync("/api/Chat/my-rooms");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<List<ChatRoomDto>>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));

            mockChatService.Verify(s => s.GetMyRoomsAsync(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task GetMyRooms_Returns_Unauthorized_When_NotAuthenticated()
        {
            // Arrange
            var mockChatService = new Mock<IChatRoomService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IChatRoomService));
                    services.AddSingleton<IChatRoomService>(mockChatService.Object);
                });
            }).CreateClient();

            // Não adicionar header de autorização

            // Act
            var response = await _client.GetAsync("/api/Chat/my-rooms");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        #endregion

        #region Edge Cases
        [Test]
        public async Task GetMyRooms_ServiceThrowsException_Returns_InternalServerError()
        {
            // Arrange
            var mockChatService = new Mock<IChatRoomService>();
            mockChatService.Setup(s => s.GetMyRoomsAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IChatRoomService));
                    services.AddSingleton<IChatRoomService>(mockChatService.Object);
                });
            }).CreateClient();

            _client.DefaultRequestHeaders.Add("Authorization", "Test");

            // Act
            var response = await _client.GetAsync("/api/Chat/my-rooms");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }

        #endregion
    }
}