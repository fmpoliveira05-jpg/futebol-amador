using Application.DTOs.Filters;
using Application.DTOs.Team;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface ITeamValidator
    {
        void CreateTeamValidation(CreateTeamDto createTeamDto, Rank rank, Team? team, Player? playerCreating);
        void UpdateTeamValidation(Team? existingTeamNewName, Team? team);
        void DeleteTeamValidation(Team? team);
        void GetTeamByIdValidation(TeamDetailsDto team);
        //validar depois se fica ca!
        void GetAllTeamsValidation(IEnumerable<Team?> Teams);
        void RemovePlayerFromTeamValidation(Team? team, Player? playerRemoving, Player? playerRemoved);
        void GetTeamMembersValidation(Team? team);
        void GetMembershipRequestsValidation(Team? team, Player? adminPlayer);
        void ApproveMembershipRequestValidation(Team? team, Player? admin, Guid requestToDelete);
        void RejectMembershipRequestValidation(Team? team, Player? admin, Guid requestToDelete);
        void SendMembershipRequestValidation(MembershipRequest? mr, Team? team, Player? admin, Player? playerToInvite);
        void DemoteAdminToMemberValidation(Team? team, Player? adminToDemote, Player? adminDemoting);
        void PromoteMemberToAdminValidation(Team? team, Player? memberToPromote, Player? memberPromoting);
        void GetTeamScheduleValidation(Team? team);
        public void ValidateVariableSearchTeam(Guid idTeam);
        public void ValidateVaribleSearchTeamWithFilters(Guid idTeam, FilterListTeamDto filter);
        public void ValidateTeamSearch(Team team);

        public void ValidateFiltersGetPlayersWithout(FilterPlayersWithoutTeamDto filter);
    }
}
