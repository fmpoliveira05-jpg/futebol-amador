using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators.Hub;
using Domain.Constants;
using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;


//Criar configuração do cache para apagar o gajo em 30 minutos
namespace Application.Services.Hub
{
    public class ManagerRankMatchMakerService: IManagerRankMatchMakerService
    {
        private readonly IMatchMakerService serviceMatchMaker;
        private readonly ITeamRepository teamRepository;
        private readonly IMatchRepository matchRepository;
        private readonly IUnityOfWork unityOfWork;
        private readonly IRankMatchMakerValidator validator;
        private readonly IMemoryCache cache;

        public ManagerRankMatchMakerService(IMatchMakerService serviceMatchMaker, ITeamRepository teamRepository,
            IMatchRepository matchRepository, IUnityOfWork unityOfWork, IRankMatchMakerValidator validator, 
            IMemoryCache cache)
        {
            this.serviceMatchMaker = serviceMatchMaker;
            this.teamRepository = teamRepository;
            this.matchRepository = matchRepository;
            this.unityOfWork = unityOfWork;
            this.validator = validator;
            this.cache = cache;
        }

        public async Task<EntryRankMatchMakerHub?> JoinRankMatchMaker(Guid idPlayer, 
            Guid idTeam, TimeOnly hoursGame, string connectionId)
        {
            validator.ValidateVariableJoinRankMatchMaker(idPlayer, idTeam, hoursGame, connectionId);
            
            Teams? team;
            string hubCacheKey = "";
            bool? findUser = false;
            float averageAge = 0;
            InfoTeamRankMatchMakerDto InfoTeam;
            EntryRankMatchMakerHub entry;
            string city = "";
            string previousOrNextRank = "";
            bool haveChangeDifferenceRank = false;
            DateOnly atualDate = DateOnly.FromDateTime(GetNextSunday(DateTime.UtcNow));
            DateTime gameDate = new DateTime(atualDate, hoursGame);
            double differenteHoursNowAndGame = (DateTime.UtcNow - gameDate).TotalHours;

            var teamHaveMatchInThisDay = await matchRepository.GetMatchProxim12HoursMatchs(idTeam, gameDate);
            validator.ValidateHoursToMatch(differenteHoursNowAndGame, teamHaveMatchInThisDay);

            team = await teamRepository.GetTeamWitchMemberRankAndPitchAsync(idTeam);
            validator.ValidateTeamJoinRankMatchMaker(team);

            findUser = team?.Members.Any(m => m.Id == idPlayer && m.IsAdmin);
            hubCacheKey = GetHubCacheKey(idTeam);
            if (!cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub))
            {
                hub = new ConcurrentDictionary<Guid, EntryRankMatchMakerHub>();
            }

            averageAge = CalculateAverageAge(team);
            city = GetCityTeam(team.Pitch.Address);
            previousOrNextRank = GetPreviousOrNextRankTeam(team.CurrentPoints, team.Rank);

            if (!string.IsNullOrEmpty(previousOrNextRank))
            {
                haveChangeDifferenceRank = true;
            }
            
            validator.ValidateJoinRankMatchMaker(team, averageAge, city, findUser, hub);
            
            InfoTeam = new InfoTeamRankMatchMakerDto
            {
                IdTeam = idTeam,
                Name = team.Name,
                Rank = new InfoRankMatchMakerDto
                {   
                    IdRank = team.IdRank,
                    Name = team.Name,
                },
                NumberPointsTeam = team.CurrentPoints,
                AverageAge = averageAge, 
                City = city,
                GameDate = gameDate,
                NextOrPreviousRank = previousOrNextRank,
                isNearToChangeRank = haveChangeDifferenceRank
            };

            entry = new EntryRankMatchMakerHub
            {
                ConnectionId = connectionId,
                Team = InfoTeam
            };
            
            var teamsInCache = GetAllTeamsInCache();
            
            //Não retornar um id mas sim a team (para corrigir o erro, se calhar)
            var teamMatchId = serviceMatchMaker.LogicMatchMakerJoinHub(InfoTeam, teamsInCache, gameDate);

            if (teamMatchId == null || teamMatchId == Guid.Empty)
            {
                hub?.TryAdd(idTeam, entry);
                cache.Set(hubCacheKey, hub, GetCacheOptions());

                if (!cache.TryGetValue(ModelConstants.ManagerRankMatchMakerServiceConst.GlobalHubKeysCacheKey, out HashSet<string>? globalKeys))
                {
                    globalKeys = new HashSet<string>();
                }

                globalKeys?.Add(hubCacheKey);
                cache.Set(ModelConstants.ManagerRankMatchMakerServiceConst.GlobalHubKeysCacheKey, globalKeys, GetCacheOptions());
            }
            else
            {
                await CreateMatch(teamMatchId.Value, team, gameDate);
            }

            return entry;
        }

        public async Task<Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub>> MatchMaker(CriteriaMatchMaker criteria)
        {
            var teamsInCache = GetAllEntryInCache();
            var result = serviceMatchMaker.LogicMatchMaker(teamsInCache, criteria);

            if (result?.Count > 0)
            {
                await CreateMatchMaking(result);
            }

            return result;
        }

        public async Task<bool> LeaveRankMatchMakerAsync(Guid teamId, string connectionId)
        {
            validator.ValidateLeaveRankMatchMaker(teamId);
            var hubCacheKey = GetHubCacheKey(teamId);

            if (cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub))
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
                        cache.Set(hubCacheKey, hub);
                    }

                    if (cache.TryGetValue(ModelConstants.ManagerRankMatchMakerServiceConst.GlobalHubKeysCacheKey, out HashSet<string>? globalKeys))
                    {
                        globalKeys?.Remove(hubCacheKey);
                        cache.Set(ModelConstants.ManagerRankMatchMakerServiceConst.GlobalHubKeysCacheKey, globalKeys, GetCacheOptions());
                    }

                    return true;
                }
            }

            return false;
        }

        public async Task<bool> HandleDisconnectAsync(Guid? maybeTeamId, string connectionId)
        {
            if (!maybeTeamId.HasValue)
            {
                return false;
            }

            return await LeaveRankMatchMakerAsync(maybeTeamId.Value, connectionId);
        }


        #region Private Methods
        /*
         metodo que é cahamdo pelo backGroundService onde se o mesmo achar teams para fazer
         uma match ele chama este metodo para criar as partidas e notificar as equipas
         */
        private async Task CreateMatchMaking(Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub> matchMaker)
        {
            foreach (var pair in matchMaker)
            {
                var team1 = pair.Key.Team;
                var team2 = pair.Value.Team;

                await CreateMatch(team1.IdTeam, team2.IdTeam, team2.GameDate);
            }
        }
        
        /*
         Retorna todas as teams em cache em que atualmente estão sozinhas em hub
         há procura de match
        */
        private IEnumerable<EntryRankMatchMakerHub> GetAllEntryInCache()
        {
            var allTeams = new List<EntryRankMatchMakerHub>();

            if (cache.TryGetValue(ModelConstants.ManagerRankMatchMakerServiceConst.GlobalHubKeysCacheKey, out HashSet<string>? hubKeys))
            {
                foreach (var hubCacheKey in hubKeys)
                {
                    if (cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub))
                    {
                        if (hub.Count == 1)
                        {
                            var entry = hub.Values.First();
                            allTeams.Add(entry);
                        }
                    }
                }
            }

            return allTeams;
        }

        private IEnumerable<InfoTeamRankMatchMakerDto> GetAllTeamsInCache()
        {
            var allTeams = new List<InfoTeamRankMatchMakerDto>();

            if (cache.TryGetValue(ModelConstants.ManagerRankMatchMakerServiceConst.GlobalHubKeysCacheKey, out HashSet<string>? hubKeys))
            {
                foreach (var hubCacheKey in hubKeys)
                {
                    if (cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub))
                    {
                        if (hub.Count == 1)
                        {
                            var entry = hub.Values.First();
                            allTeams.Add(entry.Team);
                        }
                    }
                }
            }
            return allTeams;
        }

        private static MemoryCacheEntryOptions GetCacheOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
        }

        private static string GetHubCacheKey(Guid idTeam)
        {
            return ModelConstants.ManagerRankMatchMakerServiceConst.PrefixHubCache + idTeam;
        }

        private static DateTime GetNextSunday(DateTime startDate)
        {
            int currentDayOfWeek = (int)startDate.DayOfWeek;
            int targetDayOfWeek = (int)DayOfWeek.Sunday;

            int daysToAdd = targetDayOfWeek - currentDayOfWeek;
            if (daysToAdd <= 0)
            {
                daysToAdd += 7;
            }

            return startDate.Date.AddDays(daysToAdd);
        }

        private static float CalculateAverageAge(Teams team)
        {
            var dateOnly = DateOnly.FromDateTime(DateTime.UtcNow);
            double averageDays = team.Members.Average(m => (dateOnly.DayNumber - m.DateOfBirth.DayNumber));
            return (float)(averageDays / 365.2425);
        }

        private static string GetCityTeam(string addressTeam)
        {
            var city = "";
            var pattern = new Regex(@"^(?<street>.*),(?<number>.*),(?<floor>.*),(?<postalCode>.*),(?<city>.*),(?<parish>.*),(?<district>.*)$", RegexOptions.Compiled);
            var validateAddres = pattern.Match(addressTeam);
            city = validateAddres.Groups["city"].Value;       
            
            return city;
        }

        private static string GetPreviousOrNextRankTeam(int numberPointsTeam, Rank rankTeam)
        {
            const int differencePoints = ModelConstants.DeafultCriteriaMatchMaker.differencePoint;
            var nextOrPreviewsRank = "";
            var previousRankTeam = rankTeam.PreviousRank;

            if(rankTeam.PointsToPromotion - numberPointsTeam <= differencePoints)
            {
                nextOrPreviewsRank = rankTeam.NextRank.Name;
            }
            else if (numberPointsTeam - previousRankTeam.PreviousRank.PointsToPromotion <= differencePoints)
            {
                nextOrPreviewsRank = previousRankTeam.Name;
            }

            return nextOrPreviewsRank;
        }

        private async Task CreateMatch(Guid teamMatchId, Teams team, DateTime gameDate)
        {
            var teamMatchFind = await teamRepository.GetTeamByIdAsync(teamMatchId);

            var teamStatistics = new List<TeamStatistics>();
            var team1 = new TeamStatistics(team);
            var teamFind = new TeamStatistics(teamMatchFind);
            var match = new Matches(gameDate, true, teamMatchFind.Pitch.Id, teamStatistics);

            team1.MatchesId = match.Id;
            teamFind.MatchesId = match.Id;

            await unityOfWork.SaveChangesAsync();
        }

        private async Task CreateMatch(Guid teamMatchId, Guid idTeam2, DateTime gameDate)
        {
            var teamMatchFind = await teamRepository.GetTeamByIdAsync(teamMatchId);
            var team = await teamRepository.GetTeamByIdAsync(idTeam2);

            var teamStatistics = new List<TeamStatistics>();
            var team1 = new TeamStatistics(team);
            var teamFind = new TeamStatistics(teamMatchFind);
            var match = new Matches(gameDate, true, teamMatchFind.Pitch.Id, teamStatistics);

            team1.MatchesId = match.Id;
            teamFind.MatchesId = match.Id;

            await unityOfWork.SaveChangesAsync();
        }

        #endregion
    }
}
