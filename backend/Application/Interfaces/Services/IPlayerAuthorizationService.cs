namespace Application.Interfaces.Services
{
    public interface IPlayerAuthorizationService
    {
        Task UserAuthorizationIsAdminTeamById(string userId, Guid idTeam);
        Task UserAuthorizationIsMemberTeamById(string userId, Guid idTeam);
        Task UserAuthorizationIsMemberTeamNotAdminById(string userId, Guid idTeam);
        Task UserAuthorizationIsPlayerWithoutTeamById(string userId);
    }
}
