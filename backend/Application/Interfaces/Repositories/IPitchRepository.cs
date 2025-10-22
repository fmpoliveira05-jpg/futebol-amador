using Domain.Entities;

namespace Application.Interfaces.Repositorys
{
    public interface IPitchRepository
    {
        public Task<Pitch?> GetPitchById(Guid id);

        public Task<Pitch?> GetPitchByName(string namePitch);
    }
}