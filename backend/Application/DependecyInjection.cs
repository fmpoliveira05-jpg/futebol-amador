using Application.Interfaces;
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
    /// <summary>
    /// Classe estática de extensão responsável pela configuração e registo de dependências da camada de Aplicação.
    /// 
    /// Centraliza a injeção de todos os Serviços de Domínio, Validadores e Gestores de Hubs SignalR,
    /// garantindo que a lógica de negócio está desacoplada e pronta a ser utilizada pelos controladores ou outros serviços.
    /// </summary>
    public static class DependecyInjection
    {
        /// <summary>
        /// Método de extensão principal para registar todos os serviços da camada de Aplicação no contentor DI.
        /// </summary>
        /// <remarks>
        /// Este método orquestra a chamada de métodos privados auxiliares para registar categorias específicas de dependências:
        /// <list type="bullet">
        /// <item><description><see cref="AddServices"/>: Serviços de lógica de negócio.</description></item>
        /// <item><description><see cref="AddValidators"/>: Validadores de dados e regras de negócio.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="services">A coleção de serviços ([IServiceCollection]) onde as dependências serão registadas.</param>
        /// <returns>A coleção de serviços atualizada para permitir encadeamento (Fluent API).</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddServices();
            services.AddValidators();

            return services;
        }

        /// <summary>
        /// Regista os serviços de domínio e lógica de negócio.
        /// </summary>
        /// <param name="services">A coleção de serviços.</param>
        /// <returns>A coleção de serviços atualizada.</returns>
        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IMatchInviteService, MatchInviteService>();
            services.AddScoped<IMatchService, MatchService>();
            services.AddScoped<IPlayerService, PlayerService>();
            services.AddScoped<IMatchMakerService, MatchMakerService>();
            services.AddScoped<ISuperAdminService, SuperAdminService>();
            services.AddScoped<IMembershipRequestService, MembershipService>();
            services.AddScoped<IChatRoomService, FirebaseChatService>();
            services.AddScoped<IPlayerAuthorizationService, PlayerAuthorizationService>();
            services.AddScoped<IAuthService, FireBaseAuthService>();
            services.AddScoped<INotificationFirebaseService, NotificationFirebaseService>();

            services.AddManagerHubService();
            services.AddHubServiceClients();

            return services;
        }

        /// <summary>
        /// Regista os serviços clientes dos Hubs SignalR (Hub Client Services).
        /// </summary>
        /// <remarks>
        /// Estes serviços são registados como <c>Transient</c> porque geralmente mantêm estado leve ou são instanciados per-call
        /// para interagir diretamente com o contexto do Hub.
        /// </remarks>
        /// <param name="services">A coleção de serviços.</param>
        /// <returns>A coleção de serviços atualizada.</returns>
        private static IServiceCollection AddHubServiceClients(this IServiceCollection services)
        {
            services.AddTransient<IStartMatchHubClientService, StartMatchHubClientService>();
            services.AddTransient<IFinishMatchHubClientService, FinishMatchHubClientService>();
            services.AddTransient<IRankMatchMakerHubClientService, RankMatchMakerHubClientService>();

            return services;
        }

        /// <summary>
        /// Regista os serviços de gestão de estado dos Hubs (Manager Hub Services).
        /// </summary>
        /// <remarks>
        /// Estes serviços contêm a lógica pesada de gestão de salas, lobbies e sincronização de estado (ex: quem está pronto para começar o jogo).
        /// </remarks>
        /// <param name="services">A coleção de serviços.</param>
        /// <returns>A coleção de serviços atualizada.</returns>
        private static IServiceCollection AddManagerHubService(this IServiceCollection services)
        {
            services.AddScoped<IManagerStartMatchService, ManagerStartMatchService>();
            services.AddScoped<IManagerFinishMatchService, ManagerFinishMatchService>();
            services.AddScoped<IManagerRankMatchMakerService, ManagerRankMatchMakerService>(); 

            return services;
        }

        /// <summary>
        /// Regista todos os validadores da aplicação.
        /// </summary>
        /// <remarks>
        /// Inclui validadores de entidades (Player, Team), validadores de lógica de negócio (MatchInvite)
        /// e validadores específicos para operações de Hub em tempo real.
        /// </remarks>
        /// <param name="services">A coleção de serviços.</param>
        /// <returns>A coleção de serviços atualizada.</returns>
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
            services.AddScoped<IUserDataValidator, UserDataValidator>();
            services.AddScoped<IPlayerAuthorizationValidator, PlayerAuthorizationValidator>();
            services.AddScoped<IMembershipValidator, MembershipValidator>();
            return services;
        }
    }
}