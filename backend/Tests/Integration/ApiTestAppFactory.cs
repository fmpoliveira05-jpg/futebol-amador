using Application.Interfaces.Validators.Hub;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Reflection;

namespace Tests.Integration
{
    public class ApiTestAppFactory: WebApplicationFactory<Program>
    {
        public string TestUserId { get; private set; } = "fake-user-id-" + Guid.NewGuid();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptorsToRemove = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions<AmateurFootballContext>) ||
                        d.ServiceType == typeof(AmateurFootballContext) ||
                        (d.ImplementationType != null && d.ImplementationType == typeof(AmateurFootballContext)) ||
                        (d.ImplementationFactory != null && d.ImplementationFactory.GetMethodInfo().ReturnType == typeof(AmateurFootballContext))
                    )
                    .ToList();

                foreach (var d in descriptorsToRemove)
                {
                    services.Remove(d);
                }

                var hostedServices = services
                    .Where(d => d.ServiceType == typeof(IHostedService) ||
                                (d.ImplementationType != null && typeof(IHostedService).IsAssignableFrom(d.ImplementationType)))
                    .ToList();

                foreach (var h in hostedServices)
                {
                    services.Remove(h);
                }

                services.AddDbContext<AmateurFootballContext>(options => 
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                services.AddMemoryCache();

                var authDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(Microsoft.AspNetCore.Authentication.IAuthenticationService));

                if (authDescriptor != null)
                {
                    services.Remove(authDescriptor);
                }

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                var mockHubFinshValidator = new Mock<IHubFinshMatchValidator>();
                var mockGeralValidator = new Mock<IGeralHubValidator>();

                services.AddSingleton<IHubFinshMatchValidator>(mockHubFinshValidator.Object);
                services.AddSingleton<IGeralHubValidator>(mockGeralValidator.Object);
            });
        }

        public void SeedDatabase(Action<AmateurFootballContext> seeder) 
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AmateurFootballContext>(); 
            context.Database.EnsureCreated();
            seeder(context);
            context.SaveChanges();
        }
    }
}
