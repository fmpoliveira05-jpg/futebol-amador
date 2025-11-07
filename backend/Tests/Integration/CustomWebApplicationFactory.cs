using Api;
using Application.Interfaces.Services;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Api.IntegrationTests.Fixtures
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IMatchService> MatchServiceMock { get; } = new();
        public Mock<IPlayerAuthorizationService> AuthorizationServiceMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                var testConfig = new Dictionary<string, string?>
                {
                    ["Firebase:ProjectId"] = "fake-firebase-project",
                    ["Firebase:ApiKey"] = "fake-api-key",
                    ["Firebase:AuthDomain"] = "fake.firebaseapp.com"
                };

                configBuilder.AddInMemoryCollection(testConfig);
            });

            builder.ConfigureServices(services =>
            {
                var matchServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IMatchService));
                if (matchServiceDescriptor != null)
                    services.Remove(matchServiceDescriptor);

                var authServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IPlayerAuthorizationService));
                if (authServiceDescriptor != null)
                    services.Remove(authServiceDescriptor);

                var firestoreDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(FirestoreDb));
                if (firestoreDescriptor != null)
                    services.Remove(firestoreDescriptor);

                services.AddSingleton(MatchServiceMock.Object);
                services.AddSingleton(AuthorizationServiceMock.Object);
                services.AddSingleton(_ => Mock.Of<FirestoreDb>());

                services.AddAuthentication("FakeJwt")
                        .AddScheme<AuthenticationSchemeOptions, FakeJwtAuthHandler>(
                            "FakeJwt", options => { });

                var hostedServices = services
                    .Where(s => typeof(IHostedService).IsAssignableFrom(s.ServiceType))
                    .ToList();

                foreach (var svc in hostedServices)
                    services.Remove(svc);
            });

            builder.Configure(app =>
            {
                app.UseAuthentication();
                app.UseAuthorization();
            });
        }
    }
}