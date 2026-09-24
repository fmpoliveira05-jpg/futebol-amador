using Api.BackGroundServices;
using Application.Services.BackGroundServices;

namespace Api.Extensions
{
    /// <summary>
    /// Classe estática de extensão responsável pela configuração e registo de dependências exclusivas da camada de API (Presentation Layer).
    /// 
    /// Serve para organizar a injeção de dependências, separando a lógica de registo de Background Services
    /// da configuração principal no <c>Program.cs</c>.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Regista os serviços de plano de fundo (Background Services / Hosted Services) no contentor de DI.
        /// </summary>
        /// <remarks>
        /// Este método utiliza <c>AddHostedService</c>, o que garante que:
        /// <list type="bullet">
        /// <item><description>Os serviços são registados como <b>Singletons</b>.</description></item>
        /// <item><description>O método <c>StartAsync</c> (ou <c>ExecuteAsync</c>) é chamado automaticamente quando a aplicação arranca.</description></item>
        /// <item><description>O método <c>StopAsync</c> é chamado quando a aplicação encerra graciosamente.</description></item>
        /// </list>
        /// 
        /// <b>Serviços Registados:</b>
        /// <list type="number">
        /// <item>
        ///     <term><see cref="RankMatchMakerBackGroundService"/></term>
        ///     <description>Responsável pelo algoritmo contínuo de emparelhamento de equipas em jogos competitivos.</description>
        /// </item>
        /// <item>
        ///     <term><see cref="NotificationBackGroundService"/></term>
        ///     <description>Responsável pela verificação periódica e envio de notificações (ex: "Dia de Jogo").</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="services">A coleção de serviços ([IServiceCollection]) onde os workers serão registados.</param>
        /// <returns>A própria coleção de serviços atualizada, permitindo encadeamento de chamadas (Fluent API).</returns>
        public static IServiceCollection AddApiBackGroundService(this IServiceCollection services)
        {
            services.AddHostedService<RankMatchMakerBackGroundService>();
            services.AddHostedService<NotificationBackGroundService>();

            return services;
        }
    }
}