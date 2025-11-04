using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Services.Hub.ClienteService;
using Application.Interfaces.Validators;
using Application.Interfaces.Validators.Hub;
using Application.Services;
using Application.Services.Hub;
using Application.Services.Hub.ClientService;
using Application.Validators;
using Application.Validators.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependecyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddServices();
            services.AddValidators();

            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IMatchInviteService, MatchInviteService>();
            services.AddScoped<IMatchService, MatchService>();
            services.AddScoped<IPlayerService, PlayerService>();
            services.AddScoped<IMatchMakerService, MatchMakerService>();
            services.AddScoped<ISuperAdminService, SuperAdminService>();
            services.AddScoped<IChatRoomService, FirebaseChatService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();

            services.AddManagerHubService();
            services.AddHubServiceClients();

            return services;
        }

        private static IServiceCollection AddHubServiceClients(this IServiceCollection services)
        {
            services.AddTransient<IStartMatchHubClientService, StartMatchHubClientService>();
            services.AddTransient<IFinishMatchHubClientService, FinishMatchHubClientService>();
            services.AddTransient<IRankMatchMakerHubClientService, RankMatchMakerHubClientService>();

            return services;
        }

        private static IServiceCollection AddManagerHubService(this IServiceCollection services)
        {
            services.AddScoped<IManagerStartMatchService, ManagerStartMatchService>();
            services.AddScoped<IManagerFinishMatchService, ManagerFinishMatchService>();
            services.AddScoped<IManagerRankMatchMakerService, ManagerRankMatchMakerService>(); 

            return services;
        }

        private static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddScoped<IPlayerValidator, PlayerValidator>();
            services.AddScoped<ITeamValidator, TeamValidator>();
            services.AddScoped<IMatchInviteValidator, MatchInviteValidator>();
            services.AddScoped<ICalendarValidator, CalendarValidator>();
            services.AddScoped<IStartMatchHubValidator, StartMatchHubValidator>();
            services.AddScoped<IFinishMatchValidator, FinishMatchValidator>();
            services.AddScoped<IGeralHubValidator, GeralHubValidator>();
            services.AddScoped<IRankMatchMakerValidator, RankMatchMakerValidator>();
            services.AddScoped<ISuperAdminValidator, SuperAdminValidator>();
            services.AddScoped<IEmailValidator, EmailValidator>();
            services.AddScoped<IAuthorizationValidator, AuthorizationValidator>();

            return services;
        }

    }
}