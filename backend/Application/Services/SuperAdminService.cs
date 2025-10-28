using Application.DTOs.SuperAdmin;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class SuperAdminService : ISuperAdminService
    {
        ISuperAdminRepository superAdminRepository;
        IUnityOfWork unityOfWork;

        public SuperAdminService(ISuperAdminRepository superAdminRepository, IUnityOfWork unityOfWork)
        {
            this.superAdminRepository = superAdminRepository;
            this.unityOfWork = unityOfWork;
        }

        public async Task<Guid> CreateSuperAdminAsync(CreateSuperAdminDTO dto)
        {
            var existingSadmin = new SuperAdmin[]{
                await superAdminRepository.GetSuperAdminByEmailAsync(dto.Email),
                await superAdminRepository.GetSuperAdminByPhoneAsync(dto.Phone),
            };

            //validator

            var superAdmin = new SuperAdmin
            {
                Name = dto.Name,
                DateOfBirth = dto.DateOfBirth,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Password = dto.Password,
            };

            await superAdminRepository.AddAsync(superAdmin);

            await unityOfWork.SaveChangesAsync();

            return superAdmin.Id;
        }

        public async Task DeleteSuperAdminAsync(Guid superAdminId)
        {
            var superAdminToDelete = await superAdminRepository.GetSuperAdminByIdAsync(superAdminId);

            //validator

            superAdminRepository.DeleteSuperAdmin(superAdminToDelete);

            await unityOfWork.SaveChangesAsync();
        }

        public async Task<SuperAdminDetailsDTO> GetSuperAdminByIdAsync(Guid superAdminId)
        {
            var superAdmin = await superAdminRepository.GetSuperAdminByIdAsync(superAdminId);

            //validator

            SuperAdminDetailsDTO superAdminDetails = new SuperAdminDetailsDTO
            {
                Name = superAdmin.Name,
                DateOfBirth = superAdmin.DateOfBirth,
                Address = superAdmin.Address,
            };

            return superAdminDetails;
        }

        public async Task UpdateSuperAdminAsync(Guid superAdminId, UpdateSuperAdminDTO dto)
        {
            var superAdmin = await superAdminRepository.GetSuperAdminByIdAsync(superAdminId);
            var existingSadmin = new SuperAdmin[]{
                await superAdminRepository.GetSuperAdminByEmailAsync(dto.Email),
                await superAdminRepository.GetSuperAdminByPhoneAsync(dto.Phone),
            };

            //validator

            superAdmin.Name = dto.Name;
            superAdmin.DateOfBirth = dto.DateOfBirth;
            superAdmin.Address = dto.Address;
            superAdmin.Phone = dto.Phone;
            superAdmin.Email = dto.Email;

            superAdminRepository.UpdateSuperAdmin(superAdmin);

            await unityOfWork.SaveChangesAsync();
        }
    }
}
