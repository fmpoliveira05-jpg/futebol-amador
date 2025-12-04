using Application.DTOs.SuperAdmin;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Validators
{
    /// <summary>
    /// Validador de Regras de Negócio e de Entidade para operações relacionadas com [SuperAdmin].
    /// 
    /// Esta classe verifica a validade dos dados e a unicidade de campos críticos (Email, Telefone)
    /// antes de a persistência de dados ser executada.
    /// </summary>
    public class SuperAdminValidator : ISuperAdminValidator
    {
        /// <summary>
        /// Validador delegado para operações de validação de formato de baixo nível (Email, Telefone).
        /// </summary>
        private readonly IUserDataValidator emailValidator;

        /// <summary>
        /// Construtor da classe [SuperAdminValidator].
        /// </summary>
        /// <param name="emailValidator">O validador delegado de dados de utilizador, injetado via Dependency Injection.</param>
        public SuperAdminValidator(IUserDataValidator emailValidator)
        {
            this.emailValidator = emailValidator;
        }

        /// <summary>
        /// Valida se a entidade [SuperAdmin] existe.
        /// </summary>
        /// <param name="sadmin">A entidade SuperAdmin a ser verificada.</param>
        /// <exception cref="NotFoundException">Lançada se a entidade SuperAdmin for nula.</exception>
        public void SuperAdminExists(SuperAdmin sadmin)
        {
            if (sadmin == null)
            {
                throw new NotFoundException("Super Admin doesn't exist.");
            }
        }

        /// <summary>
        /// Valida os dados e a unicidade antes de criar um novo Super Administrador.
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>Unicidade: O Email e o Telefone não podem estar em uso por outro utilizador.</item>
        ///     <item>Formato: O formato do Email deve ser válido.</item>
        ///     <item>Idade: A data de nascimento deve estar dentro dos limites válidos (18 a 70 anos).</item>
        /// </list>
        /// </remarks>
        /// <param name="dto">O DTO contendo os dados do Super Admin.</param>
        /// <param name="sadmins">Um array/coleção de utilizadores existentes (utilizado para verificação de unicidade).</param>
        /// <exception cref="ValidationException">Se o email/telefone já estiver em uso ou a data de nascimento for inválida.</exception>
        public void CreateSuperAdminValidator(CreateSuperAdminDTO dto, User[] sadmins)
        {
            if (sadmins[0] != null)
            {
                throw new ValidationException($"The email '{sadmins[0].Email}' is already in use.");
            }

            if (sadmins[1] != null)
            {
                throw new ValidationException($"The phone number '{sadmins[1].Phone}' is already in use.");
            }

            emailValidator.EmailValidation(dto.Email);

            if (dto.DateOfBirth > DateOnly.FromDateTime(DateTime.Now).AddYears(-18)
                || dto.DateOfBirth < DateOnly.FromDateTime(DateTime.Now).AddYears(-70))
            {
                throw new ValidationException("Invalid Date of birth");
            }
        }

        /// <summary>
        /// Valida as condições para eliminar um Super Administrador.
        /// </summary>
        /// <remarks>
        /// A única regra verificada é que a entidade [SuperAdmin] a ser eliminada deve existir.
        /// </remarks>
        /// <param name="sadmin">A entidade SuperAdmin a ser eliminada.</param>
        /// <exception cref="NotFoundException">Se a entidade SuperAdmin não for encontrada.</exception>
        public void DeleteSuperAdminValidator(SuperAdmin sadmin)
        {
            SuperAdminExists(sadmin);
        }

        /// <summary>
        /// Valida os dados e a unicidade antes de atualizar um Super Administrador.
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>A entidade deve existir.</item>
        ///     <item>Unicidade: Se o Email/Telefone for alterado, o novo valor não pode estar em uso por outra conta.</item>
        ///     <item>Formato: Validação de formato de Email e Telefone (comprimento e caracteres).</item>
        ///     <item>Idade: A data de nascimento deve ser válida.</item>
        /// </list>
        /// </remarks>
        /// <param name="dto">O DTO com os novos dados de atualização.</param>
        /// <param name="sadmin">A entidade SuperAdmin existente.</param>
        /// <param name="sadmins">Utilizadores existentes (para verificação de unicidade).</param>
        /// <exception cref="NotFoundException">Se a entidade SuperAdmin não for encontrada.</exception>
        /// <exception cref="ValidationException">Se o formato ou a unicidade for violada.</exception>
        public void UpdateSuperAdminValidator(UpdateSuperAdminDTO dto, SuperAdmin sadmin, User[] sadmins)
        {
            SuperAdminExists(sadmin);

            if (dto.Email != sadmin.Email)
            {
                if (sadmins[0] != null)
                {
                    throw new ValidationException($"The email '{sadmins[0].Email}' is already in use.");
                }
            }

            if (dto.Phone != sadmin.Phone)
            {
                if (sadmins[1] != null)
                {
                    throw new ValidationException($"The phone number '{sadmins[1].Phone}' is already in use.");
                }
            }

            emailValidator.EmailValidation(dto.Email);

            if (dto.DateOfBirth > DateOnly.FromDateTime(DateTime.Now).AddYears(-18)
                || dto.DateOfBirth < DateOnly.FromDateTime(DateTime.Now).AddYears(-70))
            {
                throw new ValidationException("Invalid Date of birth");
            }

            if (dto.Phone.Length != 9)
            {
                throw new ValidationException("Phone number must have 9 digits.");
            }

            if (!int.TryParse(dto.Phone, out _))
            {
                throw new ValidationException("Phone number must only have numbers.");
            }
        }

        /// <summary>
        /// Validação de existência para o Super Administrador através do seu ID.
        /// </summary>
        /// <param name="sadmin">A entidade SuperAdmin a ser verificada.</param>
        /// <exception cref="NotFoundException">Se a entidade SuperAdmin não for encontrada.</exception>
        public void GetSuperAdminByIdValidator(SuperAdmin sadmin)
        {
            SuperAdminExists(sadmin);
        }
    }
}