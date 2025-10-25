namespace Application.Interfaces.Validators.Hub
{
    public interface IGeralHubValidator
    {
        public void ValidateIdMatchLeaveMatch(Guid idMatch, Guid idTeam);
        public void ValidateLeaveMatch(bool success);
    }
}
