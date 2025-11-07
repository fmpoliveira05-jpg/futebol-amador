using Application.Interfaces.Validators.Hub;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Tests.Integration.ClassTests.HubTests.NotificationTests
{
    public class NotificationHubIntegrationTests
    {
        #region Inicializer
        private ApiTestAppFactory _factory = null!;
        private WebApplicationFactory<Program> _appFactory = null!;
        private const string HubPath = "/Notification";
        #endregion

        #region SetUp
        [SetUp]
        public void SetUp()
        {
            _factory = new ApiTestAppFactory();
            _appFactory = _factory;
        }
        #endregion

        #region TearDown
        [TearDown]
        public void TearDown()
        {
            _appFactory?.Dispose();
            _factory?.Dispose();
        }
        #endregion

        #region Tests
        [Test(Description = "Se o validator lançar, JoinNotificationHub resulta em HubException.")]
        public async Task JoinNotificationHub_WhenValidatorThrows_ClientGetsHubException()
        {
            var mockValidator = new Mock<INotificationValidator>();
            mockValidator.Setup(v => v.ValidateTeamMembershipAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("not member"));

            var clientFactory = _appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockValidator.Object);
                });
            });

            var baseAddress = clientFactory.Server.BaseAddress!;
            var connection = new HubConnectionBuilder()
                .WithUrl(new Uri(baseAddress, HubPath), options =>
                {
                    options.HttpMessageHandlerFactory = _ => clientFactory.Server.CreateHandler();
                    options.Headers.Add("Authorization", "Test");
                })
                .Build();

            await connection.StartAsync();

            var idTeam = Guid.NewGuid();

            var ex = Assert.ThrowsAsync<Microsoft.AspNetCore.SignalR.HubException>(async () =>
            {
                await connection.InvokeAsync("JoinNotificationHub", idTeam);
            });

            Assert.That(ex, Is.Not.Null);

            await connection.StopAsync();
            await connection.DisposeAsync();
        }
        #endregion
    }
}
