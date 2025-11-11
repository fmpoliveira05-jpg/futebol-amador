using Application.Interfaces.Services;
using Application.Interfaces.Validators.Hub;
using Google.Cloud.Firestore;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Reflection;

namespace Tests.Integration
{
    public class ApiTestAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptorsToRemove = services
                    .Where(d =>
                        d.ServiceType == typeof(IDbContextOptionsConfiguration<AmateurFootballContext>) ||
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

                RemoveFirebaseServices(services);

                AddFirebaseServiceMocks(services);

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

        private void RemoveFirebaseServices(IServiceCollection services)
        {
            services.RemoveAll<FirestoreDb>();

            var firebaseServices = services
                .Where(s =>
                    (s.ServiceType.Namespace != null && (
                        s.ServiceType.Namespace.Contains("Firebase") ||
                        s.ServiceType.Namespace.Contains("Google.Cloud") ||
                        s.ServiceType.Namespace.Contains("Firestore"))) ||
                    (s.ImplementationType?.Namespace?.Contains("Firebase") == true) ||
                    (s.ImplementationType?.Namespace?.Contains("Google.Cloud") == true) ||
                    (s.ImplementationFactory?.Method.ReturnType.Namespace?.Contains("Firebase") == true) ||
                    (s.ImplementationFactory?.Method.ReturnType.Namespace?.Contains("Google.Cloud") == true))
                .ToList();

            foreach (var service in firebaseServices)
            {
                services.Remove(service);
            }

            var specificFirebaseServices = new List<Type>
            {
                typeof(IChatRoomService),
                typeof(IAuthService),
                typeof(ISuperAdminService)
            };

            foreach (var serviceType in specificFirebaseServices)
            {
                var descriptors = services.Where(d => d.ServiceType == serviceType).ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }
            }
        }

        private void AddFirebaseServiceMocks(IServiceCollection services)
        {
            var mockAuthService = new Mock<IAuthService>();
            mockAuthService.Setup(x => x.RegisterUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Guid.NewGuid().ToString());
            mockAuthService.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Application.DTOs.LoginResponseDto());
            mockAuthService.Setup(x => x.DeleteUserAsync(It.IsAny<string>())).Verifiable();
                
            mockAuthService.Setup(x => x.UpdateEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            

            services.AddSingleton(mockAuthService.Object);

            var mockChatService = new Mock<IChatRoomService>();
            services.AddSingleton(mockChatService.Object);

            var mockSuperAdminService = new Mock<ISuperAdminService>();
            services.AddSingleton(mockSuperAdminService.Object);
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