using Application.DTOs;
using Application.Hubs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators.Hub;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class ManagerFinishMatchService: IManagerFinishMatchService
    {
        private readonly IMatchRepository matchRepository; 
        private readonly IUnityOfWork unityOfWork;
        private readonly IFinishMatchValidator validator;
        private readonly IGeralHubValidator geralValidator;
        private readonly IMemoryCache cache;

        public ManagerFinishMatchService(IMatchRepository matchRepository, IUnityOfWork unityOfWork, IFinishMatchValidator validator, IGeralHubValidator geralValidator, IMemoryCache cache)
        {
            this.matchRepository = matchRepository;
            this.unityOfWork = unityOfWork;
            this.validator = validator;
            this.geralValidator = geralValidator;
            this.cache = cache;
        }
        private string GetHubCacheKey(Guid matchId)
        {
            return $"hubFinishMatch-{matchId}";
        }

        private MemoryCacheEntryOptions GetCacheOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        }

        private bool CoincideResults(FinishMatchDTO firstResult, FinishMatchDTO secondResult)
        {
            var coincide = false;
            validator.ValidateMatchResultTwoTeams(firstResult, secondResult);
           
            if (firstResult.NumGoalsTeam == secondResult.NumGoalsOpponent &&
                firstResult.NumGoalsOpponent == secondResult.NumGoalsTeam)
            {
                coincide = true;
            }

            return coincide;
        }

        private void DefineWinnerMatch(TeamStatistics team, TeamStatistics opponent)
        {
            var numGoalsTeam = team.NumGoals;
            var numGoalsOpponent = opponent.NumGoals;

            if (numGoalsTeam > numGoalsOpponent)
            {
                team.MatchResult = MatchResult.WIN;
                opponent.MatchResult = MatchResult.LOSE;
            }
            else if (numGoalsTeam < numGoalsOpponent) 
            {
                team.MatchResult = MatchResult.LOSE;
                opponent.MatchResult = MatchResult.WIN;
            }
            else
            {
                team.MatchResult = MatchResult.DRAW;
                opponent.MatchResult = MatchResult.DRAW;
            }
        }

        private async Task FinalizeMatchIfResultsMatch(ConcurrentDictionary<Guid, EntryHubFinishMatch>? hub, 
            Matches match, TeamStatistics team, JoinFinishMatch result, string hubCacheKey, FinishMatchDTO finishMatch)
        {
            var first = hub.First();
            var firstTeamId = first.Key;
            var firstConnectionId = first.Value.ConnectionId;
            var firstResult = first.Value.Result.ResultMatch;

            var coincideResult = CoincideResults(firstResult, finishMatch);
            
            if (coincideResult)
            {
                var opponent = match.Teams.FirstOrDefault(ts => ts.IdTeam == finishMatch.IdTeam);
                validator.ValidateOpponentTeam(opponent);

                DefineWinnerMatch(team, opponent);
                match.MatchStatus = MatchStatus.DONE;

                await unityOfWork.SaveChangesAsync();
                result.IsCoincides = true;

                cache.Remove(hubCacheKey);
            }   
        }

        //Validar se o IdMatch é o mesmo do da ultima pessoa
        public async Task<JoinFinishMatch> JoinHubAsync(Guid matchId, FinishMatchDTO finishMatch, Guid userId, string connectionId)
        {  
            validator.ValidateVariableJoinMatch(matchId, finishMatch, userId, connectionId);
            
            EntryHubFinishMatch entry;
            var match = await matchRepository.GetMatchWithListPlayerById(matchId);
            validator.ValidateMatchJoinMatch(match);

            var teamMatchAdmin = match.Teams.FirstOrDefault(ts => 
                ts.Team.Members.Any(p => p.Id == userId && p.IsAdmin == true)
            );

            var teamId = teamMatchAdmin.IdTeam;
            var hubCacheKey = GetHubCacheKey(matchId);

            if (!cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryHubFinishMatch>? hub))
            {
                hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();
            }

            validator.ValidateJoinMatch(teamMatchAdmin, teamId, hub);

            var result = new JoinFinishMatch
            {
                IdTeam = teamId,
                ResultMatch = finishMatch,
            };

            if (hub.Count() == 0)
            {
                result.IsFirstAdmin = true;
                result.MatchFinish = false;

                entry = new EntryHubFinishMatch
                {
                    ConnectionId = connectionId,
                    Result = result
                };

                hub.TryAdd(teamId, entry);
                cache.Set(hubCacheKey, hub, GetCacheOptions());
            }
            else 
            {
                result.IsFirstAdmin = false;
                result.MatchFinish = true;
                result.FirstAdminConnectionId = hub.First().Value.ConnectionId;
                await FinalizeMatchIfResultsMatch(hub, match, teamMatchAdmin, result, hubCacheKey, finishMatch);
            }

            return result;
        }

        public async Task<JoinFinishMatch> UpdateResult(Guid matchId, FinishMatchDTO finishMatch, Guid userId, string connectionId)
        {
            validator.ValidateVariableJoinMatch(matchId, finishMatch, userId, connectionId);
   
            var match = await matchRepository.GetMatchWithListPlayerById(matchId);
            validator.ValidateMatchJoinMatch(match);

            var teamMatchAdmin = match.Teams.FirstOrDefault(ts =>
                ts.Team.Members.Any(p => p.Id == userId && p.IsAdmin == true)
            );

            var teamId = teamMatchAdmin.IdTeam;

            var hubCacheKey = GetHubCacheKey(matchId);
            if (!cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryHubFinishMatch>? hub))
            {
                hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();
            }

            validator.ValidateJoinMatch(teamMatchAdmin, teamId, hub);

            var result = new JoinFinishMatch
            {
                IdTeam = teamId,
                ResultMatch = finishMatch,
            };

            await FinalizeMatchIfResultsMatch(hub, match, teamMatchAdmin, result, hubCacheKey, finishMatch);

            return result;
        }
        public async Task<bool> LeaveHubAsync(Guid matchId, Guid teamId, string connectionId)
        {
            geralValidator.ValidateIdMatchLeaveMatch(matchId, teamId);
            var hubCacheKey = GetHubCacheKey(matchId);

            if (cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryHubFinishMatch>? hub))
            {
                if (hub.TryRemove(teamId, out _))
                {
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
