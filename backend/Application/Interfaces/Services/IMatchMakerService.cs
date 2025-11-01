using Application.DTOs.RankMatchMaker;

namespace Application.Interfaces.Services
{
    public interface IMatchMakerService
    {
        Guid? LogicMatchMakerJoinHub(InfoTeamRankMatchMakerDto finder, IEnumerable<InfoTeamRankMatchMakerDto> teamsInSearch, DateTime gameDate);
        Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub>? LogicMatchMaker(IEnumerable<EntryRankMatchMakerHub> teamsInSearch, CriteriaMatchMaker criteria);
    }
}