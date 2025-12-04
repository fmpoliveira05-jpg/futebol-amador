using Domain.Entities;

namespace Application.Interfaces.Validators
{
    /// <summary>
    /// Contrato de Validador de Regras de Negócio para Autorização e Verificação de Permissões de Jogadores.
    /// 
    /// Esta interface define as regras de validação síncrona de alto nível que garantem o Controlo de Acesso Baseado em Papel (RBAC) 
    /// e a segurança da identidade (ID) do utilizador em relação ao recurso solicitado.
    /// </summary>
    public interface IPlayerAuthorizationValidator
    {
        /// <summary>
        /// Valida se o ID de utilizador fornecido não é nulo nem vazio.
        /// </summary>
        /// <param name="userId">O ID (string) do utilizador a ser validado.</param>
        /// <exception cref="System.InvalidOperationException">Lançada se o ID do utilizador for nulo ou vazio.</exception>
        void ValidateUserId(string userId);

        /// <summary>
        /// Valida se o ID do utilizador autenticado é o mesmo ID presente no URL (evita acesso lateral).
        /// </summary>
        /// <param name="userId">O ID do utilizador obtido do contexto de autenticação (Claims).</param>
        /// <param name="userIdUrl">O ID do utilizador obtido do URL ou corpo do pedido.</param>
        /// <exception cref="System.UnauthorizedAccessException">Lançada se os IDs não coincidirem.</exception>
        public void ValidateUserIdIsSameUrl(string userId, string userIdUrl);

        /// <summary>
        /// Verifica se o utilizador tem permissão de Administrador para aceder ao recurso da equipa alvo.
        /// </summary>
        /// <remarks>
        /// Requer: 1. Utilizador existe. 2. Utilizador é Admin (IsAdmin = true). 3. Utilizador pertence à equipa [idTeam].
        /// </remarks>
        /// <param name="user">A entidade [Player] (carregada com Team ID).</param>
        /// <param name="idTeam">O ID da equipa alvo do recurso (contexto).</param>
        /// <exception cref="System.InvalidOperationException">Se o utilizador não for administrador ou não pertencer à equipa.</exception>
        void ValidatePlayerAutorizationIsAdmin(Player user, Guid idTeam);

        /// <summary>
        /// Verifica se o utilizador é um Membro Regulares da Equipa (excluindo Administradores).
        /// </summary>
        /// <remarks>
        /// O acesso é negado se o utilizador tiver privilégios de Admin.
        /// </remarks>
        /// <param name="user">A entidade [Player] (carregada com Team ID).</param>
        /// <param name="idTeam">O ID da equipa à qual o utilizador deve pertencer.</param>
        /// <exception cref="System.InvalidOperationException">Se o utilizador for administrador ou não for membro da equipa.</exception>
        void ValidatePlayerAutorizationIsNotAdmin(Player user, Guid idTeam);

        /// <summary>
        /// Verifica se o utilizador é um Membro de QUALQUER nível (Admin ou Jogador regular) da equipa alvo.
        /// </summary>
        /// <remarks>
        /// Utilizado para recursos básicos da equipa (ex: Ver Calendário, Lista de Membros).
        /// </remarks>
        /// <param name="user">A entidade [Player] (carregada com Team ID).</param>
        /// <param name="idTeam">O ID da equipa à qual o utilizador deve pertencer.</param>
        /// <exception cref="System.InvalidOperationException">Se o utilizador não pertencer à equipa.</exception>
        void ValidatePlayerAutorizationIsMember(Player user, Guid idTeam);

        /// <summary>
        /// Verifica se o utilizador é um Jogador que **não pertence a nenhuma equipa** (Agente Livre).
        /// </summary>
        /// <remarks>
        /// Utilizado para proteger rotas de Onboarding ou Criação de Equipa (onde a afiliação é um bloqueio).
        /// </remarks>
        /// <param name="user">A entidade [Player] (carregada com Team ID).</param>
        /// <exception cref="System.InvalidOperationException">Se o jogador já tiver uma afiliação (IdTeam) não nula.</exception>
        void ValidatePlayerAutorizationWithoutTeam(Player user);
    }
}