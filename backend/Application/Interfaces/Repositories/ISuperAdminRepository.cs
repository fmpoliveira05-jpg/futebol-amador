using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>
    /// Contrato de Repositório para operações de acesso a dados da entidade [SuperAdmin].
    /// 
    /// Define os métodos de consulta e persistência (CRUD) para utilizadores com privilégios de Super Administrador.
    /// </summary>
    public interface ISuperAdminRepository
    {
        /// <summary>
        /// Obtém uma lista de todos os Super Administradores registados.
        /// </summary>
        /// <returns>Uma lista de todas as entidades [SuperAdmin].</returns>
        Task<List<SuperAdmin>> GetAllSuperAdminsAsync();

        /// <summary>
        /// Obtém um Super Administrador pelo seu identificador único (ID/UID do Firebase).
        /// </summary>
        /// <param name="sadminId">O ID (string) do Super Administrador.</param>
        /// <returns>A entidade [SuperAdmin] ou null se não for encontrada.</returns>
        Task<SuperAdmin?> GetSuperAdminByIdAsync(string sadminId);

        /// <summary>
        /// Obtém um Super Administrador pelo seu endereço de e-mail.
        /// </summary>
        /// <param name="email">O endereço de e-mail.</param>
        /// <returns>A entidade [SuperAdmin] ou null.</returns>
        Task<SuperAdmin?> GetSuperAdminByEmailAsync(string email);

        /// <summary>
        /// Obtém um Super Administrador pelo seu número de telefone.
        /// </summary>
        /// <param name="phone">O número de telefone completo.</param>
        /// <returns>A entidade [SuperAdmin] ou null.</returns>
        Task<SuperAdmin?> GetSuperAdminByPhoneAsync(string phone);

        /// <summary>
        /// Adiciona um novo registo de Super Administrador à base de dados.
        /// </summary>
        /// <param name="superAdmin">A entidade [SuperAdmin] a ser persistida.</param>
        Task AddAsync(SuperAdmin superAdmin);

        /// <summary>
        /// Marca uma entidade [SuperAdmin] existente para ser atualizada na base de dados.
        /// </summary>
        /// <param name="updatedSadmin">A entidade [SuperAdmin] com os novos valores.</param>
        void UpdateSuperAdmin(SuperAdmin updatedSadmin);

        /// <summary>
        /// Marca um Super Administrador existente para ser removido da base de dados (deleção).
        /// </summary>
        /// <param name="sadminToRemove">A entidade [SuperAdmin] a ser removida.</param>
        void DeleteSuperAdmin(SuperAdmin sadminToRemove);
    }
}