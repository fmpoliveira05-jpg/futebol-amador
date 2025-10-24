using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Application.Services;
using Application.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependecyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IMatchInviteService, MatchInviteService>();
            services.AddScoped<IMatchService, MatchService>();
            services.AddScoped<ITeamValidator, TeamValidator>();
            services.AddScoped<IMatchInviteValidator, MatchInviteValidator>();
            services.AddScoped<IMatchValidator, MatchValidator>();
            services.AddScoped<IPlayerService, PlayerService>();
            return services;
        }
    }
}
