using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SuperAdminRepository : ISuperAdminRepository
    {
        private readonly AmateurFootballContext context;

        public async Task AddAsync(SuperAdmin superAdmin)
        {
            await context.AddAsync(superAdmin);
        }

        public void DeleteSuperAdmin(SuperAdmin sadminToRemove)
        {
            context.SuperAdmin.Remove(sadminToRemove);
        }

        public async Task<List<SuperAdmin>> GetAllSuperAdminsAsync()
        {
            return await context.SuperAdmin.ToListAsync();
        }

        public async Task<SuperAdmin?> GetSuperAdminByEmailAsync(string email)
        {
            var superAdmin = await context.SuperAdmin.FirstOrDefaultAsync(s => s.Email == email);

            return superAdmin;
        }

        public async Task<SuperAdmin?> GetSuperAdminByIdAsync(Guid sadminId)
        {
            var superAdmin = await context.SuperAdmin.FirstOrDefaultAsync(s => s.Id == sadminId);

            return superAdmin;
        }

        public async Task<SuperAdmin?> GetSuperAdminByPhoneAsync(string phone)
        {
            var superAdmin = await context.SuperAdmin.FirstOrDefaultAsync(s => s.Phone == phone);

            return superAdmin;
        }

        public void UpdateSuperAdmin(SuperAdmin updatedSadmin)
        {
            context.SuperAdmin.Update(updatedSadmin);
        }
    }
}
