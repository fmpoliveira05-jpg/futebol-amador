using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators.Hub;
using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Application.Services.Hub
{
    public class ManagerRankMatchMakerHub: IManagerRankMatchMakerService
    {
        private readonly ITeamRepository teamRepository;
        private readonly IUnityOfWork unityOfWork;
        private readonly IRankMatchMakerValidator validator;
        private readonly IMemoryCache cache;

        public ManagerRankMatchMakerHub(ITeamRepository teamRepository, IUnityOfWork unityOfWork, 
            IRankMatchMakerValidator validator, IGeralHubValidator geralValidator,
            IMemoryCache cache)
        {
            this.teamRepository = teamRepository;
            this.unityOfWork = unityOfWork;
            this.validator = validator;
            //this.geralValidator = geralValidator;
            this.cache = cache;
        }

        //Falta implementar os validatores
        //Falta depois criar uma cena para apos 5 minutos de pesquisa ele ser rejeitado automaticamente
        public async Task<InfoTeamRankMatchMakerDto> JoinRankMatchMaker(Guid idPlayer, Guid idTeam, string connectionId)
        {
            validator.ValidateVariableJoinRankMatchMaker(idPlayer, idTeam, connectionId);
           
            Teams? team;
            string hubCacheKey = "";
            bool? findUser = false;
            float averageAge = 0;
            InfoTeamRankMatchMakerDto result;
            EntryRankMatchMakerHub entry;

            team = await teamRepository.GetTeamForMemberManagementAsync(idPlayer);

            validator.ValidateTeamJoinRankMatchMaker(team);

            findUser = team?.Members.Any(m => m.Id == idPlayer && m.IsAdmin);

            hubCacheKey = GetHubCacheKey(idTeam);
            if (!cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub))
            {
                hub = new ConcurrentDictionary<Guid, EntryRankMatchMakerHub>();
            }

            averageAge = CalculateAverageAge(team);
            validator.ValidateJoinRankMatchMaker(team, averageAge, findUser, hub);
            
            result = new InfoTeamRankMatchMakerDto
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
            };

            entry = new EntryRankMatchMakerHub
            {
                ConnectionId = connectionId,
                Team = result
            };


            hub?.TryAdd(idTeam, entry);
            cache.Set(hubCacheKey, hub);
            //cache.Set(hubCacheKey, hub, GetCacheOptions());
            
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
                        //cache.Set(hubCacheKey, hub, GetCacheOptions());
                        cache.Set(hubCacheKey, hub);
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

            return await Task.FromResult(await LeaveRankMatchMakerAsync(maybeTeamId.Value, connectionId));
        }

        private static string GetHubCacheKey(Guid idTeam)
        {
            return $"matchRankMaker-{idTeam}";
        }

        private static float CalculateAverageAge(Teams team)
        {
            var dateOnly = DateOnly.FromDateTime(DateTime.UtcNow);
            double averageDays = team.Members.Average(m => (dateOnly.DayNumber - m.DateOfBirth.DayNumber));
            return (float)(averageDays / 365.2425);
        }
    }
}
