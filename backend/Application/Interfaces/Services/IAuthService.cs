using Application.DTOs;

namespace Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de Serviço de Domínio para operações de Autenticação e Gestão de Identidade de Utilizadores.
    /// 
    /// Esta interface abstrai a lógica de login, registo e gestão de credenciais, coordenando a comunicação
    /// com o fornecedor de identidade (Firebase) e a base de dados relacional.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Realiza o processo de login do utilizador.
        /// </summary>
        /// <remarks>
        /// Autentica o utilizador no fornecedor de identidade e recupera os dados do perfil interno.
        /// </remarks>
        /// <param name="email">O email do utilizador.</param>
        /// <param name="password">A password do utilizador.</param>
        /// <returns>Um DTO [LoginResponseDto] contendo os dados do perfil e os tokens de acesso.</returns>
        Task<LoginResponseDto> LoginAsync(string email, string password);

        /// <summary>
        /// Altera a palavra-passe de um utilizador.
        /// </summary>
        /// <remarks>
        /// Requer a senha atual para validação de segurança.
        /// </remarks>
        /// <param name="userId">O ID do utilizador alvo.</param>
        /// <param name="currentPassword">A password atual.</param>
        /// <param name="newPassword">A nova password.</param>
        Task ChangePasswordAsync(string userId, string currentPassword, string newPassword);

        /// <summary>
        /// Atualiza o endereço de e-mail de um utilizador.
        /// </summary>
        /// <param name="userId">O ID do utilizador alvo.</param>
        /// <param name="newEmail">O novo endereço de e-mail.</param>
        Task UpdateEmailAsync(string userId, string newEmail);

        /// <summary>
        /// Termina a sessão do utilizador, revogando os tokens de atualização.
        /// </summary>
        /// <param name="userId">O ID do utilizador a fazer logout.</param>
        Task LogoutAsync(string userId);

        /// <summary>
        /// Elimina permanentemente a conta de um utilizador.
        /// </summary>
        /// <remarks>
        /// Este método deve ser assíncrono, mas a assinatura é definida como void/Task, dependendo da necessidade de esperar a conclusão.
        /// </remarks>
        /// <param name="userId">O ID do utilizador a eliminar.</param>
        void DeleteUserAsync(string userId);

        /// <summary>
        /// Cria uma nova identidade de utilizador no fornecedor de autenticação (Firebase).
        /// </summary>
        /// <param name="email">Email do novo utilizador.</param>
        /// <param name="password">Password do novo utilizador.</param>
        /// <param name="phoneNumber">Número de telefone do novo utilizador.</param>
        /// <returns>O ID (string) gerado para o novo utilizador.</returns>
        Task<string> RegisterUser(string email, string password, string phoneNumber);

        /// <summary>
        /// Obtém todos os dados detalhados do perfil do utilizador (Player/SuperAdmin) a partir da base de dados interna.
        /// </summary>
        /// <remarks>
        /// É utilizado durante o login para construir o DTO de resposta final.
        /// </remarks>
        /// <param name="userId">O ID do utilizador (UID).</param>
        /// <returns>O DTO [LoginResponseDto] com os dados do perfil.</returns>
        Task<LoginResponseDto> GetFullUserData(string userId);
    }
}