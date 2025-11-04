using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface ICalendarValidator
    {
        public void ExistsMatch(Matches match);
        public void ExistsTeamStatistics(TeamStatistics team);
        public void ValidateTeamCalendar(Guid idTeam);
        public void ValidateFilterCalendar(Guid idTeam, FilterCalendarDto filter);
        public void ValidatePostPoneMatchDto(Guid idTeam, PostPoneMatchDto dto);
        public void ValidatorPostPoneMatch(Matches match, DateTime newDate, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent);
        public void ValidateAcceptPostPoneMatchDto(Guid idTeam, AcceptRefusePostPoneDto dto);
        public void ValidatorAcceptPostPoneMatch(PostPoneMatch postPoneMatch, Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent, Matches matchFind);
        public void ValidateRejectPostPoneMatchDTO(Guid idTeam, AcceptRefusePostPoneDto dto);
        public void ValidatorRejectPostPoneMatch(PostPoneMatch postPoneMatch, Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent);
        public void ValidateVariabelCancelMatch(Guid idTeam, Guid idMatch, string description);
        public void ValidateCancelMatch(Matches match, TeamStatistics team, Guid idTeam, TeamStatistics opponent, Guid idOpponent);
        public void validateResultMatch(Guid idTeam, ResultMatchDto result);
        public void ValidateFinishMatch(Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponent, Guid idOponnent, ResultMatchDto result);
        public void ValidateCancelFinishMatch(Matches match, Team team);
        public void ValidateFilterPostPoneMatch(FilterPostPoneMatchDto filter);
    }
}
