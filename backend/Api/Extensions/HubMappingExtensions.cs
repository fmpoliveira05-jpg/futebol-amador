using Api.Hubs;

namespace Api.Extensions
{
    public static class HubMappingExtensions
    {
        public static WebApplication MapHubs(this WebApplication app)
        {
            app.MapHub<StartMatchHub>("/StartMatch");
            app.MapHub<FinishMatchHub>("/FinishMatch");
            app.MapHub<RankMatchMakerHub>("/MatchMaker");

            return app;
        }
    }
}
