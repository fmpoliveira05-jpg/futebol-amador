using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;

namespace Application.Services
{
    /// <summary>
    /// Serviço de domínio responsável pela verificação de permissões e autorização de jogadores.
    /// 
    /// Esta classe é utilizada para implementar o Controlo de Acesso Baseado em Papel (RBAC),
    /// validando se o utilizador autenticado é um Administrador, Membro, ou se não tem equipa,
    /// antes de permitir a execução de uma operação na camada de serviço.
    /// </summary>
    public class PlayerAuthorizationService: IPlayerAuthorizationService
    {
        #region Initializer
        private readonly IPlayerRepository PlayerRepository;
        private readonly ISuperAdminRepository SuperAdminRepository;
        private readonly IPlayerAuthorizationValidator AuthorizationValidator;

        /// <summary>
        /// Construtor do PlayerAuthorizationService.
        /// </summary>
        /// <param name="playerRepository">Repositório de Jogadores para buscar a entidade principal.</param>
        /// <param name="SuperAdminRepository">Repositório de Super Administradores (para verificações de contexto).</param>
        /// <param name="authorizationValidator">Validador de Controlo de Acesso que contém as regras de permissão.</param>
        public PlayerAuthorizationService(IPlayerRepository playerRepository, 
            ISuperAdminRepository SuperAdminRepository,
            IPlayerAuthorizationValidator authorizationValidator)
        {
            this.PlayerRepository = playerRepository;
            this.SuperAdminRepository = SuperAdminRepository;
            this.AuthorizationValidator = authorizationValidator;
        }
        #endregion

        #region Player Authorization

        /// <summary>
        /// Autoriza o acesso a um recurso que é exclusivo para Administradores de Equipa.
        /// </summary>
        /// <remarks>
        /// O acesso é negado se o utilizador não for administrador da [idTeam] especificada.
        /// </remarks>
        /// <param name="userId">O ID do utilizador autenticado.</param>
        /// <param name="idTeam">O ID da equipa alvo do recurso (contexto).</param>
        /// <returns>Uma Tarefa (<see cref="Task"/>). Lança uma exceção se a autorização falhar.</returns>
        public async Task UserAuthorizationIsAdminTeamById(string userId, Guid idTeam)
        {
            var user = await GetPlayerById(userId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(user, idTeam);
        }

        /// <summary>
        /// Autoriza o acesso a um recurso que é acessível por qualquer Membro de uma Equipa (Admin ou Jogador).
        /// </summary>
        /// <param name="userId">O ID do utilizador autenticado.</param>
        /// <param name="idTeam">O ID da equipa à qual o utilizador deve pertencer.</param>
        /// <returns>Uma Tarefa (<see cref="Task"/>). Lança uma exceção se não for membro da equipa.</returns>
        public async Task UserAuthorizationIsMemberTeamById(string userId, Guid idTeam)
        {
            var user = await GetPlayerById(userId);
            AuthorizationValidator.ValidatePlayerAutorizationIsMember(user, idTeam);
        }

        /// <summary>
        /// Autoriza o acesso a um recurso que é acessível apenas por Membros Regulares (excluindo Administradores).
        /// </summary>
        /// <param name="userId">O ID do utilizador autenticado.</param>
        /// <param name="idTeam">O ID da equipa à qual o utilizador deve pertencer.</param>
        /// <returns>Uma Tarefa (<see cref="Task"/>). Lança uma exceção se o utilizador for Admin ou não for membro.</returns>
        public async Task UserAuthorizationIsMemberTeamNotAdminById(string userId, Guid idTeam)
        {
            var user = await GetPlayerById(userId);
            AuthorizationValidator.ValidatePlayerAutorizationIsNotAdmin(user, idTeam);
        }

        /// <summary>
        /// Autoriza o acesso a um recurso que é exclusivo para Jogadores sem equipa (Agentes Livres).
        /// </summary>
        /// <remarks>
        /// Utilizado para proteger rotas de Criação/Procura de Equipa.
        /// </remarks>
        /// <param name="userId">O ID do utilizador autenticado.</param>
        /// <returns>Uma Tarefa (<see cref="Task"/>). Lança uma exceção se o utilizador tiver uma afiliação (IdTeam) não nula.</returns>
        public async Task UserAuthorizationIsPlayerWithoutTeamById(string userId)
        {
            var user = await GetPlayerById(userId);
            AuthorizationValidator.ValidatePlayerAutorizationWithoutTeam(user);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Obtém a entidade [Player] pelo seu ID, após validar que o ID não é vazio.
        /// </summary>
        /// <param name="userId">O ID (string) do utilizador.</param>
        /// <returns>A entidade [Player].</returns>
        private async Task<Player> GetPlayerById(string userId)
        {
            AuthorizationValidator.ValidateUserId(userId);
            return await PlayerRepository.GetPlayerByIdAsync(userId);
        }

        /// <summary>
        /// Obtém a entidade [SuperAdmin] pelo seu ID, após validar que o ID não é vazio.
        /// </summary>
        /// <remarks>
        /// Este método é para contexto de Super Admin, mas a autorização de Player usa apenas o [GetPlayerById].
        /// </remarks>
        /// <param name="userId">O ID (string) do utilizador.</param>
        /// <returns>A entidade [SuperAdmin].</returns>
        private async Task<SuperAdmin> GetSuperAdminById(string userId)
        {
            AuthorizationValidator.ValidateUserId(userId);
            return await SuperAdminRepository.GetSuperAdminByIdAsync(userId);
        }

        #endregion
    }
}