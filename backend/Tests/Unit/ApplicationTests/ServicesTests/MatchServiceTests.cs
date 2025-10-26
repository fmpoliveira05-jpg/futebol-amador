using Application.DTOs;
using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Repositories;
using Application.Interfaces.Validators;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Unit.ApplicationTests.ServicesTests
{
    [TestFixture]
    public class MatchServiceTests
    {
        private Mock<IMatchRepository> _matchRepoMock;
        private Mock<ITeamPostPoneGameRepository> _teamPostPoneRepoMock;
        private Mock<ICancelledMatchRepository> _cancelledMatchRepoMock;
        private Mock<IUnityOfWork> _unitOfWorkMock;
        private Mock<IMatchValidator> _validatorMock;
        private MatchService _sut;

        [SetUp]
        public void SetUp()
        {
            _matchRepoMock = new Mock<IMatchRepository>();
            _teamPostPoneRepoMock = new Mock<ITeamPostPoneGameRepository>();
            _cancelledMatchRepoMock = new Mock<ICancelledMatchRepository>();
            _unitOfWorkMock = new Mock<IUnityOfWork>();
            _validatorMock = new Mock<IMatchValidator>();

            _sut = new MatchService(
                _matchRepoMock.Object,
                _teamPostPoneRepoMock.Object,
                _cancelledMatchRepoMock.Object,
                _unitOfWorkMock.Object,
                _validatorMock.Object
            );
        }

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
            var filter = new FilterCalendar
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

        [Test(Description = "PostPoneMatch deve adiar a partida, atualizar estado e persistir")]
        public async Task PostPoneMatch_Should_Create_PostPoneRecord_And_Update_MatchStatus()
        {
            // ARRANGE
            var idMatch = Guid.NewGuid();
            var idTeam = Guid.NewGuid();
            var idOpponent = Guid.NewGuid();
            var newDate = DateTime.Now.AddDays(1);
            var dto = new PostponeMatchDTO
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
            var result = await _sut.PostPoneMatch(dto);

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

        [Test(Description = "AcceptPostPoneMatch deve aceitar adiamento, remover record e atualizar estado")]
        public async Task AcceptPostPoneMatch_Should_Remove_PostponeRecord_And_Update_Match()
        {
            // ARRANGE
            var idMatch = Guid.NewGuid();
            var idTeam = Guid.NewGuid();
            var idOpponent = Guid.NewGuid();
            var newDate = DateTime.Now.AddDays(2);
            var dto = new AcceptRefusePostPoneDTO
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
            var dto = new AcceptRefusePostPoneDTO
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

        [Test(Description = "FinishMatch deve finalizar a partida com sucesso quando os dados são válidos")]
        public async Task FinishMatch_Should_Finish_Match_Successfully_When_Valid()
        {
            // ARRANGE
            var idTeam = Guid.NewGuid();
            var idOpponent = Guid.NewGuid();
            var idMatch = Guid.NewGuid();
            var result = new ResultMatchDto
            {
                IdMatch = idMatch,
                IdTeam = idTeam,
                MyTeamGoals = 1,
                IdOpponent = idOpponent,
                OpponentGoals = 1
            };
            var team = new Teams("TeamA", "desc", new byte[] { 1 }, new Pitch("p", "a"), new Rank("R", 0, 0, 0, 0, null, null)) { Id = idTeam };
            var opponent = new Teams("TeamB", "desc", new byte[] { 1 }, new Pitch("p", "a"), new Rank("R", 0, 0, 0, 0, null, null)) { Id = idOpponent };
            var teamStat = new TeamStatistics(team) { IdTeam = idTeam };
            var opponentStat = new TeamStatistics(opponent) { IdTeam = idOpponent };
            var match = new Matches(DateTime.Now.AddDays(1), true, new Pitch("p", "a"))
            {
                Id = idMatch,
                MatchStatus = MatchStatus.IN_PROGRESS,
                Teams = new List<TeamStatistics> { teamStat, opponentStat }
            };
            _matchRepoMock.Setup(r => r.GetMatchInProgressByIdAsync(idMatch)).ReturnsAsync(match);
            _validatorMock.Setup(v => v.validateResultMatch(idTeam, result));
            _validatorMock.Setup(v => v.ValidateFinishMatch(match, teamStat, idTeam, opponentStat, idOpponent, result));

            // ACT
            await _sut.FinishMatch(idTeam, result);

            // ASSERT
            _validatorMock.Verify(v => v.validateResultMatch(idTeam, result), Times.Once);
            _validatorMock.Verify(v => v.ValidateFinishMatch(match, teamStat, idTeam, opponentStat, idOpponent, result), Times.Once);
        }

        [Test(Description = "FinishMatch deve lançar exceção se a partida não for encontrada")]
        public async Task FinishMatch_Should_Throw_Exception_When_Match_Not_Found()
        {
            // ARRANGE
            var idTeam = Guid.NewGuid();
            var idOpponent = Guid.NewGuid();
            var idMatch = Guid.NewGuid();
            var result = new ResultMatchDto
            {
                IdMatch = idMatch,
                IdTeam = idTeam,
                MyTeamGoals = 1,
                IdOpponent = idOpponent,
                OpponentGoals = 1
            };
            _matchRepoMock.Setup(r => r.GetMatchInProgressByIdAsync(idMatch)).ReturnsAsync((Matches)null);

            // ACT
            Func<Task> act = async () => await _sut.FinishMatch(idTeam, result);

            // ASSERT
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("A match não existe ou então não está em progresso");
        }

    }
}