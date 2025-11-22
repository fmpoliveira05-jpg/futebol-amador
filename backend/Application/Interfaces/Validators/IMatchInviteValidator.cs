using Application.DTOs.Filters;
using Application.DTOs.MatchInvites;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface IMatchInviteValidator
    {
        public void ValidateSenderMatchInvite(SendMatchInviteDto dto, Guid idSender);
        public void ValidateAcceptRefuseMatchInvite(Guid idReceiver, Guid idMatchInvite);
        public void ValidateTeamCalendar(Guid idTeam);
        public void ValidateSendMatchInvite(Team receiver, Team sender, MatchInvite matchInviteFind, Matches findMatchWith12hours);
        public void ValidateMatchInvite(MatchInvite matchInvite);
        public void ValidateAcceptMatchInvite(Team sender, Matches twentyhoursMatch, MatchInvite matchInvite, Pitch pitch);
        public void ValidateReciever(Team receiver);
        public void ValidateRefuseMatchInvite(Team sender, MatchInvite matchInvite);
        public void ValidateNegociateMatchInvite(Pitch pitch, MatchInvite matchInvite, Team senderTeam, Team receiverTeam, Matches findMatchWith12hour);
        public void ValidateHasChangeNegociateMatchInvite(bool hasChanged);
        public void ValidateFilterMatchInvite(Guid idTeam, FilterMatchInvitesDto filter);
    }
}
