using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IPitchRepository
    {
        public Task<Pitch?> GetPitchById(Guid id);

        public Task<Pitch?> GetPitchByName(string namePitch);
    }
}