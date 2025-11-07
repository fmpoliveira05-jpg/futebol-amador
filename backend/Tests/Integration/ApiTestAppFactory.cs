using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration
{
    public class ApiTestAppFactory: WebApplicationFactory<Program>
    {
        public string TestUserId { get; private set; } = "fake-user-id-" + Guid.NewGuid();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AmateurFootballContext>)); 

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                services.AddDbContext<AmateurFootballContext>(options => 
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                var authDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IAuthenticationService));

                if (authDescriptor != null)
                {
                    services.Remove(authDescriptor);
                }

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
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
