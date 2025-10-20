using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ITeamPostPoneGameRepository
    {
        Task AddTeamPostPoneMatch(PostPoneMatch postPoneMatch);

        void RemoveTeamPostPoneMatch(PostPoneMatch postPoneMatch);
        Task<PostPoneMatch?> GetTeamPostPoneMatch(Guid idTeam, Guid idMatch);
    }
}
