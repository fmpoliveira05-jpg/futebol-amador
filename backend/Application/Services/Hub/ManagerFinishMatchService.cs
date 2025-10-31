using Application.DTOs;
using Application.Hubs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators.Hub;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Application.Services.Hub
{
    public class ManagerFinishMatchService: IManagerFinishMatchService
    {
        #region Initialization
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
        #endregion

        #region public Methods
        public async Task<JoinFinishMatch> JoinHubAsync(Guid matchId, ResultMatchDto finishMatch, Guid userId, string connectionId)
        {  
            validator.ValidateVariableJoinMatch(matchId, finishMatch, userId, connectionId);
            
            EntryHubFinishMatch entry;
            var match = await matchRepository.GetMatchWithListPlayerById(matchId);
            validator.ValidateMatchJoinMatch(match);

            var teamMatchAdmin = match.Teams.FirstOrDefault(ts => 
                ts.Team.Members.Any(p => p.Id == userId && p.IsAdmin)
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

            if (hub.Count == 0)
            {
                result.IsFirstAdmin = true;
                result.MatchFinish = false;
            }
            else 
            {
                result.IsFirstAdmin = false;
                result.MatchFinish = true;

                result.FirstAdminConnectionId = hub.First().Value.ConnectionId;
                await FinalizeMatchIfResultsMatch(hub, match, teamMatchAdmin, result, hubCacheKey, finishMatch);
            }

            //Só guarda uma team em cache se o resultado das partidas não coincidirem
            if (!result.IsCoincides.GetValueOrDefault(false))
            {
                entry = new EntryHubFinishMatch
                {
                    ConnectionId = connectionId,
                    Result = result
                };

                //Guardo em cache o id da equipa e a sua entrada
                hub.TryAdd(teamId, entry);
                cache.Set(hubCacheKey, hub, GetCacheOptions());
            }

            return result;
        }

        public async Task<JoinFinishMatch> UpdateResult(Guid matchId, ResultMatchDto finishMatch, Guid userId, string connectionId)
        {
            validator.ValidateVariableJoinMatch(matchId, finishMatch, userId, connectionId);
   
            var match = await matchRepository.GetMatchWithListPlayerById(matchId);
            validator.ValidateMatchJoinMatch(match);

            var teamMatchAdmin = match.Teams.FirstOrDefault(ts =>
                ts.Team.Members.Any(p => p.Id == userId && p.IsAdmin)
            );

            var teamId = teamMatchAdmin.IdTeam;

            var hubCacheKey = GetHubCacheKey(matchId);
            if (!cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryHubFinishMatch>? hub))
            {
                hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();
            }

            validator.ValidateUpdateResult(teamMatchAdmin, teamId, hub);

            var result = new JoinFinishMatch
            {
                IdTeam = teamId,
                ResultMatch = finishMatch,
            };

            //Atualizar o resultado do hub
            hub[teamId].Result.ResultMatch = finishMatch;
            cache.Set(hubCacheKey, hub);

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

            return await LeaveHubAsync(maybeMatchId.Value, maybeTeamId.Value, connectionId);
        }
        #endregion
        
        #region private Methods
        private static string GetHubCacheKey(Guid matchId)
        {
            return ModelConstants.FinishMatchHubConst.PrefixHubCache + matchId;
        }

        private static MemoryCacheEntryOptions GetCacheOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        }

        private bool CoincideResults(ResultMatchDto firstResult, ResultMatchDto secondResult)
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

        private async Task FinalizeMatchIfResultsMatch(ConcurrentDictionary<Guid, EntryHubFinishMatch>? hub,
            Matches match, TeamStatistics team, JoinFinishMatch result, string hubCacheKey, ResultMatchDto finishMatch)
        {
            var opponententry = hub?.FirstOrDefault(kvp => kvp.Key != team.IdTeam).Value;
            
            if (opponententry == null)
            {
                result.IsCoincides = false;
                return;
            }

            var opponentResult = opponententry.Result.ResultMatch;

            if (opponentResult == null)
            {
                result.IsCoincides = false;
                return;
            }

            var coincideResult = CoincideResults(opponentResult, finishMatch);

            if (coincideResult)
            {
                var opponent = match.Teams.FirstOrDefault(ts => ts.IdTeam == finishMatch.IdOpponent);
                validator.ValidateOpponentTeam(opponent);

                //Atualização do numero de golos das equipas
                team.NumGoals = finishMatch.NumGoalsTeam;
                opponent.NumGoals = finishMatch.NumGoalsOpponent;

                DefineWinnerMatch(team, opponent);
                match.MatchStatus = MatchStatus.DONE;

                await unityOfWork.SaveChangesAsync();
                result.IsCoincides = true;

                cache.Remove(hubCacheKey);
            }
        }

        private static void DefineWinnerMatch(TeamStatistics team, TeamStatistics opponent)
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

            updatePointsTeams(team);
            updatePointsTeams(opponent);
        }

        private static void updatePointsTeams(TeamStatistics teamStatistic)
        {
            var team = teamStatistic.Team;
            var rank = team.Rank;

            switch(teamStatistic.MatchResult) {
                case MatchResult.WIN:
                {
                    team.CurrentPoints += rank.WinPoints;
                    break;
                }
                case MatchResult.DRAW: 
                {
                    team.CurrentPoints += rank.DrawPoints;  
                    break;
                }
                case MatchResult.LOSE:
                {
                    team.CurrentPoints += rank.LosePoints;
                    break;
                }
            }

            ValidatePromotionOrDepromotionTeam(team);
        }

        private static void ValidatePromotionOrDepromotionTeam(Teams team)
        {
            var nextRank = team.Rank.NextRank;
            var previousRank = team.Rank.PreviousRank;

            if (team.CurrentPoints >= team.Rank.PointsToPromotion) {
                team.Rank = nextRank;
            }
            else if (team.CurrentPoints < previousRank.PointsToPromotion)
            {
                team.Rank = previousRank;
            }
        }

        #endregion
    }
}
