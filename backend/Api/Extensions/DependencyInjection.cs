using Application.Services.BackGroundServices;

namespace Api.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiBackGroundService(this IServiceCollection services)
        {
            services.AddHostedService<RankMatchMakerBackGroundService>();

            return services;
        }
    }
}