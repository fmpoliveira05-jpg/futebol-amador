using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;

namespace Application.Services
{
    public class PlayerAuthorizationService: IPlayerAuthorizationService
    {
        #region Initializer
        private readonly IPlayerRepository PlayerRepository;
        private readonly ISuperAdminRepository SuperAdminRepository;
        private readonly IPlayerAuthorizationValidator AuthorizationValidator;

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
        public async Task UserAuthorizationIsAdminTeamById(string userId, Guid idTeam)
        {
            var user = await GetPlayerById(userId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(user, idTeam);
        }

        public async Task UserAuthorizationIsMemberTeamById(string userId, Guid idTeam)
        {
            var user = await GetPlayerById(userId);
            AuthorizationValidator.ValidatePlayerAutorizationIsMember(user, idTeam);
        }

        public async Task UserAuthorizationIsMemberTeamNotAdminById(string userId, Guid idTeam)
        {
            var user = await GetPlayerById(userId);
            AuthorizationValidator.ValidatePlayerAutorizationIsNotAdmin(user, idTeam);
        }

        public async Task UserAuthorizationIsPlayerWithoutTeamById(string userId)
        {
            var user = await GetPlayerById(userId);
            AuthorizationValidator.ValidatePlayerAutorizationWithoutTeam(user);
        }

        #endregion

        #region Private Methods
        private async Task<Player> GetPlayerById(string userId)
        {
            AuthorizationValidator.ValidateUserId(userId);
            return await PlayerRepository.GetPlayerByIdAsync(userId);
        }

        private async Task<SuperAdmin> GetSuperAdminById(string userId)
        {
            AuthorizationValidator.ValidateUserId(userId);
            return await SuperAdminRepository.GetSuperAdminByIdAsync(userId);
        }

        #endregion
    }
}
