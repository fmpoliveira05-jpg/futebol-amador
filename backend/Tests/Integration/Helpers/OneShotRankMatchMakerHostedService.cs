using Api.Hubs;
using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Hub;
using Application.Interfaces.Services.Hub;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tests.Integration.Helpers
{
    public class OneShotRankMatchMakerHostedService : IHostedService
    {
        private readonly IServiceProvider _provider;
        private readonly IHubContext<RankMatchMakerHub, IRankMatchMakerHub> _hubContext;
        private readonly ILogger<OneShotRankMatchMakerHostedService> _logger;

        public OneShotRankMatchMakerHostedService(IServiceProvider provider, IHubContext<RankMatchMakerHub, IRankMatchMakerHub> hubContext, ILogger<OneShotRankMatchMakerHostedService> logger)
        {
            _provider = provider;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _provider.CreateScope();
                var manager = scope.ServiceProvider.GetRequiredService<IManagerRankMatchMakerService>();

                var criteria = new CriteriaMatchMaker { differencPoints = 10, diffAverageAge = 5f };

                var result = await manager.MatchMaker(criteria);

                if (result != null && result.Count > 0)
                {
                    foreach (var kv in result)
                    {
                        if (kv.Key != null)
                        {
                            var entry = kv.Key;
                            var connId = entry.ConnectionId;
                            var groupName = Domain.Constants.ModelConstants.RankMatchMakerHubConst.PrefixGroupName + entry.Team.IdTeam;

                            await _hubContext.Clients.Group(groupName).OnGroupClosed(groupName);
                            await _hubContext.Groups.RemoveFromGroupAsync(connId, groupName);
                        }

                        if (kv.Value != null)
                        {
                            var entry = kv.Value;
                            var connId = entry.ConnectionId;
                            var groupName = Domain.Constants.ModelConstants.RankMatchMakerHubConst.PrefixGroupName + entry.Team.IdTeam;

                            await _hubContext.Clients.Group(groupName).OnGroupClosed(groupName);
                            await _hubContext.Groups.RemoveFromGroupAsync(connId, groupName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OneShotRankMatchMakerHostedService failed.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    }
}
