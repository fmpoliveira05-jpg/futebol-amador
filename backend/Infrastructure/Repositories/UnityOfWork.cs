using Application.Interfaces.Repositories;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class UnityOfWork : IUnityOfWork
    {
        private readonly AmateurFootballContext context;

        public UnityOfWork(AmateurFootballContext context)
        {
            this.context = context;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await context.SaveChangesAsync();
        }
    }
}
