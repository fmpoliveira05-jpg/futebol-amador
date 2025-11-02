using Application.DTOs;
using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.Pitch;
using Application.DTOs.PostPoneGame;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Validators;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Unit.ApplicationTests.ServicesTests
{
    [TestFixture]
    public class MatchServiceTests
    {
        #region Variables
        private Mock<IMatchRepository> _matchRepoMock;
        private Mock<ITeamPostPoneGameRepository> _teamPostPoneRepoMock;
        private Mock<ICancelledMatchRepository> _cancelledMatchRepoMock;
        private Mock<IUnityOfWork> _unitOfWorkMock;
        private Mock<ICalendarValidator> _validatorMock;
        private Mock<IPlayerRepository> _playerRepoMock;
        private MatchService _sut;
        #endregion

        #region SetUp
        [SetUp]
        public void SetUp()
        {
            _matchRepoMock = new Mock<IMatchRepository>();
            _teamPostPoneRepoMock = new Mock<ITeamPostPoneGameRepository>();
            _cancelledMatchRepoMock = new Mock<ICancelledMatchRepository>();
            _unitOfWorkMock = new Mock<IUnityOfWork>();
            _validatorMock = new Mock<ICalendarValidator>();
            _playerRepoMock = new Mock<IPlayerRepository>();

            _sut = new MatchService(
                _matchRepoMock.Object,
                _teamPostPoneRepoMock.Object,
                _cancelledMatchRepoMock.Object,
                _unitOfWorkMock.Object,
                _validatorMock.Object,
                _playerRepoMock.Object
            );
        }
        #endregion

        #region Tests
        [Test(Description = "GetCalendar deve devolver todas as partidas para uma equipa")]
        public async Task GetCalendar_Should_Return_All_Matches_For_Team()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();

            var team = new TeamDto { IdTeam = teamId, Name = "FC Unity" };
            var opponent = new TeamDto { IdTeam = Guid.NewGuid(), Name = "Real Opponent" };
            var pitch = new PitchDto { Name = "Campo Central", Address = "Rua Principal" };

            var matches = new List<InfoMatchCalendar>
            {
                new InfoMatchCalendar
                {
                    IdMatch = Guid.NewGuid(),
                    MatchStatus = MatchStatus.SCHEDULED,
                    GameDate = DateTime.Today.AddDays(1),
                    Result = null,
                    MatchResult = MatchResult.UNPLAYED,
                    Team = team,
                    Opponent = opponent,
                    pitchGame = pitch
                },
                new InfoMatchCalendar
                {
                    IdMatch = Guid.NewGuid(),
                    MatchStatus = MatchStatus.DONE,
                    GameDate = DateTime.Today.AddDays(-1),
                    Result = "2-1",
                    MatchResult = MatchResult.WIN,
                    Team = team,
                    Opponent = opponent,
                    pitchGame = pitch
                }
            };

            _matchRepoMock.Setup(r => r.GetAllMatchesTeam(teamId)).ReturnsAsync(matches);

            // ACT
            var result = await _sut.GetCalendar(teamId);

            // ASSERT
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(matches);
            _matchRepoMock.Verify(r => r.GetAllMatchesTeam(teamId), Times.Once);
        }

        [Test(Description = "GetCalendarWithFilters deve devolver resultados quando o filtro é válido")]
        public async Task GetCalendarWithFilters_Should_ValidateAnd_ReturnFilteredMatches()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var filter = new FilterCalendarDto
            {
                IsRealized = true,
                IsRanqued = true,
                IsHome = true,
                MinDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
                MaxDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                NameOpponent = "Real Opponent"
            };
            var team = new TeamDto { IdTeam = teamId, Name = "FC Unity" };
            var opponent = new TeamDto { IdTeam = Guid.NewGuid(), Name = "Real Opponent" };
            var pitch = new PitchDto { Name = "Campo Norte", Address = "Avenida do Desporto" };
            var filteredMatches = new List<InfoMatchCalendar>
            {
                new InfoMatchCalendar
                {
                    IdMatch = Guid.NewGuid(),
                    MatchStatus = Domain.Enums.MatchStatus.DONE,
                    GameDate = DateTime.Today.AddDays(-2),
                    Result = "3-1",
                    MatchResult = Domain.Enums.MatchResult.WIN,
                    Team = team,
                    Opponent = opponent,
                    pitchGame = pitch
                }
            };
            _validatorMock.Setup(v => v.ValidateFilterCalendar(teamId, filter));
            _matchRepoMock.Setup(r => r.GetAllMatchesTeamWithFilters(teamId, filter))
                          .ReturnsAsync(filteredMatches);

            // ACT
            var result = await _sut.GetCalendarWithFilters(teamId, filter);

            // ASSERT
            result.Should().HaveCount(1, "porque há uma partida que cumpre os filtros");
            result.Should().BeEquivalentTo(filteredMatches, "porque o serviço apenas retorna o resultado do repositório");
            _validatorMock.Verify(v => v.ValidateFilterCalendar(teamId, filter), Times.Once, "porque o filtro deve ser validado antes da procura");
            _matchRepoMock.Verify(r => r.GetAllMatchesTeamWithFilters(teamId, filter), Times.Once, "porque deve chamar o repositório uma única vez com os filtros aplicados");
        }

        [Test(Description = "GetCalendarWithFilters deve lançar exceção quando a data mínima for maior ou igual à data máxima")]
        public async Task GetCalendarWithFilters_Should_Throw_Exception_When_MinDate_Is_Greater_Than_MaxDate()
        {
            // ARRANGE
            var idTeam = Guid.NewGuid();
            var filter = new FilterCalendarDto
            {
                MinDate = new DateOnly(2025, 12, 15),
                MaxDate = new DateOnly(2025, 12, 14)
            };
            _validatorMock.Setup(v => v.ValidateFilterCalendar(It.IsAny<Guid>(), It.IsAny<FilterCalendarDto>())).Throws(new InvalidOperationException("A data minima tem de ser inferior ou igual à data maxima"));

            // ACT
            Func<Task> act = async () => await _sut.GetCalendarWithFilters(idTeam, filter);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("A data minima tem de ser inferior ou igual à data maxima");
        }

        [Test(Description = "GetCalendarWithFilters deve lançar exceção quando o id da equipa for nulo")]
        public async Task GetCalendarWithFilters_Should_Throw_Exception_When_Team_Id_Is_Empty()
        {
            // ARRANGE
            var idTeam = Guid.Empty;
            var filter = new FilterCalendarDto
            {
                MinDate = new DateOnly(2025, 12, 1),
                MaxDate = new DateOnly(2025, 12, 31)
            };
            _validatorMock.Setup(v => v.ValidateFilterCalendar(It.IsAny<Guid>(), It.IsAny<FilterCalendarDto>())).Throws(new InvalidOperationException("O id da equipa não pode estar nulo"));

            // ACT
            Func<Task> act = async () => await _sut.GetCalendarWithFilters(idTeam, filter);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("O id da equipa não pode estar nulo");
        }

        [Test(Description = "PostPoneMatch deve adiar a partida, atualizar estado e persistir")]
        public async Task PostPoneMatch_Should_Create_PostPoneRecord_And_Update_MatchStatus()
        {
            // ARRANGE
            var idMatch = Guid.NewGuid();
            var idTeam = Guid.NewGuid();
            var idOpponent = Guid.NewGuid();
            var newDate = DateTime.Now.AddDays(1);
            var dto = new PostPoneMatchDto
            {
                IdMatch = idMatch,
                IdTeam = idTeam,
                IdOpponent = idOpponent,
                PostPoneDate = newDate
            };
            var pitch = new Pitch("Campo A", "Rua A");
            var rank = new Rank("R", 0, 0, 0, 0, null!, null!);
            var teamA = new Teams("TeamA", "desc", new byte[] { 1 }, pitch, rank) { Id = idTeam };
            var teamB = new Teams("TeamB", "desc", new byte[] { 1 }, pitch, rank) { Id = idOpponent };
            var teamStat = new TeamStatistics(teamA) { IdTeam = idTeam };
            var oppStat = new TeamStatistics(teamB) { IdTeam = idOpponent };
            var match = new Matches(DateTime.Now.AddHours(2), true, pitch)
            {
                Id = idMatch,
                MatchStatus = MatchStatus.SCHEDULED,
                Teams = new List<TeamStatistics> { teamStat, oppStat }
            };
            _matchRepoMock.Setup(r => r.GetMatchById(idMatch)).ReturnsAsync(match);
            _validatorMock.Setup(v => v.ValidatorPostPoneMatch(match, newDate, teamStat, idTeam, oppStat, idOpponent));
            _teamPostPoneRepoMock
                .Setup(r => r.AddTeamPostPoneMatch(It.IsAny<PostPoneMatch>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // ACT
            var result = await _sut.PostPoneMatch(idTeam, dto);

            // ASSERT
            result.IdMatch.Should().Be(idMatch);
            result.PostPoneDate.Should().Be(newDate);
            result.IdTeam.Should().Be(idTeam);
            result.IdOpponent.Should().Be(idOpponent);
            result.nameTeam.Should().Be("TeamA");
            result.nameOpponent.Should().Be("TeamB");
            match.MatchStatus.Should().Be(MatchStatus.POST_PONED, "o estado do jogo deve mudar para POST_PONED");
            _validatorMock.Verify(v => v.ValidatorPostPoneMatch(match, newDate, teamStat, idTeam, oppStat, idOpponent), Times.Once);
            _teamPostPoneRepoMock.Verify(r => r.AddTeamPostPoneMatch(It.IsAny<PostPoneMatch>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "PostPoneMatch deve lançar exceção quando a partida for nula")]
        public async Task PostPoneMatch_Should_Throw_Exception_When_Match_Is_Null()
        {
            // ARRANGE
            var idMatch = Guid.NewGuid();
            var idTeam = Guid.NewGuid();
            var idOpponent = Guid.NewGuid();
            var newDate = DateTime.Now.AddDays(1);
            var dto = new PostPoneMatchDto { IdMatch = idMatch, IdTeam = idTeam, IdOpponent = idOpponent, PostPoneDate = newDate };
            _matchRepoMock.Setup(r => r.GetMatchById(idMatch)).ReturnsAsync((Matches?)null);
            _validatorMock.Setup(v => v.ValidatorPostPoneMatch(It.IsAny<Matches>(), It.IsAny<DateTime>(), It.IsAny<TeamStatistics>(), It.IsAny<Guid>(), It.IsAny<TeamStatistics>(), It.IsAny<Guid>())).Throws(new ArgumentException("A match não pode estar nula"));

            // ACT
            Func<Task> act = async () => await _sut.PostPoneMatch(idTeam, dto);

            // ASSERT
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("A match não pode estar nula");
        }

        [Test(Description = "PostPoneMatch deve lançar exceção quando a data de adiamento for a mesma da data já marcada")]
        public async Task PostPoneMatch_Should_Throw_Exception_When_NewDate_Is_Same_As_MatchDate()
        {
            // ARRANGE
            var idMatch = Guid.NewGuid();
            var idTeam = Guid.NewGuid();
            var idOpponent = Guid.NewGuid();
            var matchDate = DateTime.Now.AddDays(1);
            var dto = new PostPoneMatchDto { IdMatch = idMatch, IdTeam = idTeam, IdOpponent = idOpponent, PostPoneDate = matchDate };
            var match = new Matches(matchDate, true, new Pitch("Campo", "Rua"))
            {
                Id = idMatch,
                MatchDate = matchDate,
                MatchStatus = MatchStatus.SCHEDULED
            };
            _matchRepoMock.Setup(r => r.GetMatchById(idMatch)).ReturnsAsync(match);
            _validatorMock.Setup(v => v.ValidatorPostPoneMatch(It.IsAny<Matches>(), It.IsAny<DateTime>(), It.IsAny<TeamStatistics>(), It.IsAny<Guid>(), It.IsAny<TeamStatistics>(), It.IsAny<Guid>())).Throws(new BusinessRuleException("A data de adiamento não pode ser a mesma da data já marcada"));


            // ACT
            Func<Task> act = async () => await _sut.PostPoneMatch(idTeam, dto);

            // ASSERT
            await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("A data de adiamento não pode ser a mesma da data já marcada");
        }

        [Test(Description = "PostPoneMatch deve lançar exceção quando a data de adiamento for menos de 12 horas após a hora atual")]
        public async Task PostPoneMatch_Should_Throw_Exception_When_NewDate_Is_Less_Than_12_Hours_After_Current_Time()
        {
            // ARRANGE
            var idMatch = Guid.NewGuid();
            var idTeam = Guid.NewGuid();
            var idOpponent = Guid.NewGuid();
            var newDate = DateTime.UtcNow.AddHours(1);
            var dto = new PostPoneMatchDto { IdMatch = idMatch, IdTeam = idTeam, IdOpponent = idOpponent, PostPoneDate = newDate };
            var match = new Matches(DateTime.UtcNow.AddHours(5), true, new Pitch("Campo", "Rua"))
            {
                Id = idMatch,
                MatchDate = DateTime.UtcNow.AddHours(5),
                MatchStatus = MatchStatus.SCHEDULED
            };
            _matchRepoMock.Setup(r => r.GetMatchById(idMatch)).ReturnsAsync(match);
            _validatorMock.Setup(v => v.ValidatorPostPoneMatch(It.IsAny<Matches>(), It.IsAny<DateTime>(), It.IsAny<TeamStatistics>(), It.IsAny<Guid>(), It.IsAny<TeamStatistics>(), It.IsAny<Guid>())).Throws(new BusinessRuleException("O horario da partida deve ser pelo menos 12 horas apos a hora atual"));

            // ACT
            Func<Task> act = async () => await _sut.PostPoneMatch(idTeam, dto);

            // ASSERT
            await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("O horario da partida deve ser pelo menos 12 horas apos a hora atual");
        }

        [Test(Description = "PostPoneMatch deve lançar exceção quando o status da partida não for SCHEDULED ou POST_PONED")]
        public async Task PostPoneMatch_Should_Throw_Exception_When_Match_Status_Is_Not_Scheduled_Or_PostPoned()
        {
            // ARRANGE
            var idMatch = Guid.NewGuid();
            var idTeam = Guid.NewGuid();
            var idOpponent = Guid.NewGuid();
            var newDate = DateTime.Now.AddDays(1);
            var dto = new PostPoneMatchDto { IdMatch = idMatch, IdTeam = idTeam, IdOpponent = idOpponent, PostPoneDate = newDate };
            var match = new Matches(DateTime.Now.AddDays(1), true, new Pitch("Campo", "Rua"))
            {
                Id = idMatch,
                MatchDate = DateTime.Now.AddDays(1),
                MatchStatus = MatchStatus.IN_PROGRESS
            };
            _matchRepoMock.Setup(r => r.GetMatchById(idMatch)).ReturnsAsync(match);
            _validatorMock.Setup(v => v.ValidatorPostPoneMatch(It.IsAny<Matches>(), It.IsAny<DateTime>(), It.IsAny<TeamStatistics>(), It.IsAny<Guid>(), It.IsAny<TeamStatistics>(), It.IsAny<Guid>())).Throws(new BusinessRuleException("Só podem ser adiadas partidas marcadas ou em estado de adiamento"));

            // ACT
            Func<Task> act = async () => await _sut.PostPoneMatch(idTeam, dto);

            // ASSERT
            await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Só podem ser adiadas partidas marcadas ou em estado de adiamento");
        }

        [Test(Description = "AcceptPostPoneMatch deve aceitar adiamento, remover record e atualizar estado")]
        public async Task AcceptPostPoneMatch_Should_Remove_PostponeRecord_And_Update_Match()
        {
            // ARRANGE
            var idMatch = Guid.NewGuid();
            var idTeam = Guid.NewGuid();
            var idOpponent = Guid.NewGuid();
            var newDate = DateTime.Now.AddDays(2);
            var dto = new AcceptRefusePostPoneDto
            {
                IdMatch = idMatch,
                IdTeam = idTeam,
                IdOpponent = idOpponent
            };
            var pitch = new Pitch("p", "a");
            var rank = new Rank("R", 0, 0, 0, 0, null!, null!);
            var teamA = new Teams("TeamA", "desc", new byte[] { 1 }, pitch, rank) { Id = idTeam };
            var teamB = new Teams("TeamB", "desc", new byte[] { 1 }, pitch, rank) { Id = idOpponent };
            var teamStat = new TeamStatistics(teamA) { IdTeam = idTeam };
            var oppStat = new TeamStatistics(teamB) { IdTeam = idOpponent };
            var match = new Matches(DateTime.Now.AddHours(2), true, pitch)
            {
                Id = idMatch,
                Teams = new List<TeamStatistics> { teamStat, oppStat },
                MatchStatus = MatchStatus.POST_PONED
            };
            var postPone = new PostPoneMatch(teamA, match, newDate);
            _teamPostPoneRepoMock.Setup(r => r.GetTeamPostPoneMatchWithPitch(idOpponent, idMatch)).ReturnsAsync(postPone);
            _validatorMock.Setup(v => v.ValidatorAcceptPostPoneMatch(postPone, match, teamStat, idTeam, oppStat, idOpponent));
            _teamPostPoneRepoMock.Setup(r => r.RemoveTeamPostPoneMatch(postPone));
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // ACT
            var result = await _sut.AcceptPostPoneMatch(idTeam, dto);

            // ASSERT
            result.IdMatch.Should().Be(idMatch);
            result.GameDate.Should().Be(newDate);
            result.NameTeam.Should().Be("TeamA");
            result.NameOpponent.Should().Be("TeamB");
            result.NamePitch.Should().Be(pitch.Name);
            match.MatchStatus.Should().Be(MatchStatus.SCHEDULED, "o estado do jogo deve voltar a ser SCHEDULED após o adiamento ser aceite");
            _teamPostPoneRepoMock.Verify(r => r.RemoveTeamPostPoneMatch(postPone), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "RejectPostPoneMatch deve cancelar a partida e mudar estado para CANCELED")]
        public async Task RejectPostPoneMatch_Should_Cancel_Match_When_AdminRejects()
        {
            // ARRANGE
            var idMatch = Guid.NewGuid();
            var idTeam = Guid.NewGuid();
            var idOpponent = Guid.NewGuid();
            var dto = new AcceptRefusePostPoneDto
            {
                IdMatch = idMatch,
                IdTeam = idTeam,
                IdOpponent = idOpponent
            };
            var pitch = new Pitch("p", "a");
            var rank = new Rank("R", 0, 0, 0, 0, null!, null!);
            var teamA = new Teams("TeamA", "desc", new byte[] { 1 }, pitch, rank) { Id = idTeam };
            var teamB = new Teams("TeamB", "desc", new byte[] { 1 }, pitch, rank) { Id = idOpponent };
            var teamStat = new TeamStatistics(teamA) { IdTeam = idTeam };
            var oppStat = new TeamStatistics(teamB) { IdTeam = idOpponent };
            var match = new Matches(DateTime.Now.AddDays(3), true, pitch)
            {
                Id = idMatch,
                MatchStatus = MatchStatus.POST_PONED,
                Teams = new List<TeamStatistics> { teamStat, oppStat }
            };
            var postPone = new PostPoneMatch(teamA, match, DateTime.Now.AddDays(4));
            _teamPostPoneRepoMock.Setup(r => r.GetTeamPostPoneMatch(idOpponent, idMatch)).ReturnsAsync(postPone);
            _validatorMock.Setup(v => v.ValidatorRejectPostPoneMatch(postPone, match, teamStat, idTeam, oppStat, idOpponent));
            _teamPostPoneRepoMock.Setup(r => r.RemoveTeamPostPoneMatch(postPone));
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // ACT
            await _sut.RejectPostPoneMatch(idTeam, dto);

            // ASSERT
            match.MatchStatus.Should().Be(MatchStatus.CANCELED, "porque ao rejeitar o adiamento o jogo deve ser cancelado");
            _teamPostPoneRepoMock.Verify(r => r.RemoveTeamPostPoneMatch(postPone), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "GetListPostPoneMatchTeam deve devolver a lista de partidas adiadas")]
        public async Task GetListPostPoneMatchTeam_Should_Return_List_When_Valid()
        {
            // ARRANGE
            var idTeam = Guid.NewGuid();
            var list = new List<InfoPostPoneMatch> { new InfoPostPoneMatch() };
            _matchRepoMock.Setup(r => r.GetAllMatchPostPoneReceiverById(idTeam)).ReturnsAsync(list);
            _validatorMock.Setup(v => v.ValidatorGetListPostPoneMatchTeam(list));

            // ACT
            var result = await _sut.GetListPostPoneMatchTeam(idTeam);

            // ASSERT
            result.Should().BeEquivalentTo(list, "porque a lista de partidas adiadas para a equipa deve ser retornada");
            _validatorMock.Verify(v => v.ValidatorGetListPostPoneMatchTeam(list), Times.Once);
        }

        [Test(Description = "GetListPostPoneMatchTeam deve lançar exceção quando a lista de adiamentos estiver vazia")]
        public async Task GetListPostPoneMatchTeam_Should_Throw_Exception_When_List_Is_Empty()
        {
            // ARRANGE
            var idTeam = Guid.NewGuid();
            var list = new List<InfoPostPoneMatch>();
            _matchRepoMock.Setup(r => r.GetAllMatchPostPoneReceiverById(idTeam)).ReturnsAsync(list);
            _validatorMock.Setup(v => v.ValidatorGetListPostPoneMatchTeam(list)).Throws(new EmptyCollectionException("A lista de adiamentos da equipa está vazia"));

            // ACT
            Func<Task> act = async () => await _sut.GetListPostPoneMatchTeam(idTeam);

            // ASSERT
            await act.Should().ThrowAsync<EmptyCollectionException>().WithMessage("A lista de adiamentos da equipa está vazia");
            _validatorMock.Verify(v => v.ValidatorGetListPostPoneMatchTeam(list), Times.Once);
        }

        [Test(Description = "CancelMatch deve criar CancelledMatch e persistir")]
        public async Task CancelMatch_Should_Create_CancelledMatch_And_Update_Status()
        {
            // ARRANGE
            var idTeam = Guid.NewGuid();
            var idMatch = Guid.NewGuid();
            var description = "Motivo";
            var team = new Teams("TeamA", "desc", new byte[] { 1 }, new Pitch("p", "a"), new Rank("R", 0, 0, 0, 0, null, null)) { Id = idTeam };
            var opponentTeam = new Teams("TeamB", "desc", new byte[] { 1 }, new Pitch("p", "a"), new Rank("R", 0, 0, 0, 0, null, null)) { Id = Guid.NewGuid() };
            var match = new Matches(DateTime.Now.AddDays(1), true, new Pitch("p", "a"))
            {
                Id = idMatch,
                MatchStatus = MatchStatus.SCHEDULED,
                Teams = new List<TeamStatistics>
                {
                    new TeamStatistics(team) { IdTeam = idTeam },
                    new TeamStatistics(opponentTeam) { IdTeam = opponentTeam.Id }
                }
            };
            _matchRepoMock.Setup(r => r.GetMatchToCancelById(idMatch)).ReturnsAsync(match);
            _cancelledMatchRepoMock.Setup(r => r.AddCancelledMatch(It.IsAny<CancelledMatch>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.CancelMatch(idTeam, idMatch, description);

            // ASSERT
            match.MatchStatus.Should().Be(MatchStatus.CANCELED, "porque o estado da partida deve ser atualizado para CANCELED");
            _cancelledMatchRepoMock.Verify(r => r.AddCancelledMatch(It.IsAny<CancelledMatch>()), Times.Once, "porque um CancelledMatch deve ser criado");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once, "porque as mudanças devem ser persistidas");
        }

        /*
        [Test(Description = "CancelMatch deve lançar exceção se o estado da partida não for SCHEDULED")]
        public async Task CancelMatch_Should_Throw_Exception_When_Match_Is_Not_Scheduled()
        {
            // ARRANGE
            var idTeam = Guid.NewGuid();
            var idMatch = Guid.NewGuid();
            var description = "Motivo";
            var team = new Teams("TeamA", "desc", new byte[] { 1 }, new Pitch("p", "a"), new Rank("R", 0, 0, 0, 0, null, null));
            var opponentTeam = new Teams("TeamB", "desc", new byte[] { 1 }, new Pitch("p", "a"), new Rank("R", 0, 0, 0, 0, null, null));

            var match = new Matches(DateTime.Now.AddDays(1), true, new Pitch("p", "a"))
            {
                Id = idMatch,
                Teams = new List<TeamStatistics>
        {
            new TeamStatistics(team) { IdTeam = idTeam, Team = team },
            new TeamStatistics(opponentTeam) { IdTeam = opponentTeam.Id }
        },
                MatchStatus = MatchStatus.IN_PROGRESS
            };

            _matchRepoMock.Setup(r => r.GetMatchToCancelById(idMatch)).ReturnsAsync(match);

            // ACT
            Func<Task> act = async () => await _sut.CancelMatch(idTeam, idMatch, description);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>().WithMessage("A partida não pode ser cancelada se não estiver no estado SCHEDULED.");
        }
        */

        [Test(Description = "CancelMatch deve lançar exceção se a match for nula")]
        public async Task CancelMatch_Should_Throw_Exception_When_Match_Is_Null()
        {
            // ARRANGE
            var idTeam = Guid.NewGuid();
            var idMatch = Guid.NewGuid();
            var description = "Motivo";
            _matchRepoMock.Setup(r => r.GetMatchToCancelById(idMatch)).ReturnsAsync((Matches?)null);

            // ACT
            Func<Task> act = async () => await _sut.CancelMatch(idTeam, idMatch, description);

            // ASSERT
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("A match a cancelar não existe ou já não pode ser cancelada.");
        }

        /*
        [Test(Description = "CancelMatch deve lançar exceção se o jogador não for administrador")]
        public async Task CancelMatch_Should_Throw_Exception_When_Player_Is_Not_Admin()
        {
            // ARRANGE
            var idTeam = Guid.NewGuid();
            var idMatch = Guid.NewGuid();
            var description = "Motivo";
            var team = new Teams("TeamA", "desc", new byte[] { 1 }, new Pitch("p", "a"), new Rank("R", 0, 0, 0, 0, null, null));
            var opponentTeam = new Teams("TeamB", "desc", new byte[] { 1 }, new Pitch("p", "a"), new Rank("R", 0, 0, 0, 0, null, null));

            var match = new Matches(DateTime.Now.AddDays(1), true, new Pitch("p", "a"))
            {
                Id = idMatch,
                Teams = new List<TeamStatistics>
        {
            new TeamStatistics(team) { IdTeam = idTeam, Team = team },
            new TeamStatistics(opponentTeam) { IdTeam = opponentTeam.Id }
        },
                MatchStatus = MatchStatus.SCHEDULED
            };

            var player = new Player { Id = Guid.NewGuid(), IdTeam = idTeam, IsAdmin = false };
            _matchRepoMock.Setup(r => r.GetMatchToCancelById(idMatch)).ReturnsAsync(match);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.CancelMatch(idTeam, idMatch, description);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>().WithMessage("Apenas administradores podem cancelar partidas.");
        }
        */

        /*
        [Test(Description = "CancelMatch deve lançar exceção se o jogador não estiver associado a um clube")]
        public async Task CancelMatch_Should_Throw_Exception_When_Player_Is_Not_In_A_Team()
        {
            // ARRANGE
            var idMatch = Guid.NewGuid();
            var description = "Motivo";
            var player = new Player { Id = Guid.NewGuid(), IdTeam = Guid.Empty };

            // ACT
            Func<Task> act = async () => await _sut.CancelMatch(player.Id, idMatch, description);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>().WithMessage("O jogador precisa estar associado a uma equipa para cancelar uma partida.");
        }
        */
        #endregion
    }
}