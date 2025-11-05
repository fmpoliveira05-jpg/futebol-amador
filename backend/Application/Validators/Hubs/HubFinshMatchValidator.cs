using Application.Interfaces.Validators.Hub;

namespace Application.Validators.Hubs
{
    public class HubFinshMatchValidator : IHubFinshMatchValidator
    {
        public void ValidateIsCoincide(bool? isCoincide)
        {
            if (!isCoincide.HasValue)
            {
                throw new ArgumentException("O resultado da partida introduzido pelas duas equipas não coincidem");
            }
        }
    }
}
