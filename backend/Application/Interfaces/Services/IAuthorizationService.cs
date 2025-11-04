namespace Application.Interfaces.Services
{
    public interface IAuthorizationService
    {
        Task UserAuthorizationIsAdminTeamById(string userId, Guid idTeam);
        Task UserAuthorizationIsMemberTeamById(string userId, Guid idTeam);
        Task UserAuthorizationIsPlayerWithoutTeamById(string userId);
    }
}
