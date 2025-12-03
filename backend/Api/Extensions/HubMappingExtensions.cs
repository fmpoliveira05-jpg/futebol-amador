using Api.Hubs;

namespace Api.Extensions
{
    /// <summary>
    /// Classe de extensão responsável pelo mapeamento e configuração das rotas (Endpoints) dos Hubs SignalR.
    /// 
    /// Esta classe centraliza a definição dos caminhos de URL para todos os serviços de tempo real,
    /// mantendo o ficheiro <c>Program.cs</c> limpo e organizado.
    /// </summary>
    public static class HubMappingExtensions
    {
        /// <summary>
        /// Mapeia os Hubs SignalR da aplicação para os seus respetivos endpoints HTTP.
        /// 
        /// Este método deve ser chamado no pipeline de configuração da aplicação (<c>app</c>) 
        /// após a construção do host.
        /// </summary>
        /// <remarks>
        /// <b>Tabela de Endpoints Configurados:</b>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Rota (URL)</term>
        ///         <description>Hub Associado</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>/StartMatch</c></term>
        ///         <description>Gere o handshake inicial e o começo do jogo em tempo real.</description>
        ///     </item>
        ///     <item>
        ///         <term><c>/FinishMatch</c></term>
        ///         <description>Coordena a submissão e validação de resultados finais entre as equipas.</description>
        ///     </item>
        ///     <item>
        ///         <term><c>/MatchMaker</c></term>
        ///         <description>Lobby para procura automática de adversários (Ranked).</description>
        ///     </item>
        ///     <item>
        ///         <term><c>/Notification</c></term>
        ///         <description>Canal genérico para notificações push (ex: Alertas de Jogo).</description>
        ///     </item>
        /// </list>
        /// </remarks>
        /// <param name="app">A instância da aplicação web (<see cref="WebApplication"/>) onde os hubs serão registados.</param>
        /// <returns>A instância da aplicação atualizada, permitindo o encadeamento de chamadas (Fluent API).</returns>
        public static WebApplication MapHubs(this WebApplication app)
        {
            app.MapHub<StartMatchHub>("/StartMatch");
            app.MapHub<FinishMatchHub>("/FinishMatch");
            app.MapHub<RankMatchMakerHub>("/MatchMaker");
            app.MapHub<NotificationHub>("/Notification");

            return app;
        }
    }
}
