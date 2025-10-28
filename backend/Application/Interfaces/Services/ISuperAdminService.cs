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
        Task<Guid> CreateSuperAdminAsync(CreateSuperAdminDTO dto);

        Task<SuperAdminDetailsDTO> GetSuperAdminByIdAsync(Guid superAdminId);

        Task UpdateSuperAdminAsync(Guid superAdminId, UpdateSuperAdminDTO dto);

        Task DeleteSuperAdminAsync(Guid superAdminId);
    }
}
