using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ITeamPostPoneGameRepository
    {
        public Task AddTeamPostPoneMatch(PostPoneMatch postPoneMatch);
        public void RemoveTeamPostPoneMatch(PostPoneMatch postPoneMatch);
        public Task<PostPoneMatch?> GetTeamPostPoneMatchWithPitch(Guid idTeam, Guid idMatch);
        public Task<PostPoneMatch?> GetTeamPostPoneMatch(Guid idTeam, Guid idMatch);
    }
}
