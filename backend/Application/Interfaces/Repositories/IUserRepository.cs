using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>
    /// Contrato de Repositório para operações de acesso a dados da entidade base [User].
    /// 
    /// Define os métodos de consulta para buscar utilizadores por diferentes identificadores (ID, Email, Telefone),
    /// atuando sobre a tabela base de herança (Table-Per-Type).
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Obtém uma lista de todos os utilizadores registados no sistema (incluindo Player e SuperAdmin).
        /// </summary>
        /// <returns>Uma lista de todas as entidades [User].</returns>
        Task<List<User>> GetAllUsersAsync();

        /// <summary>
        /// Obtém um utilizador pelo seu identificador único (ID/UID do Firebase) de forma assíncrona.
        /// </summary>
        /// <param name="userId">O ID (string) do utilizador.</param>
        /// <returns>A entidade [User] correspondente ou null.</returns>
        Task<User?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Obtém um utilizador pelo seu endereço de e-mail de forma assíncrona.
        /// </summary>
        /// <param name="email">O endereço de e-mail do utilizador.</param>
        /// <returns>A entidade [User] ou null.</returns>
        Task<User?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Obtém um utilizador pelo seu número de telefone de forma assíncrona.
        /// </summary>
        /// <param name="phone">O número de telefone completo (incluindo código do país).</param>
        /// <returns>A entidade [User] ou null.</returns>
        Task<User?> GetUserByPhoneAsync(string phone);
    }
}