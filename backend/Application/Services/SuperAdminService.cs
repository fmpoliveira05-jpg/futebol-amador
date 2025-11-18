using Application.DTOs.SuperAdmin;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;

namespace Application.Services
{
    public class SuperAdminService : ISuperAdminService
    {
        #region Variables and Inicializor
        private readonly ISuperAdminRepository superAdminRepository;
        private readonly IUserRepository userRepository;
        private readonly IUnityOfWork unityOfWork;
        private readonly IAuthService authService;
        private readonly ISuperAdminValidator superAdminValidator;

        public SuperAdminService(ISuperAdminRepository superAdminRepository, IUserRepository userRepository, IUnityOfWork unityOfWork, ISuperAdminValidator superAdminValidator, IAuthService authService)
        {
            this.superAdminRepository = superAdminRepository;
            this.userRepository = userRepository;
            this.unityOfWork = unityOfWork;
            this.superAdminValidator = superAdminValidator;
            this.authService = authService;
        }
        #endregion

        #region CRUD Super Admin
        public async Task<string> CreateSuperAdminAsync(CreateSuperAdminDTO dto)
        {
            var existingSadmin = new User[]{
                await userRepository.GetUserByEmailAsync(dto.Email),
                await userRepository.GetUserByPhoneAsync(dto.Phone),
            };

            superAdminValidator.CreateSuperAdminValidator(dto, existingSadmin);

            var newUserId = await authService.RegisterUser(dto.Email, dto.Password, dto.Phone);

            if (string.IsNullOrEmpty(newUserId))
            {
                throw new Exception("Error creating user in authentication service.");
            }

            var superAdmin = new SuperAdmin
            {
                Id = newUserId,
                Name = dto.Name,
                DateOfBirth = dto.DateOfBirth,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
            };

            await superAdminRepository.AddAsync(superAdmin);

            await unityOfWork.SaveChangesAsync();

            return superAdmin.Id;
        }

        public async Task DeleteSuperAdminAsync(string superAdminId)
        {
            var superAdminToDelete = await superAdminRepository.GetSuperAdminByIdAsync(superAdminId);

            superAdminValidator.DeleteSuperAdminValidator(superAdminToDelete);

            superAdminRepository.DeleteSuperAdmin(superAdminToDelete);

            authService.DeleteUserAsync(superAdminId);

            await unityOfWork.SaveChangesAsync();
        }

        public async Task<SuperAdminDetailsDTO> GetSuperAdminByIdAsync(string superAdminId)
        {
            var superAdmin = await superAdminRepository.GetSuperAdminByIdAsync(superAdminId);

            superAdminValidator.GetSuperAdminByIdValidator(superAdmin);

            SuperAdminDetailsDTO superAdminDetails = new SuperAdminDetailsDTO
            {
                Name = superAdmin.Name,
                DateOfBirth = superAdmin.DateOfBirth,
                Address = superAdmin.Address,
            };

            return superAdminDetails;
        }

        public async Task UpdateSuperAdminAsync(string superAdminId, UpdateSuperAdminDTO dto)
        {
            var superAdmin = await superAdminRepository.GetSuperAdminByIdAsync(superAdminId);
            var existingSadmin = new User[]{
                await userRepository.GetUserByEmailAsync(dto.Email),
                await userRepository.GetUserByPhoneAsync(dto.Phone),
            };

            superAdminValidator.UpdateSuperAdminValidator(dto, superAdmin, existingSadmin);

            superAdmin.Name = dto.Name;
            superAdmin.DateOfBirth = dto.DateOfBirth;
            superAdmin.Address = dto.Address;
            superAdmin.Phone = dto.Phone;
            superAdmin.Email = dto.Email;
            await authService.UpdateEmailAsync(superAdminId, dto.Email);

            superAdminRepository.UpdateSuperAdmin(superAdmin);
            
            await unityOfWork.SaveChangesAsync();
        }

        #endregion
    }
}
