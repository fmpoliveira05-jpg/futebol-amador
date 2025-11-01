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
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext(configuration);
            services.AddRepositorys();

            return services;
        }

        private static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddDbContext<AmateurFootballContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }

        private static IServiceCollection AddRepositorys(this IServiceCollection services) 
        {
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<IMatchInviteRepository, MatchInviteRepository>();
            services.AddScoped<IMatchRepository, MatchRepository>();
            services.AddScoped<ICancelledMatchRepository, CancelledMatchRepository>();
            services.AddScoped<IPitchRepository, PitchRepository>();
            services.AddScoped<ITeamStatisticsRepository, TeamStatisticsRepository>();
            services.AddScoped<IPlayerRepository, PlayerRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITeamPostPoneGameRepository, TeamPostPoneGame>();
            services.AddScoped<IRankRepository, RankRepository>();
            services.AddScoped<IUnityOfWork, UnityOfWork>();
            services.AddScoped<IMembershipRequestRepository, MembershipRequestRepository>();

            return services;
        } 
    }
}