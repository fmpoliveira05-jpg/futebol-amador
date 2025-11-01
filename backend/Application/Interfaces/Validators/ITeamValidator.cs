using Application.DTOs.Filters;
using Application.DTOs.Team;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    public interface ITeamValidator
    {
        void CreateTeamValidation(CreateTeamDto createTeamDto, Teams? team, Player? playerCreating);
        void UpdateTeamValidation(Teams? existingTeamNewName, Teams? team, Player? playerEditing);

        void DeleteTeamValidation(Teams? team, Player? playerDeleting);

        void GetTeamByIdValidation(TeamDetailsDto team);
        //validar depois se fica ca!
        void GetAllTeamsValidation(IEnumerable<Teams?> Teams);

        void RemovePlayerFromTeamValidation(Teams? team, Player? playerRemoving, Player? playerRemoved);

        void GetTeamMembersValidation(Teams? team);

        void GetMembershipRequestsValidation(Teams? team, Player? adminPlayer);

        void SearchTeamsValidation(Player player, TeamSearchFiltersDto filters);

        void ApproveMembershipRequestValidation(Teams? team, Player? playerApproving, Guid requestToDelete);

        void RejectMembershipRequestValidation(Teams? team, Player? playerRejecting, Guid requestToDelete);

        void SendMembershipRequestValidation(Teams? team, Player? playerSending, Player? playerReceiving);

        void DemoteAdminToMemberValidation(Teams? team, Player? adminToDemote, Player? adminDemoting);

        void PromoteMemberToAdminValidation(Teams? team, Player? memberToPromote, Player? memberPromoting);

        void GetTeamScheduleValidation(Teams? team);
        public void ValidateVariableSearchTeam(Guid idTeam);
        public void ValidateVaribleSearchTeamWithFilters(Guid idTeam, FilterListTeamDto filter);
        public void ValidateTeamSearch(Teams team);
    }
}
