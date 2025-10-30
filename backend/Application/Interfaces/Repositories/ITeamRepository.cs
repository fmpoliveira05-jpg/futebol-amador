using Application.DTOs.MemberShip;
using Application.DTOs.Team;
using Domain.Entities;
namespace Application.Interfaces.Repositories
{
    public interface ITeamRepository
    {
        Task<List<Teams>?> GetAllTeamsAsync();
        Task<Teams?> GetTeamByIdAsync(Guid id);
        Task<Teams?> GetTeamByNameAsync(string name);
        void DeleteTeam(Teams teamToRemove);
        void UpdateTeam(Teams updatedTeam);
        Task AddAsync(Teams team);
        Task<TeamDetailsDto?> GetTeamDetailsDtoAsync(Guid teamId);

        Task<List<MemberShipRequestDto>?> GetMembershipRequestsDtoAsync(Guid teamId);

        Task<Teams?> GetTeamForMembershipRequestAsync(Guid id);

        Task<Teams?> GetTeamForDeletionAsync(Guid id);

        Task<Teams?> GetTeamForUpdateAsync(Guid id);

        Task<Teams?> GetTeamByNameWithMembersAsync(string name);

        Task<Teams?> GetTeamForMemberManagementAsync(Guid id);

        Task<Teams?> GetTeamWitchMemberRankAndPitchAsync(Guid id);

        Task<Teams?> GetTeamByIdWithPitchAsync(Guid id);

        Task<Teams?> GetByIdWithReceivedInvitesAndCalendar(Guid id);

        Task<Teams?> GetByIdWithReceivedInvites(Guid id);

        IQueryable<Teams> GetTeamsQueryable();
    }
}
