using Application.DTOs.SuperAdmin;

namespace Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de Serviço de Domínio para a gestão completa da entidade [SuperAdmin].
    /// 
    /// Esta interface define as operações CRUD essenciais para a administração da plataforma.
    /// </summary>
    public interface ISuperAdminService
    {
        /// <summary>
        /// Cria e regista um novo perfil de Super Administrador.
        /// </summary>
        /// <remarks>
        /// Esta operação deve ser transacional, sincronizando a criação do utilizador no fornecedor de identidade (Firebase) com a base de dados relacional.
        /// </remarks>
        /// <param name="dto">DTO [CreateSuperAdminDTO] com os dados do Super Admin a criar.</param>
        /// <returns>Uma tarefa assíncrona que retorna o ID (string) do Super Administrador criado.</returns>
        Task<string> CreateSuperAdminAsync(CreateSuperAdminDTO dto);

        /// <summary>
        /// Obtém os dados detalhados do perfil de um Super Administrador pelo ID.
        /// </summary>
        /// <param name="superAdminId">O ID (string) do Super Administrador.</param>
        /// <returns>Uma tarefa assíncrona que retorna o DTO [SuperAdminDetailsDTO] com os dados do perfil.</returns>
        Task<SuperAdminDetailsDTO> GetSuperAdminByIdAsync(string superAdminId);

        /// <summary>
        /// Atualiza as informações de um Super Administrador.
        /// </summary>
        /// <remarks>
        /// Requer validação de unicidade e deve sincronizar as alterações de Email/Telefone com o fornecedor de identidade (Firebase) se necessário.
        /// </remarks>
        /// <param name="superAdminId">O ID do Super Administrador a atualizar.</param>
        /// <param name="dto">DTO [UpdateSuperAdminDTO] com os dados a serem alterados.</param>
        Task UpdateSuperAdminAsync(string superAdminId, UpdateSuperAdminDTO dto);

        /// <summary>
        /// Elimina permanentemente a conta de um Super Administrador.
        /// </summary>
        /// <remarks>
        /// Esta operação deve ser transacional, eliminando o Super Admin da base de dados relacional e do fornecedor de identidade (Firebase).
        /// </remarks>
        /// <param name="superAdminId">O ID do Super Administrador a eliminar.</param>
        Task DeleteSuperAdminAsync(string superAdminId);
    }
}