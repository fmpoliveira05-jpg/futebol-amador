using Application.Hubs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators.Hub;
using Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Application.Services
{
    public class ManagerStartMatchService : IManagerStartMatchService
    {
        private readonly IMatchRepository matchRepository; //Atualizar match
        private readonly IUnityOfWork unityOfWork;
        private readonly IStartMatchHubValidator validator;
        private readonly IGeralHubValidator geralValidator;
        private readonly IMemoryCache cache;

        public ManagerStartMatchService(IMatchRepository matchRepository, IUnityOfWork unityOfWork,
            IStartMatchHubValidator validator, IGeralHubValidator geralValidator, IMemoryCache cache)
        {
            this.matchRepository = matchRepository;
            this.unityOfWork = unityOfWork;
            this.validator = validator;
            this.geralValidator = geralValidator;
            this.cache = cache;
        }

        private string GetHubCacheKey(Guid matchId)
        {
            return $"hub-{matchId}";
        } 

        private MemoryCacheEntryOptions GetCacheOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        }

        public async Task<JoinStartMatchResult> JoinHubAsync(Guid matchId, Guid userId, Guid idTeam, string connectionId)
        {
            validator.ValidateVariableJoinMatch(matchId, userId, connectionId);

            var match = await matchRepository.GetMatchWithListPlayerById(matchId);
            validator.ValidateMatchJoinMatch(match);

            var teamMatchAdmin = match.Teams.FirstOrDefault(ts => ts.IdTeam == idTeam &&
                ts.Team.Members.Any(p => p.Id == userId && p.IsAdmin == true)
            );

            var hubCacheKey = GetHubCacheKey(matchId);
            if (!cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, string>? hub))
            {
                hub = new ConcurrentDictionary<Guid, string>();
            }

            validator.ValidateJoinMatch(teamMatchAdmin, idTeam, hub);

            var result = new JoinStartMatchResult
            {
                TeamId = idTeam,
                Match = match
            };

            if (hub.Count() == 0)
            {
                //Adicionar admin ao hub
                hub.TryAdd(idTeam, connectionId);
                cache.Set(hubCacheKey, hub, GetCacheOptions());
                result.IsFirstAdmin = true;
                result.MatchStarted = false;
            }
            else
            {
                //Adicionar 2º admin e fechar hub
                var first = hub.First();
                var firstTeamId = first.Key;
                var firstConnectionId = first.Value;

                match.MatchStatus = MatchStatus.IN_PROGRESS;
                match.TimeStart = DateTime.UtcNow;
                await unityOfWork.SaveChangesAsync();

                cache.Remove(hubCacheKey);

                result.IsFirstAdmin = false;
                result.MatchStarted = true;
                result.FirstAdminConnectionId = firstConnectionId;
            }

            return result;
        }

        /*
         public async Task<JoinStartMatchResult> JoinHubAsync(Guid matchId, Guid userId, string connectionId)
        {
            Guid teamId = Guid.Empty;
            
            validator.ValidateVariableJoinMatch(matchId, userId, connectionId);
            
            var match = await matchRepository.GetMatchWithListPlayerById(matchId);
            validator.ValidateMatchJoinMatch(match);

            var teamMatchAdmin = match.Teams.FirstOrDefault(ts =>
                ts.Team.Members.Any(p => p.Id == userId && p.IsAdmin == true)
            );

            teamId = teamMatchAdmin.IdTeam;

            var hubCacheKey = GetHubCacheKey(matchId);
            if (!cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, string>? hub))
            {
                hub = new ConcurrentDictionary<Guid, string>();
            }

            validator.ValidateJoinMatch(teamMatchAdmin, teamId, hub);

            var result = new JoinStartMatchResult
            {
                TeamId = teamId,
                Match = match
            };

            if (hub.Count() == 0)
            {
                //Adicionar admin ao hub
                hub.TryAdd(teamId, connectionId);
                cache.Set(hubCacheKey, hub, GetCacheOptions());
                result.IsFirstAdmin = true;
                result.MatchStarted = false;
            }
            else
            {
                //Adicionar 2º admin e fechar hub
                var first = hub.First();
                var firstTeamId = first.Key;
                var firstConnectionId = first.Value;

                match.MatchStatus = MatchStatus.IN_PROGRESS;
                match.TimeStart = DateTime.UtcNow;
                await unityOfWork.SaveChangesAsync();

                cache.Remove(hubCacheKey);

                result.IsFirstAdmin = false;
                result.MatchStarted = true;
                result.FirstAdminConnectionId = firstConnectionId;
            }

            return result;
        }
         */

        public async Task<bool> LeaveHubAsync(Guid matchId, Guid idTeam, string connectionId)
        {
            geralValidator.ValidateIdMatchLeaveMatch(matchId, idTeam);
            var hubCacheKey = GetHubCacheKey(matchId);

            if (cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, string>? hub))
            {
                if (hub.TryRemove(idTeam, out _))
                {
                    // se esvaziou, remove cache; senão atualiza
                    if (hub.IsEmpty)
                    {
                        cache.Remove(hubCacheKey);
                    }
                    else
                    {
                        cache.Set(hubCacheKey, hub, GetCacheOptions());
                    }
                    return true;
                }
            }

            return false;
        }
        public async Task<bool> HandleDisconnectAsync(Guid? maybeMatchId, Guid? maybeTeamId, string connectionId)
        {
            if (!maybeMatchId.HasValue || !maybeTeamId.HasValue)
            {
                return false;
            }

            return await Task.FromResult(await LeaveHubAsync(maybeMatchId.Value, maybeTeamId.Value, connectionId));
        }
    }
}
