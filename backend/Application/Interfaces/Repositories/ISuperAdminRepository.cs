using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ISuperAdminRepository
    {
        Task<List<SuperAdmin>> GetAllSuperAdminsAsync();

        Task<SuperAdmin?> GetSuperAdminByIdAsync(Guid sadminId);

        Task<SuperAdmin?> GetSuperAdminByEmailAsync(string email);

        Task<SuperAdmin?> GetSuperAdminByPhoneAsync(string phone);

        void DeleteSuperAdmin(SuperAdmin sadminToRemove);

        void UpdateSuperAdmin(SuperAdmin updatedSadmin);

        Task AddAsync(SuperAdmin superAdmin);
    }
}
