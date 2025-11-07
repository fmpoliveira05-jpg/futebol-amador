using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Services;
using Domain.Constants;

namespace Application.Services
{
    public class MatchMakerService : IMatchMakerService
    {
        #region MatchMaker Methods
        /**
         Metodo que procura uma partida para uma equipa que acabou de começar a procura por
         */
        public Guid? LogicMatchMakerJoinHub(InfoTeamRankMatchMakerDto finder, 
            IEnumerable<InfoTeamRankMatchMakerDto> teamsInSearch, DateTime gameDate)
        {
            IEnumerable<InfoTeamRankMatchMakerDto> orderTeams;
            var rank = finder.Rank.Name;
            InfoTeamRankMatchMakerDto? teamFind;

            if (string.IsNullOrEmpty(finder.NextOrPreviousRank))
            {
                orderTeams = teamsInSearch.Where(t => t.Rank.Name == rank)
                                          .OrderByDescending(t => t.timeEntry);
            } 
            else
            {
                var otherRank = finder.NextOrPreviousRank;
                orderTeams = teamsInSearch.Where(t => t.Rank.Name == rank ||
                                                 t.Rank.Name == otherRank)
                                           .OrderByDescending(t => t.timeEntry);
            }

            teamFind = orderTeams.FirstOrDefault(t => t.City == finder.City &&
            t.GameDate == finder.GameDate &&
            Math.Abs(finder.AverageAge - t.AverageAge) <= ModelConstants.DeafultCriteriaMatchMaker.differenceAverageAge &&
            Math.Abs(finder.NumberPointsTeam - t.NumberPointsTeam) <= ModelConstants.DeafultCriteriaMatchMaker.differencePoint);

            if (teamFind == null)
            {
                return null;
            }

            return teamFind.IdTeam;
        }

        public Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub>? LogicMatchMaker(
            IEnumerable<EntryRankMatchMakerHub> teamsInSearch,
            CriteriaMatchMaker criteria)
        {
            var result = new Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub>();
            var matched = new HashSet<Guid?>(); // tornar O(1) ver se uma team já tem uma match
            var diffPoints = criteria.differencPoints;
            var diffAge = criteria.diffAverageAge;
            var agrupListTeams = GetDicitonaryTeamGroupByMatchDate(teamsInSearch);
            
            //Utilizar sliding window
            foreach (var dateGame in agrupListTeams)
            {
                var teamsList = dateGame.Value; //lista de equipas
                var countTeams = teamsList.Count;

                if (countTeams < 2)
                {
                    continue;
                }

                //Ordernar a lista pelo o numero de pontos
                teamsList.Sort((a, b) => a.Team.NumberPointsTeam.CompareTo(b.Team.NumberPointsTeam));

                for (int left = 0; left < countTeams; left++)
                {
                    var entry = teamsList[left];
                    var team = entry.Team;
                    var teamId = team.IdTeam;
                    //Ver se a team já está numa match
                    
                    if (matched.Contains(teamId))
                    {
                        continue;
                    }

                    var findTeam = false;
                    var pointerRight = left + 1;
                    while (pointerRight < countTeams && !findTeam && Math.Abs(team.NumberPointsTeam - teamsList[pointerRight].Team.NumberPointsTeam) <= diffPoints)
                    {
                        var criteriaPass = 0;
                        var entryRight = teamsList[pointerRight];
                        var teamRight = entryRight.Team;

                        if (team.City == teamRight.City)
                        {
                            criteriaPass++;
                        }

                        if (Math.Abs(team.AverageAge - teamRight.AverageAge) <= diffAge)
                        {
                            criteriaPass++;
                        }

                        if (criteriaPass == 2)
                        {
                            if (!result.ContainsKey(entry))
                            {
                                result[entry] = entryRight;
                            }
                            matched.Add(teamId);
                            matched.Add(teamRight.IdTeam);
                            findTeam = true;
                        } 
                        else 
                        {
                            pointerRight++;
                        }

                    }
                }

            }

            return result;
        }
        #endregion

        #region Private Methods
        private static Dictionary<DateTime, List<EntryRankMatchMakerHub>> GetDicitonaryTeamGroupByMatchDate(IEnumerable<EntryRankMatchMakerHub> teamsInSearch) 
        {
            var dicionary = teamsInSearch
                .GroupBy(t => t.Team.GameDate)
                .ToDictionary(dic => dic.Key, teams => teams.ToList());

            return dicionary;
        }
        #endregion
    }
}