using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ITeamRepository
    {
        Task<List<Teams>?> GetAllTeamsAsync();
        Task<Teams?> GetTeamByNameAsync(string name);
        void DeleteTeam(Teams teamToRemove);
        void UpdateTeam(Teams updatedTeam);
        Task AddAsync(Teams team);
        Task<Teams?> GetTeamByIdAsync(Guid id);

        Task<Teams?> GetTeamByIdWithPitchAsync(Guid id);
        Task<Teams?> GetByIdWithReceivedInvites(Guid id);
        Task<Teams?> GetByIdWithReceivedInvitesAndCalendar(Guid id);
    }
}
