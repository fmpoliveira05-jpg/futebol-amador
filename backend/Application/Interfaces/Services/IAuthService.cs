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
        /// Indica se a API exige o e-mail confirmado (configuração <c>Auth:RequireVerifiedEmail</c>).
        /// </summary>
        bool ExigeEmailVerificado { get; }

        /// <summary>
        /// Realiza o processo de login do utilizador.
        /// </summary>
        /// <remarks>
        /// Autentica o utilizador no fornecedor de identidade e recupera os dados do perfil interno.
        /// Com <see cref="ExigeEmailVerificado"/>, recusa (403, <c>email_nao_verificado</c>) quem ainda
        /// não confirmou o e-mail — só depois de validar a palavra-passe.
        /// </remarks>
        /// <param name="email">O email do utilizador.</param>
        /// <param name="password">A password do utilizador.</param>
        /// <returns>Um DTO [LoginResponseDto] contendo os dados do perfil e os tokens de acesso.</returns>
        Task<LoginResponseDto> LoginAsync(string email, string password);

        /// <summary>
        /// Depois do registo: envia a mensagem de confirmação do e-mail e, se a confirmação não for
        /// exigida, devolve logo a sessão.
        /// </summary>
        /// <returns>A sessão, ou <c>null</c> quando o utilizador tem de confirmar o e-mail primeiro.</returns>
        Task<LoginResponseDto?> IniciarSessaoAposRegistoAsync(string email, string password);

        /// <summary>
        /// Volta a enviar a mensagem de confirmação do e-mail. Não lança erros por credenciais erradas
        /// nem por o e-mail já estar confirmado: a resposta ao cliente é sempre a mesma.
        /// </summary>
        Task ReenviarVerificacaoEmailAsync(string email, string password);

        /// <summary>
        /// Pede ao Firebase o envio da mensagem de recuperação da palavra-passe. Não revela se o
        /// e-mail existe: nunca lança erros por e-mail desconhecido.
        /// </summary>
        Task PedirRecuperacaoPalavraPasseAsync(string email);

        /// <summary>
        /// Troca um refresh token do Firebase por um ID token novo (sessão web).
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Se o refresh token for inválido, expirado ou revogado.</exception>
        Task<FirebaseLoginResponseDto> RenovarSessaoAsync(string refreshToken);

        /// <summary>
        /// Altera a palavra-passe de um utilizador.
        /// </summary>
        /// <remarks>
        /// Requer a senha atual para validação de segurança. O Firebase termina as outras sessões.
        /// </remarks>
        /// <param name="userId">O ID do utilizador alvo.</param>
        /// <param name="currentPassword">A password atual.</param>
        /// <param name="newPassword">A nova password.</param>
        /// <returns>Uma sessão nova, já com a palavra-passe nova (para a web substituir os cookies).</returns>
        Task<FirebaseLoginResponseDto?> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

        /// <summary>
        /// Atualiza o endereço de e-mail de um utilizador, depois de confirmar a palavra-passe atual.
        /// </summary>
        /// <remarks>
        /// O e-mail novo fica por confirmar (é enviada a mensagem de confirmação) e as sessões
        /// existentes são terminadas.
        /// </remarks>
        /// <param name="userId">O ID do utilizador alvo.</param>
        /// <param name="currentPassword">A palavra-passe atual (reautenticação).</param>
        /// <param name="newEmail">O novo endereço de e-mail.</param>
        Task UpdateEmailAsync(string userId, string currentPassword, string newEmail);

        /// <summary>
        /// Termina a sessão do utilizador, revogando os tokens de atualização.
        /// </summary>
        /// <param name="userId">O ID do utilizador a fazer logout.</param>
        Task LogoutAsync(string userId);

        /// <summary>
        /// Elimina permanentemente a conta de um utilizador.
        /// </summary>
        /// <param name="userId">O ID do utilizador a eliminar.</param>
        Task DeleteUserAsync(string userId);

        /// <summary>
        /// Confirma a palavra-passe atual do utilizador (reautenticação antes de operações
        /// irreversíveis, como eliminar a conta). Lança <c>AuthenticationException</c> se estiver errada.
        /// </summary>
        Task ConfirmarPalavraPasseAsync(string userId, string password);

        /// <summary>Dos utilizadores indicados, os que ainda não confirmaram o e-mail (ou já não existem no Firebase).</summary>
        Task<HashSet<string>> ContasPorConfirmarAsync(IReadOnlyCollection<string> userIds);

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
