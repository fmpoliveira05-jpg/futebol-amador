using Application.DTOs;
using Application.DTOs.Filters;
using Application.DTOs.PostPoneGame;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface IMatchValidator
    {
        public void ValidateFilterCalendar(Guid idTeam, FilterCalendar filter);
        public void ValidatorPostPoneMatch(Matches match, DateTime newDate, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent);
        public void ValidatorAcceptPostPoneMatch(PostPoneMatch postPoneMatch, Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent);
        public void ValidatorRejectPostPoneMatch(PostPoneMatch postPoneMatch, Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent);
        public void ValidateCancelMatch(Matches match, TeamStatistics team, Guid idTeam, TeamStatistics opponent, Guid idOpponent);
        public void ValidatorGetListPostPoneMatchTeam(List<InfoPostPoneMatch> listPostPone);
        public void validateResultMatch(Guid idTeam, ResultMatchDto result);
        public void ValidateFinishMatch(Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponent, Guid idOponnent, ResultMatchDto result);

        public void ValidateCancelFinishMatch(Matches match, Teams team);
    }
}
