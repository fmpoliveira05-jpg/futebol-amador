using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PitchRepository: IPitchRepository
    {
        private readonly AmateurFootballContext context;

        public PitchRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        public async Task<Pitch?> GetPitchById(Guid id)
        {
            return await context.Pitch.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Pitch?> GetPitchByName(string namePitch)
        {
            return await context.Pitch.FirstOrDefaultAsync(p => p.Name == namePitch);
        }
    }
}
