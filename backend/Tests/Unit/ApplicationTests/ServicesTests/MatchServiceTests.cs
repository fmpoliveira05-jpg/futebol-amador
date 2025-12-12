using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.Pitch;
using Application.DTOs.PostPoneGame;
using Application.DTOs.Team;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
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
        private Mock<INotificationFirebaseService> _notificationFireBase;
        private Mock<IPlayerRepository> _playerRepository;
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
            _playerRepository = new Mock<IPlayerRepository>();
            _notificationFireBase = new Mock<INotificationFirebaseService>();

            _sut = new MatchService(
                _matchRepoMock.Object,
                _teamPostPoneRepoMock.Object,
                _cancelledMatchRepoMock.Object,
                _unitOfWorkMock.Object,
                _validatorMock.Object,
                _playerRepository.Object,
                _notificationFireBase.Object
            );
        }
        #endregion

        #region Tests

        #region PostPoneMatchTests
        [Test(Description = "T1GP5 - Player Admin de equipa adia partida SCHEDULED para data futura (12h depois).")]
        public async Task PostPoneMatch_Should_ReturnInfoPostPoneMatch_When_AdminPostponesSuccessfully()
        {
            // ARRANGE
            var userId = "admin-id";
            var pitch = new Pitch("Campo Central", "Rua X");
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var team = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var opponent = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);

            var match = new Matches(DateTime.UtcNow.AddDays(1), false, pitch.Id, new List<TeamStatistics>(), new Chat())
            {
                Id = Guid.NewGuid(),
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
            _teamPostPoneRepoMock.Setup(r => r.AddTeamPostPoneMatch(It.IsAny<PostPoneMatch>()));
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // ACT
            var result = await _sut.PostPoneMatch(team.Id, dto);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().BeOfType<InfoPostPoneMatch>();
            result.IdMatch.Should().Be(match.Id);
            result.PostPoneDate.Should().Be(dto.PostPoneDate);

            // Verificar objetos aninhados
            result.Team.IdTeam.Should().Be(team.Id);
            result.Team.Name.Should().Be(team.Name);
            result.Opponent.IdTeam.Should().Be(opponent.Id);

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "T2GP5 - Player não admin tenta adiar partida.")]
        public async Task PostPoneMatch_Should_Throw_When_ValidatorFails()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var idMatch = Guid.NewGuid();
            var idOpponent = Guid.NewGuid();
            var dto = new PostPoneMatchDto
            {
                IdMatch = idMatch,
                PostPoneDate = DateTime.UtcNow.AddDays(1),
                IdOpponent = idOpponent
            };

            var match = new Matches(DateTime.UtcNow.AddDays(2), false, Guid.NewGuid(), new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.SCHEDULED
            };
            _matchRepoMock.Setup(r => r.GetMatchById(idMatch)).ReturnsAsync(match);


            // Configura o validador para lançar excepção (simulando falha de permissões ou outra regra)
            _validatorMock
                .Setup(v => v.ValidatePostPoneMatchDto(teamId, dto))
                .Throws(new ValidationException("O jogador não é administradora da equipa."));

            // ACT
            Func<Task> act = async () => await _sut.PostPoneMatch(teamId, dto);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*administradora*");

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Validação: Lança exceção se a equipa não pertencer à partida")]
        public async Task PostPoneMatch_Should_Throw_When_TeamDoesNotBelongToMatch()
        {
            // ARRANGE
            var outsiderTeamId = Guid.NewGuid();
            var idMatch = Guid.NewGuid();
            var idOpponent = Guid.NewGuid();

            var dto = new PostPoneMatchDto
            {
                IdMatch = idMatch,
                PostPoneDate = DateTime.UtcNow.AddDays(1),
                IdOpponent = idOpponent
            };

            var pitch = new Pitch("Campo", "Rua");
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var teamA = new Team("Team A", "desc", null, pitch, rank) { Id = Guid.NewGuid() };
            var teamB = new Team("Team B", "desc", null, pitch, rank) { Id = idOpponent };

            var match = new Matches(DateTime.UtcNow.AddDays(10), false, pitch.Id, new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.SCHEDULED
            };

            match.Teams.Add(new TeamStatistics(teamA) { IdTeam = teamA.Id });
            match.Teams.Add(new TeamStatistics(teamB) { IdTeam = teamB.Id });

            _matchRepoMock.Setup(r => r.GetMatchById(idMatch))
                          .ReturnsAsync(match);

            // ACT
            Func<Task> act = async () => await _sut.PostPoneMatch(outsiderTeamId, dto);

            // ASSERT
            await act.Should().ThrowAsync<BusinessRuleException>()
                     .WithMessage("A partida não possui equipas válidas.");

            // VERIFY
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T4GP5 - Tenta adiar partida com estado CANCELLED, DONE ou IN_PROGRESS.")]
        public async Task PostPoneMatch_Should_Throw_When_MatchHasInvalidStatus()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var opponentId = Guid.NewGuid();

            var pitch = new Pitch("Campo", "Rua");
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var team = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank) { Id = teamId };
            var opponent = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank) { Id = opponentId };

            var match = new Matches(DateTime.UtcNow.AddDays(1), false, Guid.NewGuid(), new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.DONE // Estado inválido para adiar
            };

            match.Teams.Add(new TeamStatistics(team) { IdTeam = teamId });
            match.Teams.Add(new TeamStatistics(opponent) { IdTeam = opponentId });

            var dto = new PostPoneMatchDto
            {
                IdMatch = match.Id,
                PostPoneDate = DateTime.UtcNow.AddDays(2),
                IdOpponent = opponentId
            };

            _matchRepoMock.Setup(r => r.GetMatchById(match.Id)).ReturnsAsync(match);

            // ACT
            Func<Task> act = async () => await _sut.PostPoneMatch(teamId, dto);

            // ASSERT
            await act.Should().ThrowAsync<BusinessRuleException>()
                     .WithMessage("Só podem ser adiadas partidas marcadas ou em estado de adiamento.");
        }

        [Test(Description = "T5GP5 - Tentar adiar partida para a mesma data definida.")]
        public async Task PostPoneMatch_Should_Throw_When_NewDateIsSame()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var opponentId = Guid.NewGuid();
            var sameDate = DateTime.UtcNow.AddDays(1);

            var pitch = new Pitch("Campo", "Rua");
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var team = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank) { Id = teamId };
            var opponent = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank) { Id = opponentId };

            var match = new Matches(sameDate, false, Guid.NewGuid(), new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.SCHEDULED
            };

            match.Teams.Add(new TeamStatistics(team) { IdTeam = teamId });
            match.Teams.Add(new TeamStatistics(opponent) { IdTeam = opponentId });

            var dto = new PostPoneMatchDto
            {
                IdMatch = match.Id,
                PostPoneDate = sameDate, // Mesma data
                IdOpponent = opponentId
            };

            _matchRepoMock.Setup(r => r.GetMatchById(match.Id)).ReturnsAsync(match);

            // ACT
            Func<Task> act = async () => await _sut.PostPoneMatch(teamId, dto);

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
            var opponentId = Guid.NewGuid();

            var pitch = new Pitch("Campo", "Rua");
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var team = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank) { Id = teamId };
            var opponent = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank) { Id = opponentId };

            var match = new Matches(DateTime.UtcNow.AddDays(1), false, Guid.NewGuid(), new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.SCHEDULED
            };

            match.Teams.Add(new TeamStatistics(team) { IdTeam = teamId });
            match.Teams.Add(new TeamStatistics(opponent) { IdTeam = opponentId });

            var invalidDate = DateTime.UtcNow.AddHours(-1); // Data no passado

            var dto = new PostPoneMatchDto
            {
                IdMatch = match.Id,
                PostPoneDate = invalidDate,
                IdOpponent = opponentId
            };

            _matchRepoMock.Setup(r => r.GetMatchById(match.Id)).ReturnsAsync(match);

            // ACT
            Func<Task> act = async () => await _sut.PostPoneMatch(teamId, dto);

            // ASSERT
            await act.Should().ThrowAsync<BusinessRuleException>()
                     .WithMessage("A nova data não pode ser igual ou antes da data atual.");
        }

        #endregion

        #region CancelMatchTests
        /*
        [Test(Description = "T1GP6 - Player Admin cancela partida com estado SCHEDULED.")]
        public async Task CancelMatch_Should_CancelScheduledMatch_When_AdminTeam()
        {
            // ARRANGE
            var userId = "admin-id";
            var pitch = new Pitch("Campo Central", "Rua X");
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var team = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var opponent = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            var match = new Matches(DateTime.UtcNow.AddDays(3), false, pitch.Id, new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.SCHEDULED
            };
            var teamStats = new TeamStatistics(team) { IdTeam = team.Id };
            var opponentStats = new TeamStatistics(opponent) { IdTeam = opponent.Id };
            match.Teams.Add(teamStats);
            match.Teams.Add(opponentStats);
            _matchRepoMock.Setup(r => r.GetMatchToCancelById(match.Id)).ReturnsAsync(match);
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
            var userId = "admin-id";
            var pitch = new Pitch("Campo", "Rua X");
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var team = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var opponent = new Team("Team B", "desc", new byte[] { 1 }, pitch, rank);
            var match = new Matches(DateTime.UtcNow.AddDays(3), false, pitch.Id, new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.DONE
            };
            match.Teams.Add(new TeamStatistics(team) { IdTeam = team.Id });
            match.Teams.Add(new TeamStatistics(opponent) { IdTeam = opponent.Id });
            var dto = new PostPoneMatchDto { IdMatch = match.Id, PostPoneDate = DateTime.UtcNow.AddDays(2) };
            _matchRepoMock.Setup(r => r.GetMatchToCancelById(match.Id)).ReturnsAsync(match);

            // ACT
            Func<Task> act = async () => await _sut.CancelMatch(team.Id, match.Id, "Motivo");

            // ASSERT
            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("O estado da partida tem de ser SCHEDULED.");
        }

        [Test(Description = "T3GP6 - Tentar cancelar partida que não existe.")]
        public async Task CancelMatch_Should_Throw_When_MatchDoesNotExist()
        {
            var userId = "admin-id";
            var teamId = Guid.NewGuid();
            var matchId = Guid.NewGuid();
            _matchRepoMock.Setup(r => r.GetMatchToCancelById(matchId)).ReturnsAsync((Matches?)null);

            Func<Task> act = async () => await _sut.CancelMatch(teamId, matchId, "Qualquer motivo");

            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("A partida não foi encontrada");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "T4GP6 - Jogador não administrador tenta cancelar partida.")]
        public async Task CancelMatch_Should_Throw_When_PlayerIsNotAdmin()
        {
            // ARRANGE
            var userId = "player-nonadmin-id";
            var pitch = new Pitch("Campo", "Rua X");
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var team = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var opponent = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            var player = new Player
            {
                Id = userId,
                IdTeam = team.Id,
                IsAdmin = false,
                Team = team
            };
            team.Members.Add(player);
            var match = new Matches(DateTime.UtcNow.AddDays(3), false, pitch.Id, new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.SCHEDULED
            };
            match.Teams.Add(new TeamStatistics(team) { IdTeam = team.Id });
            match.Teams.Add(new TeamStatistics(opponent) { IdTeam = opponent.Id });
            _matchRepoMock.Setup(r => r.GetMatchToCancelById(match.Id)).ReturnsAsync(match);

            // ACT
            Func<Task> act = async () => await _sut.CancelMatch(team.Id, match.Id, "Tentativa sem permissão");

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administradora*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never,
                "porque o jogador não é administrador e não deve conseguir cancelar a partida");
        }

        [Test(Description = "T5GP6 - Player não pertencente à equipa tenta cancelar partida.")]
        public async Task CancelMatch_Should_Throw_When_TeamDoesNotBelongToMatch()
        {
            // ARRANGE
            var userId = "admin-id";
            var pitch = new Pitch("Campo", "Rua X");
            var rank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);
            var teamA = new Team("Team A", "desc", new byte[] { 1 }, pitch, rank);
            var teamB = new Team("Team B", "desc", new byte[] { 2 }, pitch, rank);
            var outsider = new Team("Outsider", "desc", new byte[] { 3 }, pitch, rank);
            var match = new Matches(DateTime.UtcNow.AddDays(3), false, pitch.Id, new List<TeamStatistics>(), new Chat())
            {
                MatchStatus = MatchStatus.SCHEDULED
            };
            match.Teams.Add(new TeamStatistics(teamA) { IdTeam = teamA.Id });
            match.Teams.Add(new TeamStatistics(teamB) { IdTeam = teamB.Id });
            _matchRepoMock.Setup(r => r.GetMatchToCancelById(match.Id)).ReturnsAsync(match);

            // ACT
            Func<Task> act = async () => await _sut.CancelMatch(outsider.Id, match.Id, "Tentativa indevida");

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence*");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        */
        #endregion

        #region CalendarTests
        [Test(Description = "GetCalendar deve devolver todas as partidas para uma equipa")]
        public async Task GetCalendar_Should_Return_All_Matches_For_Team()
        {
            // ARRANGE
            var userId = "admin-id";
            var teamId = Guid.NewGuid();

            // Ajuste: Usar TeamStatisticsDto
            var team = new TeamStatisticsDto
            {
                IdTeam = teamId,
                Name = "FC Unity",
                NumGoals = 2
            };

            var opponent = new TeamStatisticsDto
            {
                IdTeam = Guid.NewGuid(),
                Name = "Real Opponent",
                NumGoals = 1
            };

            var pitch = new PitchDto { Name = "Campo Central", Address = "Rua Principal" };

            var matches = new List<InfoMatchCalendar>
            {
                new InfoMatchCalendar
                {
                    IdMatch = Guid.NewGuid(),
                    MatchStatus = MatchStatus.SCHEDULED,
                    GameDate = DateTime.Today.AddDays(1),
                    MatchResult = MatchResult.UNPLAYED, // Scheduled não tem resultado
                    IsCompetitive = true,
                    IsHome = true,
                    Team = new TeamStatisticsDto { IdTeam = teamId, Name = "FC Unity", NumGoals = 0 },
                    Opponent = new TeamStatisticsDto { IdTeam = Guid.NewGuid(), Name = "Real Opponent", NumGoals = 0 },
                    PitchGame = pitch
                },
                new InfoMatchCalendar
                {
                    IdMatch = Guid.NewGuid(),
                    MatchStatus = MatchStatus.DONE,
                    GameDate = DateTime.Today.AddDays(-1),
                    MatchResult = MatchResult.WIN,
                    IsCompetitive = true,
                    IsHome = false,
                    Team = team,
                    Opponent = opponent,
                    PitchGame = pitch
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
            var userId = "admin-id";
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

            // Ajuste para TeamStatisticsDto e campos obrigatórios
            var team = new TeamStatisticsDto { IdTeam = teamId, Name = "FC Unity", NumGoals = 3 };
            var opponent = new TeamStatisticsDto { IdTeam = Guid.NewGuid(), Name = "Real Opponent", NumGoals = 1 };
            var pitch = new PitchDto { Name = "Campo Norte", Address = "Avenida do Desporto" };

            var filteredMatches = new List<InfoMatchCalendar>
            {
                new InfoMatchCalendar
                {
                    IdMatch = Guid.NewGuid(),
                    MatchStatus = Domain.Enums.MatchStatus.DONE,
                    GameDate = DateTime.Today.AddDays(-2),
                    MatchResult = Domain.Enums.MatchResult.WIN,
                    IsCompetitive = true,
                    IsHome = true,
                    Team = team,
                    Opponent = opponent,
                    PitchGame = pitch
                }
            };

            _matchRepoMock.Setup(r => r.GetAllMatchesTeamWithFilters(teamId, filter))
                          .ReturnsAsync(filteredMatches);

            // ACT
            var result = await _sut.GetCalendarWithFilters(teamId, filter);

            // ASSERT
            result.Should().HaveCount(1, "porque há uma partida que cumpre os filtros");
            result.Should().BeEquivalentTo(filteredMatches, "porque o serviço apenas retorna o resultado do repositório");
            _matchRepoMock.Verify(r => r.GetAllMatchesTeamWithFilters(teamId, filter), Times.Once, "porque deve chamar o repositório uma única vez com os filtros aplicados");
        }

        [Test(Description = "GetCalendarWithFilters deve lançar exceção quando a data mínima for maior ou igual à data máxima")]
        public async Task GetCalendarWithFilters_Should_Throw_Exception_When_MinDate_Is_Greater_Than_MaxDate()
        {
            // ARRANGE
            var userId = "admin-id";
            var idTeam = Guid.NewGuid();
            var filter = new FilterCalendarDto
            {
                MinDate = new DateOnly(2025, 12, 15),
                MaxDate = new DateOnly(2025, 12, 14)
            };

            _validatorMock
                .Setup(v => v.ValidateFilterCalendar(idTeam, filter))
                .Throws(new InvalidOperationException("A data minima tem de ser inferior ou igual à data maxima"));

            // ACT
            Func<Task> act = async () => await _sut.GetCalendarWithFilters(idTeam, filter);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("A data minima tem de ser inferior ou igual à data maxima");
        }

        [Test(Description = "GetCalendarWithFilters deve lançar exceção quando o id da equipa for nulo")]
        public async Task GetCalendarWithFilters_Should_Throw_Exception_When_Team_Id_Is_Empty()
        {
            // ARRANGE
            var userId = "admin-id";
            var idTeam = Guid.Empty;
            var filter = new FilterCalendarDto
            {
                MinDate = new DateOnly(2025, 12, 1),
                MaxDate = new DateOnly(2025, 12, 31)
            };

            _validatorMock
                .Setup(v => v.ValidateFilterCalendar(idTeam, filter))
                .Throws(new InvalidOperationException("O id da equipa não pode estar vazio"));

            // ACT
            Func<Task> act = async () => await _sut.GetCalendarWithFilters(idTeam, filter);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("O id da equipa não pode estar vazio");
        }
        #endregion

        #endregion
    }
}