using Application.Interfaces.Repositories;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1. Registar o DbContext
            services.AddDbContext<AmateurFootballContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            //Registar os repositórios
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<IMatchInviteRepository, MatchInviteRepository>();
            services.AddScoped<IMatchRepository, MatchRepository>();
            services.AddScoped<IPitchRepository, PitchRepository>();
            services.AddScoped<ITeamStatisticsRepository, TeamStatisticsRepository>();
            services.AddScoped<IPlayerRepository, PlayerRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnityOfWork, UnityOfWork>();

            return services;
        }

    }
}
