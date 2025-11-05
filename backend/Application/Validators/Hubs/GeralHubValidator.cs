using Application.Interfaces.Validators.Hub;

namespace Application.Validators.Hubs
{
    public class GeralHubValidator : IGeralHubValidator
    {
        public void ValidateIdMatchLeaveMatch(Guid idMatch, Guid idTeam)
        {
            if (idMatch == Guid.Empty)
            {
                throw new ArgumentNullException("O id da match está vazio");
            }

            if (idTeam == Guid.Empty)
            {
                throw new ArgumentNullException("O id da equipa está vazio");
            }
        }

        public void ValidateLeaveMatch(bool success)
        {
            if (!success)
            {
                throw new InvalidOperationException("Surguiu um problema a sair do lobby");
            }
        }
    }
}
