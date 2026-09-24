namespace Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de Serviço de Domínio para a Autorização e Verificação de Permissões de Jogadores.
    /// 
    /// Esta interface define os métodos de autorização baseados no papel e na afiliação à equipa do utilizador autenticado,
    /// garantindo que as regras de acesso são aplicadas na camada de serviço.
    /// </summary>
    public interface IPlayerAuthorizationService
    {
        /// <summary>
        /// Verifica se o utilizador tem permissão de Administrador para aceder ao recurso da equipa alvo.
        /// </summary>
        /// <remarks>
        /// A autorização falha se o utilizador não pertencer à equipa ou se pertencer mas não tiver privilégios de Admin.
        /// </remarks>
        /// <param name="userId">O ID do utilizador autenticado.</param>
        /// <param name="idTeam">O ID da equipa alvo do recurso (contexto).</param>
        /// <returns>Uma Tarefa (<see cref="Task"/>). Lança uma exceção se a autorização falhar.</returns>
        Task UserAuthorizationIsAdminTeamById(string userId, Guid idTeam);

        /// <summary>
        /// Verifica se o utilizador é um Membro de QUALQUER nível (Admin ou Jogador regular) da equipa alvo.
        /// </summary>
        /// <param name="userId">O ID do utilizador autenticado.</param>
        /// <param name="idTeam">O ID da equipa à qual o utilizador deve pertencer.</param>
        /// <returns>Uma Tarefa (<see cref="Task"/>). Lança uma exceção se não for membro da equipa.</returns>
        Task UserAuthorizationIsMemberTeamById(string userId, Guid idTeam);

        /// <summary>
        /// Verifica se o utilizador é um Membro Regulares da Equipa (excluindo Administradores).
        /// </summary>
        /// <param name="userId">O ID do utilizador autenticado.</param>
        /// <param name="idTeam">O ID da equipa à qual o utilizador deve pertencer.</param>
        /// <returns>Uma Tarefa (<see cref="Task"/>). Lança uma exceção se for Admin ou não for membro.</returns>
        Task UserAuthorizationIsMemberTeamNotAdminById(string userId, Guid idTeam);

        /// <summary>
        /// Verifica se o utilizador é um Jogador que **não pertence a nenhuma equipa** (Agente Livre).
        /// </summary>
        /// <remarks>
        /// Utilizado para proteger rotas de Criação de Equipa ou Candidatura.
        /// </remarks>
        /// <param name="userId">O ID do utilizador autenticado.</param>
        /// <returns>Uma Tarefa (<see cref="Task"/>). Lança uma exceção se o utilizador tiver uma afiliação não nula.</returns>
        Task UserAuthorizationIsPlayerWithoutTeamById(string userId);
    }
}