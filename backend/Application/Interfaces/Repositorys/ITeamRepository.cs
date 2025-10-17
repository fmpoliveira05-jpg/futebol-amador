using Domain.Entities;

namespace Application.Interfaces.Repositorys
{
    public interface ITeamRepository
    {
        Task<Teams?> GetTeamById(Guid id);

        Task<Teams?> GetByIdWithReceivedInvites(Guid id);
        Task<Teams?> GetByIdWithReceivedInvitesAndCalendar(Guid id);
    }
}
