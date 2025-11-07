using Application.Interfaces.Repositories;
using Application.Interfaces.Validators;
using Application.Services;
using Domain.Entities;
using Moq;
using NUnit.Framework;

namespace Tests.Unit.ApplicationTests.ServicesTests
{
    public class PlayerAuthorizationServiceTests
    {
        #region Initializer
        private Mock<IPlayerRepository> playerRepoMock;
        private Mock<ISuperAdminRepository> superAdminRepoMock;
        private Mock<IPlayerAuthorizationValidator> validatorMock;

        private PlayerAuthorizationService service;
        #endregion

        #region SetUp
        [SetUp]
        public void Setup()
        {
            playerRepoMock = new Mock<IPlayerRepository>();
            superAdminRepoMock = new Mock<ISuperAdminRepository>();
            validatorMock = new Mock<IPlayerAuthorizationValidator>();

            service = new PlayerAuthorizationService(
                playerRepoMock.Object,
                superAdminRepoMock.Object,
                validatorMock.Object
            );
        }
        #endregion

        #region Support Methods
        private Player BuildPlayer(string id, Guid? teamId = null, bool isAdmin = false)
        {
            return new Player
            {
                Id = id,
                IdTeam = teamId,
                IsAdmin = isAdmin
            };
        }
        #endregion

        #region Tests

        #region Tests UserAuthorizationIsAdminTeamById
        [Test]
        public async Task UserAuthorizationIsAdminTeamById_Success()
        {
            var player = BuildPlayer("user1", Guid.NewGuid(), true);

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id))
                .ReturnsAsync(player);

            await service.UserAuthorizationIsAdminTeamById(player.Id, player.IdTeam.Value);

            validatorMock.Verify(v => v.ValidateUserId(player.Id), Times.Once);
            validatorMock.Verify(v => v.ValidatePlayerAutorizationIsAdmin(player, player.IdTeam.Value), Times.Once);
        }

        [Test]
        public void UserAuthorizationIsAdminTeamById_InvalidId_Throws()
        {
            validatorMock.Setup(v => v.ValidateUserId(null))
                .Throws(new ArgumentException("Invalid Id"));

            Assert.ThrowsAsync<ArgumentException>(() =>
                service.UserAuthorizationIsAdminTeamById(null, Guid.NewGuid())
            );
        }

        #endregion

        #region Tests UserAuthorizationIsMemberTeamById
     
        [Test]
        public async Task UserAuthorizationIsMemberTeamById_Success()
        {
            // ARRANGE
            var player = BuildPlayer("user2", Guid.NewGuid(), false);

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id))
                .ReturnsAsync(player);

            // ACT
            await service.UserAuthorizationIsMemberTeamById(player.Id, player.IdTeam.Value);

            // ASSERT
            validatorMock.Verify(v => v.ValidateUserId(player.Id), Times.Once);

            // corrigido: o método do serviço chama ValidatePlayerAutorizationIsMember(...)
            validatorMock.Verify(v => v.ValidatePlayerAutorizationIsMember(player, player.IdTeam.Value), Times.Once);
        }
        [Test]
        public void UserAuthorizationIsMemberTeamById_ValidatorThrows_Propagates()
        {
            validatorMock.Setup(v => v.ValidateUserId("bad-id"))
                .Throws(new ArgumentException("Invalid user"));

            Assert.ThrowsAsync<ArgumentException>(() =>
                service.UserAuthorizationIsMemberTeamById("bad-id", Guid.NewGuid())
            );
        }
        #endregion

        #region Tests UserAuthorizationIsPlayerWithoutTeamById

        [Test]
        public async Task UserAuthorizationIsPlayerWithoutTeamById_Success()
        {
            var player = BuildPlayer("user3", null);

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id))
                .ReturnsAsync(player);

            await service.UserAuthorizationIsPlayerWithoutTeamById(player.Id);

            validatorMock.Verify(v => v.ValidateUserId(player.Id), Times.Once);
            validatorMock.Verify(v => v.ValidatePlayerAutorizationWithoutTeam(player), Times.Once);
        }

        [Test]
        public void UserAuthorizationIsPlayerWithoutTeamById_NullUserId_Throws()
        {
            validatorMock.Setup(v => v.ValidateUserId(null))
                .Throws(new ArgumentNullException("userId"));

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.UserAuthorizationIsPlayerWithoutTeamById(null)
            );
        }

        #endregion

        #endregion
    }
}
