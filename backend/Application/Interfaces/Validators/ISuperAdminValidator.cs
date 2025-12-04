using Application.DTOs.SuperAdmin;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    /// <summary>
    /// Contrato de Validador de Regras de Negócio e Controlo de Integridade para a entidade [SuperAdmin].
    /// 
    /// Esta interface define as regras de validação síncrona que verificam a unicidade, o formato dos dados
    /// e a existência de entidades cruciais antes de operações CRUD serem executadas.
    /// </summary>
    public interface ISuperAdminValidator
    {
        /// <summary>
        /// Valida se a entidade [SuperAdmin] existe.
        /// </summary>
        /// <param name="sadmin">A entidade SuperAdmin a ser verificada.</param>
        /// <exception cref="Domain.Exceptions.NotFoundException">Lançada se a entidade SuperAdmin for nula.</exception>
        public void SuperAdminExists(SuperAdmin sadmin);

        /// <summary>
        /// Valida os dados e a unicidade antes de criar um novo Super Administrador.
        /// </summary>
        /// <remarks>
        /// Verifica a unicidade de Email e Telefone (em relação a todos os utilizadores existentes) e a validade do DTO de criação.
        /// </remarks>
        /// <param name="dto">O DTO [CreateSuperAdminDTO] com os dados do Super Admin a criar.</param>
        /// <param name="sadmins">Um array/coleção de utilizadores existentes (para verificação de unicidade de Email/Telefone).</param>
        /// <exception cref="Domain.Exceptions.ValidationException">Lançada se o Email ou Telefone já estiver em uso ou se a idade/formato for inválido.</exception>
        public void CreateSuperAdminValidator(CreateSuperAdminDTO dto, User[] sadmins);

        /// <summary>
        /// Valida a existência de um Super Administrador através do seu ID.
        /// </summary>
        /// <param name="sadmin">A entidade SuperAdmin a ser verificada.</param>
        /// <exception cref="Domain.Exceptions.NotFoundException">Lançada se a entidade SuperAdmin for nula.</exception>
        public void GetSuperAdminByIdValidator(SuperAdmin sadmin);

        /// <summary>
        /// Valida as condições para eliminar um Super Administrador.
        /// </summary>
        /// <remarks>
        /// A regra principal é que a entidade [SuperAdmin] a ser eliminada deve existir.
        /// </remarks>
        /// <param name="sadmin">A entidade SuperAdmin a ser eliminada.</param>
        /// <exception cref="Domain.Exceptions.NotFoundException">Lançada se a entidade SuperAdmin não for encontrada.</exception>
        public void DeleteSuperAdminValidator(SuperAdmin sadmin);

        /// <summary>
        /// Valida os dados e a unicidade antes de atualizar um Super Administrador.
        /// </summary>
        /// <remarks>
        /// Requer validação de unicidade condicional: se o Email ou Telefone for alterado, o novo valor não pode pertencer a outro utilizador.
        /// </remarks>
        /// <param name="dto">O DTO [UpdateSuperAdminDTO] com os novos dados de atualização.</param>
        /// <param name="sadmin">A entidade SuperAdmin existente.</param>
        /// <param name="sadmins">Utilizadores existentes (para verificação de unicidade de Email/Telefone).</param>
        /// <exception cref="Domain.Exceptions.NotFoundException">Lançada se a entidade SuperAdmin não for encontrada.</exception>
        /// <exception cref="Domain.Exceptions.ValidationException">Lançada se o formato ou a unicidade for violada.</exception>
        public void UpdateSuperAdminValidator(UpdateSuperAdminDTO dto, SuperAdmin sadmin, User[] sadmins);
    }
}