using Application.DTOs.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
