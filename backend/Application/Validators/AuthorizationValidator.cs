using Application.Interfaces.Validators;
using Domain.Entities;

namespace Application.Validators
{
    public class AuthorizationValidator: IAuthorizationValidator
    {
        public void ValidateUserId(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("O id do utilizador está inválido.");
            }
        }

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
                throw new InvalidOperationException("Apenas utiliazdores da equipa têm acesso a este recurso.");
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

            if (player.IdTeam != null)
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