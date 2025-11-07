using Application.Interfaces.Validators;
using Domain.Entities;

namespace Application.Validators
{
    public class PlayerAuthorizationValidator: IPlayerAuthorizationValidator
    {
        #region Validation User Id
        public void ValidateUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("O id do utilizador está inválido.");
            }
        }

        public void ValidateUserIdIsSameUrl(string userId, string userIdUrl)
        {
            if (userId != userIdUrl)
            {
                throw new UnauthorizedAccessException("O utilizador que está a tentar entrar não é o mesmo da url.");
            }
        }
        #endregion

        #region Validator Players
        public void ValidatePlayerAutorizationIsAdmin(Player player, Guid idTeam)
        {
            UserExists(player);

            ValidateIdTeam(idTeam);

            if (!player.IsAdmin) {
                throw new InvalidOperationException("Apenas administradores de equipa têm acesso a este recurso.");
            }

            ValidateUserHaveTeamAndIsMember(player, idTeam);
        }

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

        public void ValidatePlayerAutorizationIsMember(Player player, Guid idTeam)
        {
            UserExists(player);

            ValidateIdTeam(idTeam);

            ValidateUserHaveTeamAndIsMember(player, idTeam);
        }

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
        private static void UserExists(User user)
        {
            if (user == null)
            {
                throw new InvalidOperationException("O Utilizador não existe!");
            }
        }

        private static void ValidateIdTeam(Guid idTeam)
        {
            if (idTeam == Guid.Empty)
            {
                throw new InvalidOperationException("O id da equipa está inválido.");
            }
        }

        private void ValidateUserHaveTeamAndIsMember(Player player, Guid idTeam)
        {
            if (player.IdTeam != null)
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