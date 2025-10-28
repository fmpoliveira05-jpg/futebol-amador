using Application.DTOs.MemberShip;
using Application.DTOs.Team;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Application.Interfaces.Repositories
{
    public interface ITeamRepository
    {
        Task<List<string>> GetAdminsIdsByTeamIdAsync(Guid teamId);
        Task<List<string>> GetMemberIdsByTeamIdAsync(Guid teamId);
        Task<List<Teams>?> GetAllTeamsAsync();
        Task<Teams?> GetTeamByIdAsync(Guid id);
        Task<Teams?> GetTeamByNameAsync(string name);

        Task DeleteTeam(Teams teamToRemove);

        Task UpdateTeam(Teams updatedTeam);

        Task AddAsync(Teams team);
        Task<TeamDetailsDto?> GetTeamDetailsDtoAsync(Guid teamId);

        Task<List<MemberShipRequestDto>?> GetMembershipRequestsDtoAsync(Guid teamId);

        Task<Teams?> GetTeamForMembershipRequestAsync(Guid id);

        Task<Teams?> GetTeamForDeletionAsync(Guid id);

        Task<Teams?> GetTeamForUpdateAsync(Guid id);

        Task<Teams?> GetTeamByNameWithMembersAsync(string name);

        Task<Teams?> GetTeamForMemberManagementAsync(Guid id);

        Task<Teams?> GetTeamByIdWithPitchAsync(Guid id);

        Task<Teams?> GetByIdWithReceivedInvitesAndCalendar(Guid id);

        Task<Teams?> GetByIdWithReceivedInvites(Guid id);

        Task<List<TeamSummaryDto>> GetAllTeamsWithFilters(TeamSearchFiltersDto filters);
    }
}
