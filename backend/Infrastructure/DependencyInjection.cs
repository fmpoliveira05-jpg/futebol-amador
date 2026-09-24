using Application.Interfaces.Repositories;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    /// <summary>
    /// Classe estática que configura e regista todos os serviços da camada de Infraestrutura no Container de Injeção de Dependências (DI Container).
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Método de extensão principal para registar todos os serviços de Infraestrutura (Contexto DB e Repositórios).
        /// </summary>
        /// <param name="services">A coleção de serviços atual.</param>
        /// <param name="configuration">A configuração da aplicação (para Connection Strings).</param>
        /// <returns>A coleção de serviços atualizada.</returns>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext(configuration);
            services.AddRepositorys();

            return services;
        }

        /// <summary>
        /// Regista o contexto da base de dados ([AmateurFootballContext]) e configura o provedor SQL Server.
        /// </summary>
        /// <param name="services">A coleção de serviços.</param>
        /// <param name="configuration">Configuração da aplicação.</param>
        /// <returns>A coleção de serviços atualizada.</returns>
        private static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AmateurFootballContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }

        /// <summary>
        /// Regista todos os Repositórios e a Unidade de Trabalho (Unit of Work) no DI Container.
        /// 
        /// Os repositórios são registados com o escopo [AddScoped], o que significa que uma nova instância
        /// é criada por cada pedido HTTP (Request).
        /// </summary>
        /// <param name="services">A coleção de serviços.</param>
        /// <returns>A coleção de serviços atualizada.</returns>
        private static IServiceCollection AddRepositorys(this IServiceCollection services)
        {
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<IMatchInviteRepository, MatchInviteRepository>();
            services.AddScoped<IMatchRepository, MatchRepository>();
            services.AddScoped<ICancelledMatchRepository, CancelledMatchRepository>();
            services.AddScoped<IPitchRepository, PitchRepository>();
            services.AddScoped<ITeamStatisticsRepository, TeamStatisticsRepository>();
            services.AddScoped<IPlayerRepository, PlayerRepository>();
            services.AddScoped<ISuperAdminRepository, SuperAdminRepository>();
            services.AddScoped<ITeamPostPoneGameRepository, TeamPostPoneGame>();
            services.AddScoped<IRankRepository, RankRepository>();
            services.AddScoped<IUnityOfWork, UnityOfWork>();
            services.AddScoped<IMembershipRequestRepository, MembershipRequestRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}