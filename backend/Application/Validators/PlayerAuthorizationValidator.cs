using Application.Interfaces.Validators;
using Domain.Entities;

namespace Application.Validators
{
    /// <summary>
    /// Validador de Autorização e Controlo de Acesso baseado no papel e contexto de equipa do Jogador.
    /// 
    /// Esta classe é utilizada na camada de Serviço para garantir que o utilizador autenticado (Player) 
    /// tem as permissões mínimas ([IsAdmin], pertença à equipa) para executar uma determinada operação ou aceder a um recurso.
    /// </summary>
    public class PlayerAuthorizationValidator: IPlayerAuthorizationValidator
    {
        #region Validator User Id
       
        /// <summary>
        /// Valida se o ID de utilizador fornecido não é nulo nem vazio.
        /// </summary>
        /// <param name="userId">O ID (string) do utilizador a ser validado.</param>
        /// <exception cref="InvalidOperationException">Lançada se o ID do utilizador for nulo ou vazio.</exception>
        public void ValidateUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("O id do utilizador está inválido.");
            }
        }

        /// <summary>
        /// Valida se o ID do utilizador autenticado é o mesmo ID presente no URL ou nos argumentos do pedido.
        /// </summary>
        /// <remarks>
        /// Usado para prevenir que um utilizador tente aceder a recursos de outro utilizador (proteção contra acesso lateral).
        /// </remarks>
        /// <param name="userId">O ID do utilizador obtido do contexto de autenticação (JWT/Claims).</param>
        /// <param name="userIdUrl">O ID do utilizador obtido do URL ou corpo do pedido.</param>
        /// <exception cref="UnauthorizedAccessException">Lançada se os IDs não coincidirem.</exception>
        public void ValidateUserIdIsSameUrl(string userId, string userIdUrl)
        {
            if (userId != userIdUrl)
            {
                throw new UnauthorizedAccessException("O utilizador que está a tentar entrar não é o mesmo da url.");
            }
        }
        #endregion

        #region Validator Players

        /// <summary>
        /// Valida se o utilizador autenticado tem permissão de Administrador para aceder ao recurso da equipa especificada.
        /// </summary>
        /// <remarks>
        /// Requer: 1. Utilizador existe. 2. ID da equipa válido. 3. Utilizador é Admin (IsAdmin = true). 4. Utilizador pertence à equipa.
        /// </remarks>
        /// <param name="player">A entidade [Player] (carregada com Team ID).</param>
        /// <param name="idTeam">O ID da equipa alvo.</param>
        /// <exception cref="InvalidOperationException">Se o utilizador não for administrador ou não pertencer à equipa.</exception>
        public void ValidatePlayerAutorizationIsAdmin(Player player, Guid idTeam)
        {
            UserExists(player);

            ValidateIdTeam(idTeam);

            if (!player.IsAdmin) {
                throw new InvalidOperationException("Apenas administradores de equipa têm acesso a este recurso.");
            }

            ValidateUserHaveTeamAndIsMember(player, idTeam);
        }

        /// <summary>
        /// Valida se o utilizador autenticado é um Membro NÃO Administrador da equipa alvo.
        /// </summary>
        /// <remarks>
        /// Útil para recursos que são exclusivos para membros regulares, excluindo administradores.
        /// </remarks>
        /// <param name="player">A entidade [Player] (carregada com Team ID).</param>
        /// <param name="idTeam">O ID da equipa alvo.</param>
        /// <exception cref="InvalidOperationException">Se o utilizador for administrador ou não for membro da equipa.</exception>
        public void ValidatePlayerAutorizationIsNotAdmin(Player player, Guid idTeam)
        {
            UserExists(player);

            ValidateIdTeam(idTeam);

            if (player.IsAdmin)
            {
                throw new InvalidOperationException("Apenas utilizadores da equipa têm acesso a este recurso.");
            }

            ValidateUserHaveTeamAndIsMember(player, idTeam);
        }

        /// <summary>
        /// Valida se o utilizador autenticado é um Membro de QUALQUER nível (Admin ou não Admin) da equipa alvo.
        /// </summary>
        /// <remarks>
        /// Requer: 1. Utilizador existe. 2. ID da equipa válido. 3. Utilizador pertence à equipa.
        /// </remarks>
        /// <param name="player">A entidade [Player] (carregada com Team ID).</param>
        /// <param name="idTeam">O ID da equipa alvo.</param>
        /// <exception cref="InvalidOperationException">Se o utilizador não pertencer à equipa.</exception>
        public void ValidatePlayerAutorizationIsMember(Player player, Guid idTeam)
        {
            UserExists(player);

            ValidateIdTeam(idTeam);

            ValidateUserHaveTeamAndIsMember(player, idTeam);
        }

        /// <summary>
        /// Valida se o utilizador autenticado **não pertence a nenhuma equipa**.
        /// </summary>
        /// <remarks>
        /// Útil para rotas de Onboarding ou Criação de Equipa, onde a afiliação é um bloqueio.
        /// </remarks>
        /// <param name="player">A entidade [Player] (carregada com Team ID).</param>
        /// <exception cref="InvalidOperationException">Se o jogador já tiver um ID de equipa.</exception>
        public void ValidatePlayerAutorizationWithoutTeam(Player player)
        {
            UserExists(player);

            if (player.IdTeam != null || player.Team != null)
            {
                throw new InvalidOperationException("Apenas jogadores sem equipa podem aceder a este recurso!");
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Validação de existência: Garante que a entidade de Utilizador não é nula.
        /// </summary>
        /// <param name="user">A entidade de utilizador.</param>
        /// <exception cref="InvalidOperationException">Lançada se o utilizador for nulo.</exception>
        private static void UserExists(User user)
        {
            if (user == null)
            {
                throw new InvalidOperationException("O Utilizador não existe!");
            }
        }

        /// <summary>
        /// Validação de formato: Garante que o ID da equipa não é Guid.Empty.
        /// </summary>
        /// <param name="idTeam">O ID da equipa.</param>
        /// <exception cref="InvalidOperationException">Lançada se o ID for Guid.Empty.</exception>
        private static void ValidateIdTeam(Guid idTeam)
        {
            if (idTeam == Guid.Empty)
            {
                throw new InvalidOperationException("O id da equipa está inválido.");
            }
        }

        /// <summary>
        /// Validação de Membro: Verifica se o jogador pertence à equipa alvo.
        /// </summary>
        /// <remarks>
        /// Requer que o campo [player.IdTeam] não seja nulo e seja igual ao [idTeam] fornecido.
        /// </remarks>
        /// <param name="player">O jogador.</param>
        /// <param name="idTeam">O ID da equipa esperada.</param>
        /// <exception cref="InvalidOperationException">Se o jogador não tiver equipa ou pertencer a uma equipa diferente.</exception>
        private void ValidateUserHaveTeamAndIsMember(Player player, Guid idTeam)
        {
            if (player.IdTeam == null)
            {
                throw new InvalidOperationException("Apenas jogadores com equipa podem aceder a este recurso!");
            }

            if (player.IdTeam != idTeam)
            {
                throw new InvalidOperationException("O Utilizador não tem autorização para aceder a este recurso (não faz parte da equipa).");
            }
        }

        #endregion
    }
}