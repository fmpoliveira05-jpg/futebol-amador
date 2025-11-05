using Application.DTOs.SuperAdmin;

namespace Application.Interfaces.Services
{
    public interface ISuperAdminService
    {
        Task<string> CreateSuperAdminAsync(CreateSuperAdminDTO dto);

        Task<SuperAdminDetailsDTO> GetSuperAdminByIdAsync(string superAdminId);

        Task UpdateSuperAdminAsync(string superAdminId, UpdateSuperAdminDTO dto);

        Task DeleteSuperAdminAsync(string superAdminId);
    }
}
