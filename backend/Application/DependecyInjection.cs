using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators;
using Application.Interfaces.Validators.Hub;
using Application.Services;
using Application.Services.Hub;
using Application.Validators;
using Application.Validators.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependecyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddBusinessServices();
            services.AddBusinessValidators();

            return services;
        }

        private static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IMatchInviteService, MatchInviteService>();
            services.AddScoped<IMatchService, MatchService>();
            services.AddScoped<IMembershipRequestService, MembershipService>();
            services.AddScoped<IManagerStartMatchService, ManagerStartMatchService>();
            services.AddScoped<IManagerFinishMatchService, ManagerFinishMatchService>();
            services.AddScoped<IPlayerService, PlayerService>();
            services.AddTransient<IStartMatchHubClientService, StartMatchHubClientService>();
            services.AddTransient<IFinishMatchHubClientService, FinishMatchHubClientService>();

            return services;
        }

        private static IServiceCollection AddBusinessValidators(this IServiceCollection services)
        {
            services.AddScoped<ITeamValidator, TeamValidator>();
            services.AddScoped<IMatchInviteValidator, MatchInviteValidator>();
            services.AddScoped<IMatchValidator, MatchValidator>();
            services.AddScoped<IStartMatchHubValidator, StartMatchHubValidator>();
            services.AddScoped<IFinishMatchValidator, FinishMatchValidator>();
            services.AddScoped<IGeralHubValidator, GeralHubValidator>();
            services.AddScoped<IPlayerService, PlayerService>();
            services.AddScoped<IPlayerValidator, PlayerValidator>();
            return services;
        }
    }
}
