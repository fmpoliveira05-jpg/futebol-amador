using Application.DTOs.Match;
using Application.DTOs.MatchInvites;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Unit.ApplicationTests.ServicesTests
{
    [TestFixture]
    public class MatchInviteServiceTests
    {
        #region Variables
        private Mock<ITeamRepository> _teamRepoMock;
        private Mock<IMatchInviteRepository> _matchInviteRepoMock;
        private Mock<IMatchRepository> _matchRepoMock;
        private Mock<ITeamStatisticsRepository> _teamStatsRepoMock;
        private Mock<IPitchRepository> _pitchRepoMock;
        private Mock<IUnityOfWork> _unitOfWorkMock;
        private Mock<IMatchInviteValidator> _validatorMock;
        private Mock<ITeamPostPoneGameRepository> _teamPostPoneRepoMock;
        private Mock<IPlayerAuthorizationService> _authorizationService;
        private MatchInviteService _sut;
        private Rank _defaultRank;
        #endregion

        #region SetUp
        [SetUp]
        public void SetUp()
        {
            _teamRepoMock = new Mock<ITeamRepository>();
            _matchInviteRepoMock = new Mock<IMatchInviteRepository>();
            _matchRepoMock = new Mock<IMatchRepository>();
            _teamStatsRepoMock = new Mock<ITeamStatisticsRepository>();
            _pitchRepoMock = new Mock<IPitchRepository>();
            _unitOfWorkMock = new Mock<IUnityOfWork>();
            _validatorMock = new Mock<IMatchInviteValidator>();
            _teamPostPoneRepoMock = new Mock<ITeamPostPoneGameRepository>();
            _authorizationService = new Mock<IPlayerAuthorizationService>();

            _sut = new MatchInviteService(
                _matchInviteRepoMock.Object, 
                _teamRepoMock.Object,          
                _matchRepoMock.Object,         
                _teamStatsRepoMock.Object,    
                _pitchRepoMock.Object,          
                _validatorMock.Object,
                _unitOfWorkMock.Object,
                _teamPostPoneRepoMock.Object,
                _authorizationService.Object
            );

            // rank mínimo válido para testes
            _defaultRank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
        }
        #endregion

        #region Tests

        #region SendMatchInviteTests
        [Test(Description = "T1GP1 - Player Admin de Equipa envia um convite de partida casual para outra Equipa.")]
        public async Task SendMatchInvite_Should_CreateInvite_When_AdminPlayer()
        {
            // ARRANGE
            var userId = "admin-id";
            var idSender = Guid.NewGuid();
            var idReceiver = Guid.NewGuid();

            var senderPitch = new Pitch("Campo Central", "Rua A");
            var receiverPitch = new Pitch("Campo Secundário", "Rua B");

            var senderTeam = new Team("Team A", "Descrição A", new byte[] { 1 }, senderPitch, _defaultRank);
            var receiverTeam = new Team("Team B", "Descrição B", new byte[] { 1 }, receiverPitch, _defaultRank);

            var dto = new SendMatchInviteDto
            {
                IdReceiver = idReceiver,
                GameDate = DateTime.UtcNow.AddDays(1),
                namePitch = senderPitch.Name
            };

            _teamRepoMock.Setup(r => r.GetTeamByIdWithPitchAsync(idSender)).ReturnsAsync(senderTeam);
            _teamRepoMock.Setup(r => r.GetTeamByIdWithPitchAsync(idReceiver)).ReturnsAsync(receiverTeam);
            _matchInviteRepoMock.Setup(r => r.GetMatchInvite(idSender, idReceiver, It.IsAny<DateTime>()))
                                .ReturnsAsync((MatchInvite)null);
            _matchRepoMock.Setup(r => r.GetMatchProxim12HoursMatchs(idSender, It.IsAny<DateTime>()))
                          .ReturnsAsync((Matches)null);

            // ACT
            var result = await _sut.SendMatchInvite(idSender, dto);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().BeOfType<InfoMatchInviteDto>();
            result.NamePitch.Should().Be("Campo Central");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _matchInviteRepoMock.Verify(r => r.AddMatchInvite(It.IsAny<MatchInvite>()), Times.Once);
        }

        [Test(Description = "T2GP1 - Player não Admin tenta enviar convite de partida casual para outra equipa.")]
        public async Task SendMatchInvite_Should_Throw_When_PlayerIsNotAdmin()
        {
            // ARRANGE
            var userId = "admin-id";
            var idSender = Guid.NewGuid();
            var dto = new SendMatchInviteDto
            {
                IdReceiver = Guid.NewGuid(),
                GameDate = DateTime.UtcNow.AddDays(2),
                namePitch = "Campo A"
            };

            _validatorMock
                .Setup(v => v.ValidateSenderMatchInvite(dto, idSender))
                .Throws(new ValidationException("O jogador não é administrador da equipa."));

            // ACT
            Func<Task> act = async () => await _sut.SendMatchInvite(idSender, dto);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("O jogador não é administrador da equipa.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T3GP1 - Player não pertencente à equipa A tenta enviar convite de partida casual para a equipa B.")]
        public async Task SendMatchInvite_Should_Throw_When_PlayerDoesNotBelongToTeam()
        {
            // ARRANGE
            var userId = "admin-id";
            var idSender = Guid.NewGuid();
            var dto = new SendMatchInviteDto
            {
                IdReceiver = Guid.NewGuid(),
                GameDate = DateTime.UtcNow.AddDays(3),
                namePitch = "Campo B"
            };

            _validatorMock
                .Setup(v => v.ValidateSenderMatchInvite(dto, idSender))
                .Throws(new ValidationException("O jogador não pertence à equipa."));

            // ACT
            Func<Task> act = async () => await _sut.SendMatchInvite(idSender, dto);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("O jogador não pertence à equipa.");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T4GP1 - Envio de convite para equipa que não existe.")]
        public async Task SendMatchInvite_Should_Throw_When_ReceiverTeamDoesNotExist()
        {
            // ARRANGE
            var userId = "admin-id";
            var idSender = Guid.NewGuid();
            var dto = new SendMatchInviteDto
            {
                IdReceiver = Guid.NewGuid(),
                GameDate = DateTime.UtcNow.AddDays(1),
                namePitch = "Campo Central"
            };

            var senderTeam = new Team("Team Sender", "Desc", new byte[] { 1 }, new Pitch("Campo Central", "Rua X"), _defaultRank);
            _teamRepoMock.Setup(r => r.GetTeamByIdWithPitchAsync(idSender)).ReturnsAsync(senderTeam);
            _teamRepoMock.Setup(r => r.GetTeamByIdWithPitchAsync(dto.IdReceiver))
                         .ReturnsAsync((Team)null!);

            _validatorMock
                .Setup(v => v.ValidateSendMatchInvite(null, It.IsAny<Team>(), null, null, dto.namePitch))
                .Throws(new ValidationException("A equipa de destino não existe."));

            // ACT
            Func<Task> act = async () => await _sut.SendMatchInvite(idSender, dto);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("A equipa de destino não existe.");
            _matchInviteRepoMock.Verify(r => r.AddMatchInvite(It.IsAny<MatchInvite>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #region AcceptMatchInviteTests
        [Test(Description = "T1GP2 - Player Admin aceita convite de partida casual enviado de outra equipa.")]
        public async Task AcceptMatchInvite_Should_CreateMatch_When_AdminTeamAccepts()
        {
            // ARRANGE
            var userId = "admin-id";
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var pitch = new Pitch("Campo Central", "Rua Principal");

            var senderTeam = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var receiverTeam = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            var matchInvite = new MatchInvite(senderTeam, receiverTeam, DateTime.UtcNow.AddDays(1), pitch);

            receiverTeam.ReceivedInvites.Add(matchInvite);

            _matchInviteRepoMock.Setup(r => r.GetMatchInviteById(matchInvite.Id))
                                .ReturnsAsync(matchInvite);

            _teamRepoMock.Setup(r => r.GetByIdWithReceivedInvitesAndCalendar(receiverTeam.Id))
                         .ReturnsAsync(receiverTeam);
            _teamRepoMock.Setup(r => r.GetTeamByIdAsync(senderTeam.Id))
                         .ReturnsAsync(senderTeam);
            _pitchRepoMock.Setup(r => r.GetPitchById(matchInvite.IdPitch))
                          .ReturnsAsync(pitch);
            _matchRepoMock.Setup(r => r.GetMatchProxim12HoursMatchs(receiverTeam.Id, It.IsAny<DateTime>()))
                          .ReturnsAsync((Matches?)null);

            _validatorMock.Setup(v => v.ValidateAcceptRefuseMatchInvite(receiverTeam.Id, matchInvite.Id));
            _validatorMock.Setup(v => v.ValidateReciever(receiverTeam));
            _validatorMock.Setup(v => v.ValidateMatchInvite(matchInvite));
            _validatorMock.Setup(v => v.ValidateAcceptMatchInvite(senderTeam, null, matchInvite, pitch));

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // ACT
            var result = await _sut.AcceptMatchInvite(receiverTeam.Id, matchInvite.Id);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().BeOfType<MatchDto>();
            result.NameTeam.Should().Be("Team B");
            result.NameOpponent.Should().Be("Team A");

            _matchInviteRepoMock.Verify(r => r.DeleteMatchInvite(matchInvite), Times.Once);
            _matchRepoMock.Verify(r => r.AddMatch(It.IsAny<Matches>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "T2GP2 - Player não Admin de equipa tenta aceitar convite de partida.")]
        public async Task AcceptMatchInvite_Should_Throw_When_TeamIsInvalidOrNotAdmin()
        {
            // ARRANGE
            var userId = "admin-id";
            var idTeam = Guid.NewGuid();
            var idMatchInvite = Guid.NewGuid();

            _validatorMock
                .Setup(v => v.ValidateAcceptRefuseMatchInvite(idTeam, idMatchInvite))
                .Throws(new ValidationException("A equipa não é administradora ou não tem permissão para aceitar o convite."));

            // ACT
            Func<Task> act = async () => await _sut.AcceptMatchInvite(idTeam, idMatchInvite);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administradora*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T3GP2 - Player não pertencente à equipa A tenta aceitar convite de partida que a equipa B mandou.")]
        public async Task AcceptMatchInvite_Should_Throw_When_TeamDoesNotBelongToInvite()
        {
            // ARRANGE
            var userId = "admin-id";
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var pitch = new Pitch("Campo", "Rua");
            var teamA = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var teamB = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            var outsiderTeam = new Team("Team Outsider", "desc", new byte[] { 3 }, pitch, rank);

            var invite = new MatchInvite(teamA, teamB, DateTime.UtcNow.AddDays(1), pitch);
            teamB.ReceivedInvites.Add(invite);

            _teamRepoMock.Setup(r => r.GetByIdWithReceivedInvitesAndCalendar(outsiderTeam.Id))
                         .ReturnsAsync(outsiderTeam);

            _validatorMock
                .Setup(v => v.ValidateAcceptRefuseMatchInvite(outsiderTeam.Id, invite.Id))
                .Throws(new ValidationException("A equipa não recebeu este convite."));

            // ACT
            Func<Task> act = async () => await _sut.AcceptMatchInvite(outsiderTeam.Id, invite.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não recebeu este convite*");
            _matchRepoMock.Verify(r => r.AddMatch(It.IsAny<Matches>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #region RefuseMatchInvitesTests
        [Test(Description = "T1GP3 - Player Admin rejeita convite de partida casual.")]
        public async Task RefuseMatchInvites_Should_RemoveInvite_When_AdminTeamRejects()
        {
            // ARRANGE
            var userId = "admin-id";
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var pitch = new Pitch("Campo Central", "Rua A");

            var senderTeam = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var receiverTeam = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            var matchInvite = new MatchInvite(senderTeam, receiverTeam, DateTime.UtcNow.AddDays(1), pitch);

            receiverTeam.ReceivedInvites.Add(matchInvite);

            _teamRepoMock.Setup(r => r.GetByIdWithReceivedInvites(receiverTeam.Id))
                         .ReturnsAsync(receiverTeam);
            _teamRepoMock.Setup(r => r.GetTeamByIdAsync(senderTeam.Id))
                         .ReturnsAsync(senderTeam);

            _validatorMock.Setup(v => v.ValidateAcceptRefuseMatchInvite(receiverTeam.Id, matchInvite.Id));
            _validatorMock.Setup(v => v.ValidateReciever(receiverTeam));
            _validatorMock.Setup(v => v.ValidateMatchInvite(matchInvite));
            _validatorMock.Setup(v => v.ValidateRefuseMatchInvite(senderTeam, matchInvite));

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // ACT
            Func<Task> act = async () => await _sut.RefuseMatchInvites(receiverTeam.Id, matchInvite.Id);

            // ASSERT
            await act.Should().NotThrowAsync();
            _matchInviteRepoMock.Verify(r => r.DeleteMatchInvite(matchInvite), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "T2GP3 - Player não Admin tenta rejeitar convite.")]
        public async Task RefuseMatchInvites_Should_Throw_When_TeamIsNotAdmin()
        {
            // ARRANGE
            var userId = "admin-id";
            var idTeam = Guid.NewGuid();
            var idMatchInvite = Guid.NewGuid();

            _validatorMock
                .Setup(v => v.ValidateAcceptRefuseMatchInvite(idTeam, idMatchInvite))
                .Throws(new ValidationException("A equipa não tem permissão para rejeitar o convite."));

            // ACT
            Func<Task> act = async () => await _sut.RefuseMatchInvites(idTeam, idMatchInvite);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não tem permissão*");
            _matchInviteRepoMock.Verify(r => r.DeleteMatchInvite(It.IsAny<MatchInvite>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T3GP3 - Player não pertencente à equipa A tenta rejeitar convite que a equipa B mandou.")]
        public async Task RefuseMatchInvites_Should_Throw_When_TeamDoesNotBelongToInvite()
        {
            // ARRANGE
            var userId = "admin-id";
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var pitch = new Pitch("Campo", "Rua");
            var teamA = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var teamB = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            var outsiderTeam = new Team("Team Outsider", "desc", new byte[] { 3 }, pitch, rank);

            var invite = new MatchInvite(teamA, teamB, DateTime.UtcNow.AddDays(1), pitch);
            teamB.ReceivedInvites.Add(invite);

            _validatorMock
                .Setup(v => v.ValidateAcceptRefuseMatchInvite(outsiderTeam.Id, invite.Id))
                .Throws(new ValidationException("A equipa não recebeu este convite."));

            _teamRepoMock.Setup(r => r.GetByIdWithReceivedInvites(outsiderTeam.Id))
                         .ReturnsAsync(outsiderTeam);

            // ACT
            Func<Task> act = async () => await _sut.RefuseMatchInvites(outsiderTeam.Id, invite.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não recebeu este convite*");
            _matchInviteRepoMock.Verify(r => r.DeleteMatchInvite(It.IsAny<MatchInvite>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #region NegociateMatchInviteTests
        [Test(Description = "T1GP4 - Player Admin de equipa envia proposta de negociação de convite.")]
        public async Task NegociateMatchInvite_Should_ReturnDto_When_AdminTeamNegotiates()
        {
            // ARRANGE
            var userId = "admin-id";
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var pitch = new Pitch("Campo Central", "Rua A");
            var senderTeam = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var receiverTeam = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            var matchInvite = new MatchInvite(senderTeam, receiverTeam, DateTime.UtcNow.AddDays(1), pitch);
            var newDate = DateTime.UtcNow.AddDays(2);
            var dto = new SendMatchInviteDto
            {
                IdReceiver = receiverTeam.Id,
                GameDate = newDate,
                namePitch = pitch.Name
            };
            _matchInviteRepoMock.Setup(r => r.GetMatchInviteWithPitchByTeams(senderTeam.Id, receiverTeam.Id))
                                .ReturnsAsync(matchInvite);
            _matchRepoMock.Setup(r => r.GetMatchProxim12HoursMatchs(receiverTeam.Id, newDate))
                          .ReturnsAsync((Matches?)null);

            _validatorMock.Setup(v => v.ValidateSenderMatchInvite(dto, senderTeam.Id));
            _validatorMock.Setup(v => v.ValidateNegociateMatchInvite(
                dto.namePitch, matchInvite.Pitch, matchInvite, senderTeam, receiverTeam, null));
            _validatorMock.Setup(v => v.ValidateHasChangeNegociateMatchInvite(It.IsAny<bool>()));

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // ACT
            var result = await _sut.NegociateMatchInvite(senderTeam.Id, dto);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().BeOfType<InfoMatchInviteDto>();
            result.NameSender.Should().Be("Team A");
            result.NameReceiver.Should().Be("Team B");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "T2GP4 - Player não admin da equipa tenta negociar convite.")]
        public async Task NegociateMatchInvite_Should_Throw_When_TeamIsNotAdmin()
        {
            // ARRANGE
            var userId = "admin-id";
            var idSender = Guid.NewGuid();
            var dto = new SendMatchInviteDto
            {
                IdReceiver = Guid.NewGuid(),
                GameDate = DateTime.UtcNow.AddDays(1),
                namePitch = "Campo Central"
            };
            _validatorMock
                .Setup(v => v.ValidateSenderMatchInvite(dto, idSender))
                .Throws(new ValidationException("A equipa não é administradora para negociar convites."));

            // ACT
            Func<Task> act = async () => await _sut.NegociateMatchInvite(idSender, dto);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administradora*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T3GP4 - Player não pertencente à equipa A tenta negociar convite que a equipa B mandou.")]
        public async Task NegociateMatchInvite_Should_Throw_When_TeamDoesNotBelongToInvite()
        {
            // ARRANGE
            var userId = "admin-id";
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var pitch = new Pitch("Campo", "Rua");
            var teamA = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var teamB = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            var outsiderTeam = new Team("Team Outsider", "desc", new byte[] { 3 }, pitch, rank);
            var invite = new MatchInvite(teamA, teamB, DateTime.UtcNow.AddDays(1), pitch);
            var dto = new SendMatchInviteDto
            {
                IdReceiver = teamB.Id,
                GameDate = DateTime.UtcNow.AddDays(2),
                namePitch = pitch.Name
            };
            _validatorMock
                .Setup(v => v.ValidateSenderMatchInvite(dto, outsiderTeam.Id))
                .Throws(new ValidationException("A equipa não pertence ao convite a negociar."));

            // ACT
            Func<Task> act = async () => await _sut.NegociateMatchInvite(outsiderTeam.Id, dto);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence ao convite*");
            _matchInviteRepoMock.Verify(r => r.GetMatchInviteWithPitchByTeams(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #endregion
    }
}