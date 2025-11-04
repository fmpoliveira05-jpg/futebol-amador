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
using System.Runtime.ConstrainedExecution;

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

        #region PostPoneMatchTests
        [Test(Description = "T1GP5 - Player Admin de equipa adia partida SCHEDULED para data futura (12h depois).")]
        public async Task PostPoneMatch_Should_ReturnInfoPostPoneMatch_When_AdminPostponesSuccessfully()
        {
            // ARRANGE
            var pitch = new Pitch("Campo Central", "Rua X");
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var team = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var opponent = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            var match = new Matches(DateTime.UtcNow.AddDays(1), false, pitch.Id, new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.SCHEDULED
            };
            var teamStats = new TeamStatistics(team) { IdTeam = team.Id };
            var opponentStats = new TeamStatistics(opponent) { IdTeam = opponent.Id };
            match.Teams.Add(teamStats);
            match.Teams.Add(opponentStats);
            var dto = new PostPoneMatchDto
            {
                IdMatch = match.Id,
                PostPoneDate = DateTime.UtcNow.AddHours(13),
                IdOpponent = opponent.Id
            };
            _matchRepoMock.Setup(r => r.GetMatchById(match.Id)).ReturnsAsync(match);
            _validatorMock.Setup(v => v.ValidatePostPoneMatchDto(team.Id, dto));
            _validatorMock.Setup(v => v.ValidatorPostPoneMatch(match, dto.PostPoneDate, teamStats, team.Id, opponentStats, opponent.Id));
            _teamPostPoneRepoMock.Setup(r => r.AddTeamPostPoneMatch(It.IsAny<PostPoneMatch>()));
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // ACT
            var result = await _sut.PostPoneMatch(team.Id, dto);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().BeOfType<InfoPostPoneMatch>();
            result.IdMatch.Should().Be(match.Id);
            result.PostPoneDate.Should().Be(dto.PostPoneDate);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "T2GP5 - Player não admin tenta adiar partida.")]
        public async Task PostPoneMatch_Should_Throw_When_TeamIsNotAdmin()
        {
            var teamId = Guid.NewGuid();
            var dto = new PostPoneMatchDto { IdMatch = Guid.NewGuid(), PostPoneDate = DateTime.UtcNow.AddDays(1) };

            _validatorMock
                .Setup(v => v.ValidatePostPoneMatchDto(teamId, dto))
                .Throws(new ValidationException("A equipa não é administradora."));

            Func<Task> act = async () => await _sut.PostPoneMatch(teamId, dto);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*administradora*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T3GP5 - Player não pertencente à equipa tenta adiar partida.")]
        public async Task PostPoneMatch_Should_Throw_When_TeamDoesNotBelongToMatch()
        {
            var outsiderTeamId = Guid.NewGuid();
            var dto = new PostPoneMatchDto { IdMatch = Guid.NewGuid(), PostPoneDate = DateTime.UtcNow.AddDays(1) };

            _validatorMock
                .Setup(v => v.ValidatePostPoneMatchDto(outsiderTeamId, dto))
                .Throws(new ValidationException("A equipa não pertence à partida."));

            Func<Task> act = async () => await _sut.PostPoneMatch(outsiderTeamId, dto);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T4GP5 - Tenta adiar partida com estado CANCELLED, DONE ou IN_PROGRESS.")]
        public async Task PostPoneMatch_Should_Throw_When_MatchHasInvalidStatus()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var team = new Team("Team A", "desc", new byte[] { 1 }, new Pitch("Campo", "Rua"), new Rank("Unranked", 0, 0, 0, 0, null!, null!));
            var opponent = new Team("Team B", "desc", new byte[] { 2 }, new Pitch("Campo", "Rua"), new Rank("Unranked", 0, 0, 0, 0, null!, null!));
            var match = new Matches(DateTime.UtcNow.AddDays(1), false, Guid.NewGuid(), new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.DONE
            };
            var teamStats = new TeamStatistics(team) { IdTeam = team.Id };
            var opponentStats = new TeamStatistics(opponent) { IdTeam = opponent.Id };
            match.Teams.Add(teamStats);
            match.Teams.Add(opponentStats);
            var dto = new PostPoneMatchDto
            {
                IdMatch = match.Id,
                PostPoneDate = DateTime.UtcNow.AddDays(2),
                IdOpponent = opponent.Id
            };
            _matchRepoMock.Setup(r => r.GetMatchById(match.Id)).ReturnsAsync(match);

            // ACT
            Func<Task> act = async () => await _sut.PostPoneMatch(team.Id, dto);

            // ASSERT
            await act.Should().ThrowAsync<BusinessRuleException>()
                     .WithMessage("Só podem ser adiadas partidas marcadas ou em estado de adiamento.");
        }

        [Test(Description = "T5GP5 - Tentar adiar partida para a mesma data definida.")]
        public async Task PostPoneMatch_Should_Throw_When_NewDateIsSame()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var sameDate = DateTime.UtcNow.AddDays(1);
            var team = new Team("Team A", "desc", new byte[] { 1 }, new Pitch("Campo", "Rua"), new Rank("Unranked", 0, 0, 0, 0, null!, null!));
            var opponent = new Team("Team B", "desc", new byte[] { 2 }, new Pitch("Campo", "Rua"), new Rank("Unranked", 0, 0, 0, 0, null!, null!));
            var match = new Matches(sameDate, false, Guid.NewGuid(), new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.SCHEDULED
            };
            var teamStats = new TeamStatistics(team) { IdTeam = team.Id };
            var opponentStats = new TeamStatistics(opponent) { IdTeam = opponent.Id };
            match.Teams.Add(teamStats);
            match.Teams.Add(opponentStats);
            var dto = new PostPoneMatchDto {
                IdMatch = match.Id,
                PostPoneDate = sameDate,
                IdOpponent = opponent.Id
            };
            _matchRepoMock.Setup(r => r.GetMatchById(match.Id)).ReturnsAsync(match);

            // ACT
            Func<Task> act = async () => await _sut.PostPoneMatch(team.Id, dto);

            // ASSERT
            await act.Should().ThrowAsync<BusinessRuleException>()
                     .WithMessage("A data de adiamento não pode ser a mesma da data já marcada.");
        }

        [Test(Description = "T6GP5 - Tentar adiar partida inexistente.")]
        public async Task PostPoneMatch_Should_Throw_When_MatchDoesNotExist()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var dto = new PostPoneMatchDto { IdMatch = Guid.NewGuid(), PostPoneDate = DateTime.UtcNow.AddDays(1) };
            _matchRepoMock.Setup(r => r.GetMatchById(dto.IdMatch)).ReturnsAsync((Matches?)null);
            _validatorMock
                .Setup(v => v.ValidatorPostPoneMatch(null, dto.PostPoneDate, null, teamId, null, dto.IdOpponent))
                .Throws(new ArgumentException("Partida não encontrada."));

            // ACT
            Func<Task> act = async () => await _sut.PostPoneMatch(teamId, dto);

            // ASSERT
            await act.Should().ThrowAsync<BusinessRuleException>()
                     .WithMessage("A partida não foi encontrada.");
        }

        [Test(Description = "T7GP5 - Tentar adiar partida para a mesma data ou uma data anterior à data que estava marcada.")]
        public async Task PostPoneMatch_Should_Throw_When_NewDateIsBeforeCurrent()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var team = new Team("Team A", "desc", new byte[] { 1 }, new Pitch("Campo", "Rua"), new Rank("Unranked", 0, 0, 0, 0, null!, null!));
            var opponent = new Team("Team B", "desc", new byte[] { 2 }, new Pitch("Campo", "Rua"), new Rank("Unranked", 0, 0, 0, 0, null!, null!));
            var match = new Matches(DateTime.UtcNow.AddDays(1), false, Guid.NewGuid(), new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.SCHEDULED
            };
            var teamStats = new TeamStatistics(team) { IdTeam = team.Id };
            var opponentStats = new TeamStatistics(opponent) { IdTeam = opponent.Id };
            match.Teams.Add(teamStats);
            match.Teams.Add(opponentStats);
            var invalidDate = DateTime.UtcNow.AddHours(-12);
            var dto = new PostPoneMatchDto {
                IdMatch = match.Id,
                PostPoneDate = invalidDate,
                IdOpponent = opponent.Id
            };
            _matchRepoMock.Setup(r => r.GetMatchById(match.Id)).ReturnsAsync(match);

            // ACT
            Func<Task> act = async () => await _sut.PostPoneMatch(team.Id, dto);

            // ASSERT
            await act.Should().ThrowAsync<BusinessRuleException>()
                     .WithMessage("A nova data não pode ser igual ou antes da data atual.");
        }
        #endregion

        #region CancelMatchTests
        [Test(Description = "T1GP6 - Player Admin cancela partida com estado SCHEDULED.")]
        public async Task CancelMatch_Should_CancelScheduledMatch_When_AdminTeam()
        {
            // ARRANGE
            var pitch = new Pitch("Campo Central", "Rua X");
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var team = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var opponent = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            var match = new Matches(DateTime.UtcNow.AddDays(1), false, pitch.Id, new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.SCHEDULED
            };
            var teamStats = new TeamStatistics(team) { IdTeam = team.Id };
            var opponentStats = new TeamStatistics(opponent) { IdTeam = opponent.Id };
            match.Teams.Add(teamStats);
            match.Teams.Add(opponentStats);
            _matchRepoMock.Setup(r => r.GetMatchToCancelById(match.Id)).ReturnsAsync(match);
            _validatorMock.Setup(v => v.ValidateCancelMatch(match, teamStats, team.Id, opponentStats, opponentStats.IdTeam));
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // ACT
            await _sut.CancelMatch(team.Id, match.Id, "Cancelado por motivos técnicos");

            // ASSERT
            match.MatchStatus.Should().Be(MatchStatus.CANCELED);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "T2GP6 - Tenta cancelar partida com outro estado além de SCHEDULED.")]
        public async Task CancelMatch_Should_Throw_When_MatchHasInvalidStatus()
        {
            // ARRANGE
            var pitch = new Pitch("Campo", "Rua X");
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var team = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var opponent = new Team("Team B", "desc", new byte[] { 1 }, pitch, rank);
            var match = new Matches(DateTime.UtcNow.AddDays(1), false, pitch.Id, new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.DONE
            };
            match.Teams.Add(new TeamStatistics(team) { IdTeam = team.Id });
            match.Teams.Add(new TeamStatistics(opponent) { IdTeam = opponent.Id });
            var dto = new PostPoneMatchDto { IdMatch = match.Id, PostPoneDate = DateTime.UtcNow.AddDays(2) };
            _matchRepoMock.Setup(r => r.GetMatchToCancelById(match.Id)).ReturnsAsync(match);
            _validatorMock
                .Setup(v => v.ValidateCancelMatch(match, It.IsAny<TeamStatistics>(), team.Id, It.IsAny<TeamStatistics>(), opponent.Id))
                .Throws(new ValidationException("A partida não pode ser cancelada pois já foi concluída."));

            // ACT
            Func<Task> act = async () => await _sut.CancelMatch(team.Id, match.Id, "Motivo");

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pode ser cancelada*");
        }

        [Test(Description = "T3GP6 - Tentar cancelar partida que não existe.")]
        public async Task CancelMatch_Should_Throw_When_MatchDoesNotExist()
        {
            var teamId = Guid.NewGuid();
            var matchId = Guid.NewGuid();
            _matchRepoMock.Setup(r => r.GetMatchToCancelById(matchId)).ReturnsAsync((Matches?)null);

            Func<Task> act = async () => await _sut.CancelMatch(teamId, matchId, "Qualquer motivo");

            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("*não existe*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T4GP6 - Player não admin tenta cancelar partida.")]
        public async Task CancelMatch_Should_Throw_When_TeamIsNotAdmin()
        {
            var pitch = new Pitch("Campo", "Rua X");
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var team = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var opponent = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            var match = new Matches(DateTime.UtcNow.AddDays(1), false, pitch.Id, new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.SCHEDULED
            };
            match.Teams.Add(new TeamStatistics(team) { IdTeam = team.Id });
            match.Teams.Add(new TeamStatistics(opponent) { IdTeam = opponent.Id });
            _matchRepoMock.Setup(r => r.GetMatchToCancelById(match.Id)).ReturnsAsync(match);
            _validatorMock
                .Setup(v => v.ValidateCancelMatch(match, It.IsAny<TeamStatistics>(), team.Id, It.IsAny<TeamStatistics>(), opponent.Id))
                .Throws(new ValidationException("A equipa não é administradora da partida."));

            // ACT
            Func<Task> act = async () => await _sut.CancelMatch(team.Id, match.Id, "Motivo");

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administradora*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T5GP6 - Player não pertencente à equipa tenta cancelar partida.")]
        public async Task CancelMatch_Should_Throw_When_TeamDoesNotBelongToMatch()
        {
            // ARRANGE
            var pitch = new Pitch("Campo", "Rua X");
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var teamA = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var teamB = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            var outsider = new Team("Outsider", "desc", new byte[] { 3 }, pitch, rank);
            var match = new Matches(DateTime.UtcNow.AddDays(1), false, pitch.Id, new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.SCHEDULED
            };
            match.Teams.Add(new TeamStatistics(teamA) { IdTeam = teamA.Id });
            match.Teams.Add(new TeamStatistics(teamB) { IdTeam = teamB.Id });
            _matchRepoMock.Setup(r => r.GetMatchToCancelById(match.Id)).ReturnsAsync(match);
            _validatorMock
                .Setup(v => v.ValidateCancelMatch(match, null, outsider.Id, It.IsAny<TeamStatistics>(), It.IsAny<Guid>()))
                .Throws(new ValidationException("A equipa não pertence à partida."));

            // ACT
            Func<Task> act = async () => await _sut.CancelMatch(outsider.Id, match.Id, "Tentativa indevida");

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #region CalendarTests
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
        #endregion

        #endregion
    }
}