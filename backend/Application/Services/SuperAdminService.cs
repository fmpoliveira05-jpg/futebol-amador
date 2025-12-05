using Application.DTOs.SuperAdmin;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;

namespace Application.Services
{
    /// <summary>
    /// Serviço de domínio responsável pela lógica de negócio e gestão da entidade [SuperAdmin].
    /// 
    /// Implementa o CRUD e orquestra as operações transacionais que afetam tanto o fornecedor de identidade (Auth Service) 
    /// quanto a base de dados relacional.
    /// </summary>
    public class SuperAdminService : ISuperAdminService
    {
        #region Variables and Inicializor
        private readonly ISuperAdminRepository superAdminRepository;
        private readonly IUserRepository userRepository;
        private readonly IUnityOfWork unityOfWork;
        private readonly IAuthService authService;
        private readonly ISuperAdminValidator superAdminValidator;

        /// <summary>
        /// Construtor do SuperAdminService.
        /// </summary>
        /// <param name="superAdminRepository">Repositório de Super Administradores.</param>
        /// <param name="userRepository">Repositório genérico de Utilizadores (para verificar unicidade de Email/Telefone).</param>
        /// <param name="unityOfWork">Unidade de Trabalho para gerir as transações.</param>
        /// <param name="superAdminValidator">Validador de Regras de Negócio de Super Admin.</param>
        /// <param name="authService">Serviço de Autenticação (para criar, atualizar e apagar utilizadores no Firebase).</param>
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

        /// <summary>
        /// Cria um novo Super Administrador.
        /// </summary>
        /// <remarks>
        /// **Transação Firebase-DB:**
        /// 1. Valida a unicidade de Email/Telefone ([userRepository]).
        /// 2. Cria o utilizador no serviço de autenticação ([authService.RegisterUser]).
        /// 3. Cria a entidade [SuperAdmin] na base de dados relacional, usando o UID retornado.
        /// 4. Persiste as alterações ([UnityOfWork]).
        /// </remarks>
        /// <param name="dto">DTO com os dados do Super Admin a criar.</param>
        /// <returns>O ID (string) do novo Super Administrador criado.</returns>
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

        /// <summary>
        /// Elimina um Super Administrador.
        /// </summary>
        /// <remarks>
        /// **Transação:**
        /// 1. Valida a existência do Super Admin.
        /// 2. Marca a entidade para eliminação ([superAdminRepository.DeleteSuperAdmin]).
        /// 3. Elimina o utilizador do fornecedor de identidade ([authService.DeleteUserAsync]).
        /// 4. Persiste a eliminação na base de dados ([UnityOfWork]).
        /// </remarks>
        /// <param name="superAdminId">O ID do Super Admin a eliminar.</param>
        /// <param name="currentUserId">O ID do utilizador que executa a deleção (deve ser um Super Admin).</param>
        public async Task DeleteSuperAdminAsync(string superAdminId)
        {
            var superAdminToDelete = await superAdminRepository.GetSuperAdminByIdAsync(superAdminId);

            superAdminValidator.DeleteSuperAdminValidator(superAdminToDelete);

            superAdminRepository.DeleteSuperAdmin(superAdminToDelete);

            authService.DeleteUserAsync(superAdminId);

            await unityOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Obtém o perfil de um Super Administrador pelo seu ID.
        /// </summary>
        /// <param name="superAdminId">O ID do Super Admin.</param>
        /// <returns>O DTO [SuperAdminDetailsDTO] com os dados do perfil.</returns>
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

        /// <summary>
        /// Atualiza as informações de um Super Administrador.
        /// </summary>
        /// <remarks>
        /// **Regras:** Valida a unicidade de Email/Telefone. Atualiza os dados locais na DB e no serviço de autenticação (Firebase).
        /// </remarks>
        /// <param name="superAdminId">O ID do Super Admin a atualizar.</param>
        /// <param name="dto">DTO com os dados a serem alterados.</param>
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