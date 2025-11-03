using Application.Interfaces.Repositories;
using Application.Services;
using Moq;
using NUnit.Framework;

namespace Tests.Unit.ApplicationTests.ServicesTests
{
    [TestFixture]
    public class MembershipServiceTests
    {
        private Mock<IPlayerRepository> _playerRepositoryMock;
        private Mock<ITeamRepository> _teamRepositoryMock;
        private Mock<IMembershipRequestRepository> _membershipRepositoryMock;
        private Mock<IUnityOfWork> _unityOfWorkMock;
        private MembershipService _membershipService;

        [SetUp]
        public void SetUp()
        {
            _playerRepositoryMock = new Mock<IPlayerRepository>();
            _teamRepositoryMock = new Mock<ITeamRepository>();
            _membershipRepositoryMock = new Mock<IMembershipRequestRepository>();
            _unityOfWorkMock = new Mock<IUnityOfWork>();

            _membershipService = new MembershipService(
                _playerRepositoryMock.Object,
                _teamRepositoryMock.Object,
                _membershipRepositoryMock.Object,
                _unityOfWorkMock.Object
                );
        }
    }
}
