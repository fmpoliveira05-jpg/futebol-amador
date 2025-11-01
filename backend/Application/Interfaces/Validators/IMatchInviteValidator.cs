using Application.DTOs.Filters;
using Application.DTOs.MatchInvites;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface IMatchInviteValidator
    {
        public void ValidateSenderMatchInvite(SendMatchInviteDTO dto, Guid idSender);
        public void ValidateAcceptRefuseMatchInvite(Guid idReceiver, Guid idMatchInvite);
        public void ValidateTeamCalendar(Guid idTeam);
        public void ValidateSendMatchInvite(Teams receiver, Teams sender, MatchInvite matchInviteFind, Matches findMatchWith12hours, string namePitch);
        public void ValidateMatchInvite(MatchInvite matchInvite);
        public void ValidateAcceptMatchInvite(Teams sender, Matches twentyhoursMatch, MatchInvite matchInvite, Pitch pitch);
        public void ValidateReciever(Teams receiver);
        public void ValidateRefuseMatchInvite(Teams sender, MatchInvite matchInvite);
        public void ValidateNegociateMatchInvite(string namePitch, Pitch pitch, MatchInvite matchInvite, Teams senderTeam, Teams receiverTeam, Matches findMatchWith12hour);
        public void ValidateHasChangeNegociateMatchInvite(bool hasChanged);
        public void ValidateFilterMatchInvite(Guid idTeam, FilterMatchInvitesDto filter);
    }
}
