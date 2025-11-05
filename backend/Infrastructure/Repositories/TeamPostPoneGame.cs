using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TeamPostPoneGame : ITeamPostPoneGameRepository
    {
        private readonly AmateurFootballContext context;

        public TeamPostPoneGame(AmateurFootballContext context)
        {
            this.context = context;
        }

        public async Task AddTeamPostPoneMatch(PostPoneMatch postPoneMatch)
        {
            await context.PostPoneMatch.AddAsync(postPoneMatch);
        }

        public void RemoveTeamPostPoneMatch(PostPoneMatch postPoneMatch)
        {
            context.PostPoneMatch.Remove(postPoneMatch);
        }

        public async Task<PostPoneMatch?> GetTeamPostPoneMatch(Guid idTeam, Guid idMatch)
        {
            return await context.PostPoneMatch
                .Include(ts => ts.Match)
                .FirstOrDefaultAsync(ppm => ppm.IdTeamPostPone == idTeam && ppm.IdMatch == idMatch);
        }

        public async Task<PostPoneMatch?> GetTeamPostPoneMatchWithPitch(Guid idTeam, Guid idMatch)
        {
            return await context.PostPoneMatch
                .Include(ts => ts.Match).ThenInclude(m => m.Pitch)
                .FirstOrDefaultAsync(ppm => ppm.IdTeamPostPone == idTeam && ppm.IdMatch == idMatch);
        }
    }
}
