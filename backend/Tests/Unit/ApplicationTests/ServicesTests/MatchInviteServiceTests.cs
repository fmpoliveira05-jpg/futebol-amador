using Application.DTOs.Match;
using Application.DTOs.MatchInvites;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
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
        private Mock<INotificationService> _notificationServiceMock;
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
            _notificationServiceMock = new Mock<INotificationService>();

            _sut = new MatchInviteService(
                _matchInviteRepoMock.Object, 
                _teamRepoMock.Object,          
                _matchRepoMock.Object,         
                _teamStatsRepoMock.Object,    
                _pitchRepoMock.Object,          
                _validatorMock.Object,
                _unitOfWorkMock.Object,
                _teamPostPoneRepoMock.Object,
                _authorizationService.Object,
                _notificationServiceMock.Object
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
            receiverTeam.Members = new List<Player>
            {
                new Player { Id = "admin-receiver", Name = "Admin B", IsAdmin = true }
            };

            var dto = new SendMatchInviteDto
            {
                IdSender = idSender,
                IdReceiver = idReceiver,
                GameDate = DateTime.UtcNow.AddDays(1),
                homePitch = true
            };

            _teamRepoMock.Setup(r => r.GetTeamByIdWithPitchAsync(idSender)).ReturnsAsync(senderTeam);
            _teamRepoMock.Setup(r => r.GetTeamByIdWithPitchAsync(idReceiver)).ReturnsAsync(receiverTeam);

            _matchInviteRepoMock.Setup(r => r.GetMatchInvite(idSender, idReceiver, It.IsAny<DateTime>()))
                                .ReturnsAsync((MatchInvite)null);

            _matchRepoMock.Setup(r => r.GetMatchProxim12HoursMatchs(idSender, It.IsAny<DateTime>()))
                          .ReturnsAsync((Matches)null);

            _validatorMock.Setup(v => v.ValidateSendMatchInvite(receiverTeam, senderTeam, null, null));

            _notificationServiceMock
                    .Setup(n => n.SendUserAsync(
                        It.IsAny<string>(),  
                        It.IsAny<string>(), 
                        It.IsAny<string>(),  
                        It.IsAny<object?>()  
                    ))
                    .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // ACT
            var result = await _sut.SendMatchInvite(idSender, dto);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().BeOfType<InfoMatchInviteDto>();
            result.NamePitch.Should().Be("Campo Central");

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _matchInviteRepoMock.Verify(r => r.AddMatchInvite(It.IsAny<MatchInvite>()), Times.Once);

            // Validar notificação
            _notificationServiceMock.Verify(n => n.SendUserAsync(
                    It.IsAny<string>(),
                    "New Match Invite",     
                    It.IsAny<string>(),
                    It.IsAny<object?>()     
                ), Times.Once);
        }
        [Test(Description = "T2GP1 - Player não Admin tenta enviar convite de partida casual para outra equipa.")]
        public async Task SendMatchInvite_Should_Throw_When_PlayerIsNotAdmin()
        {
            // ARRANGE
            var idSender = Guid.NewGuid();
            var dto = new SendMatchInviteDto
            {
                IdSender = idSender,
                IdReceiver = Guid.NewGuid(),
                GameDate = DateTime.UtcNow.AddDays(2),
                homePitch = true
            };

            // Simular erro de validação (Player não é admin)
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
                homePitch = true
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
            var idSender = Guid.NewGuid();
            var dto = new SendMatchInviteDto
            {
                IdSender = idSender,
                IdReceiver = Guid.NewGuid(),
                GameDate = DateTime.UtcNow.AddDays(1),
                homePitch = true
            };

            var senderTeam = new Team("Team Sender", "Desc", new byte[] { 1 }, new Pitch("Campo Central", "Rua X"), _defaultRank);

            _teamRepoMock.Setup(r => r.GetTeamByIdWithPitchAsync(idSender)).ReturnsAsync(senderTeam);
            _teamRepoMock.Setup(r => r.GetTeamByIdWithPitchAsync(dto.IdReceiver))
                          .ReturnsAsync((Team?)null); // Receiver não existe

            // A validação ValidateSendMatchInvite é chamada depois de obter as equipas
            _validatorMock
                .Setup(v => v.ValidateSendMatchInvite(null, senderTeam, null, null))
                .Throws(new ValidationException("A equipa de destino não existe."));

            // ACT
            Func<Task> act = async () => await _sut.SendMatchInvite(idSender, dto);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("A equipa de destino não existe.");
            _matchInviteRepoMock.Verify(r => r.AddMatchInvite(It.IsAny<MatchInvite>()), Times.Never);
        }
        #endregion

        #region AcceptMatchInviteTests
        [Test(Description = "T1GP2 - Player Admin aceita convite de partida casual enviado de outra equipa.")]
        public async Task AcceptMatchInvite_Should_CreateMatch_When_AdminTeamAccepts()
        {
            // ARRANGE
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var pitch = new Pitch("Campo Central", "Rua Principal");

            var senderTeam = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var receiverTeam = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);

            // Inicializar listas para evitar NullReferenceException
            senderTeam.Calendar = new Calendar();
            senderTeam.Calendar.Matches = new List<Matches>();
            receiverTeam.Calendar = new Calendar();
            receiverTeam.Calendar.Matches = new List<Matches>();
            receiverTeam.ReceivedInvites = new List<MatchInvite>();

            var matchInvite = new MatchInvite(senderTeam, receiverTeam, DateTime.UtcNow.AddDays(1), pitch);
            receiverTeam.ReceivedInvites.Add(matchInvite);

            // Mocks
            _teamRepoMock.Setup(r => r.GetByIdWithReceivedInvitesAndCalendar(receiverTeam.Id))
                         .ReturnsAsync(receiverTeam);

            _teamRepoMock.Setup(r => r.GetByIdWithReceivedInvitesAndCalendar(senderTeam.Id))
                         .ReturnsAsync(senderTeam); // O Sender também precisa de ser mockado aqui

            _pitchRepoMock.Setup(r => r.GetPitchById(matchInvite.IdPitch))
                          .ReturnsAsync(pitch);
            _matchRepoMock.Setup(r => r.GetMatchProxim12HoursMatchs(receiverTeam.Id, It.IsAny<DateTime>()))
                          .ReturnsAsync((Matches?)null);

            // Setup Validator (vazios pois assumimos que passam, exceto se quisermos testar falha)
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
           // _notificationServiceMock.Verify(n => n.SendTeamAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
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
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var pitch = new Pitch("Campo Central", "Rua A");

            var senderTeam = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            senderTeam.Id = Guid.NewGuid();

            var receiverTeam = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            receiverTeam.Id = Guid.NewGuid();

            var matchInvite = new MatchInvite(senderTeam, receiverTeam, DateTime.UtcNow.AddDays(1), pitch);
            matchInvite.IdSender = senderTeam.Id;
            matchInvite.IdReceiver = receiverTeam.Id;

            var newDate = DateTime.UtcNow.AddDays(2);
            var dto = new SendMatchInviteDto
            {
                IdSender = senderTeam.Id,
                IdReceiver = receiverTeam.Id,
                GameDate = newDate,
                homePitch = true
            };

            _matchInviteRepoMock.Setup(r => r.GetMatchInviteWithPitchByTeams(senderTeam.Id, receiverTeam.Id))
                                .ReturnsAsync(matchInvite);

            _matchRepoMock.Setup(r => r.GetMatchProxim12HoursMatchs(receiverTeam.Id, newDate))
                          .ReturnsAsync((Matches?)null);

            _validatorMock.Setup(v => v.ValidateSenderMatchInvite(dto, senderTeam.Id));

            _validatorMock.Setup(v => v.ValidateNegociateMatchInvite(
                It.IsAny<Pitch>(), matchInvite, senderTeam, receiverTeam, null));

            _validatorMock.Setup(v => v.ValidateHasChangeNegociateMatchInvite(true));

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // ACT
            var result = await _sut.NegociateMatchInvite(senderTeam.Id, dto);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().BeOfType<InfoMatchInviteDto>();
            result.GameDate.Should().Be(newDate);
            result.NamePitch.Should().Be(pitch.Name);
            result.Sender.Name.Should().Be(senderTeam.Name);

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "T2GP4 - Player não admin da equipa tenta negociar convite.")]
        public async Task NegociateMatchInvite_Should_Throw_When_TeamIsNotAdmin()
        {
            // ARRANGE
            var idSender = Guid.NewGuid();
            var idReceiver = Guid.NewGuid();

            var dto = new SendMatchInviteDto
            {
                IdSender = idSender, // Agora obrigatório no DTO
                IdReceiver = idReceiver,
                GameDate = DateTime.UtcNow.AddDays(1),
                homePitch = true // Substituiu o NamePitch
            };

            // Simular que o Validator lança erro logo no início (ao validar o Sender)
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
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var pitch = new Pitch("Campo", "Rua");

            var teamA = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var teamB = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            var outsiderTeam = new Team("Team Outsider", "desc", new byte[] { 3 }, pitch, rank);

            // O convite existe entre A e B
            var invite = new MatchInvite(teamA, teamB, DateTime.UtcNow.AddDays(1), pitch);

            // O Outsider tenta negociar
            var dto = new SendMatchInviteDto
            {
                IdSender = outsiderTeam.Id, // Outsider é quem envia o pedido de negociação
                IdReceiver = teamB.Id,
                GameDate = DateTime.UtcNow.AddDays(2),
                homePitch = false // Quer jogar fora (ou em casa), irrelevante para o erro
            };

            _validatorMock
                .Setup(v => v.ValidateSenderMatchInvite(dto, outsiderTeam.Id))
                .Throws(new ValidationException("A equipa não pertence ao convite a negociar."));

            // ACT
            Func<Task> act = async () => await _sut.NegociateMatchInvite(outsiderTeam.Id, dto);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence ao convite*");

            // Garante que o repositório nunca foi chamado para buscar o invite, pois falhou na validação inicial
            _matchInviteRepoMock.Verify(r => r.GetMatchInviteWithPitchByTeams(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #endregion
    }
}