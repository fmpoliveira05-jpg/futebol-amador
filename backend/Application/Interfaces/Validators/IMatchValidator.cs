using Application.DTOs.PostPoneGame;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface IMatchValidator
    {
        public void ValidatorPostPoneMatch(Matches match, DateTime newDate, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent);
        public void ValidatorAcceptPostPoneMatch(PostPoneMatch postPoneMatch, Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent);
        public void ValidatorRejectPostPoneMatch(PostPoneMatch postPoneMatch, Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent);
        public void ValidateCancelMatch(Matches match, TeamStatistics team, Guid idTeam, TeamStatistics opponent, Guid idOpponent);
        public void ValidatorGetListPostPoneMatchTeam(List<InfoPostPoneMatch> listPostPone);
    }
}
