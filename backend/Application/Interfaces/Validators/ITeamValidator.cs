using Application.DTOs.Team;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Validators
{
    internal interface ITeamValidator
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

        void ApproveMembershipRequestValidation(Teams? team, Player? playerApproving, Guid requestToDelete);

        void RejectMembershipRequestValidation(Teams? team, Player? playerRejecting, Guid requestToDelete);

        void SendMembershipRequestValidation(Teams? team, Player? playerSending, Player? playerReceiving);

        void DemoteAdminToMemberValidation(Teams? team, Player? adminToDemote, Player? adminDemoting);

        void PromoteMemberToAdminValidation(Teams? team, Player? memberToPromote, Player? memberPromoting);

        void GetTeamScheduleValidation(Teams? team);
    }
}
