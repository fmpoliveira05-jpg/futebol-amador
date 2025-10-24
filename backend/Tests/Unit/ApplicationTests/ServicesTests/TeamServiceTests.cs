using Application.DTOs;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Validators;
using Application.Services;
using Application.Validators;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unit.ApplicationTests.ServicesTests
{
    /// <summary>
    /// Testes unitários para a classe <see cref="TeamService"/>.
    /// Estes testes validam o comportamento da camada de aplicação responsável pela gestão de equipas.
    /// 
    /// - Utiliza o padrão "AAA" (Arrange, Act, Assert).
    /// - Usa mocks (via Moq) para simular dependências externas.
    /// - Aplica FluentAssertions para maior legibilidade dos asserts.
    /// </summary>
    [TestFixture]
    public class TeamServiceTests
    {
        // DEPENDÊNCIAS E CAMPOS COMUNS
        private Mock<ITeamRepository> _teamRepoMock;
        private Mock<IPlayerRepository> _playerRepoMock;
        private Mock<IUserRepository> _userRepoMock;
        private Mock<IUnityOfWork> _unitOfWorkMock;
        private Mock<IRankRepository> _rankRepoMock;
        private ITeamValidator _validatorReal;
        private TeamService _sut;

        /// <summary>
        /// Classe auxiliar interna para criar instâncias de Rank em contexto de teste.
        /// O construtor de <see cref="Rank"/> é protegido.
        /// </summary>
        private class TestRank : Rank
        {
            public TestRank(string name = "Bronze")
            {
                typeof(Rank).GetProperty(nameof(Name))!.SetValue(this, name);
                typeof(Rank).GetProperty(nameof(WinPoints))!.SetValue(this, 3);
                typeof(Rank).GetProperty(nameof(DrawPoints))!.SetValue(this, 1);
                typeof(Rank).GetProperty(nameof(LosePoints))!.SetValue(this, 0);
                typeof(Rank).GetProperty(nameof(PointsToPromotion))!.SetValue(this, 100);
            }
        }

        /// <summary>
        /// Executa antes de cada teste.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _teamRepoMock = new Mock<ITeamRepository>();
            _playerRepoMock = new Mock<IPlayerRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnityOfWork>();
            _rankRepoMock = new Mock<IRankRepository>();
            _validatorReal = new TeamValidator();

            _sut = new TeamService(
                _teamRepoMock.Object,
                _playerRepoMock.Object,
                _userRepoMock.Object,
                _unitOfWorkMock.Object,
                _validatorReal,
                _rankRepoMock.Object
            );
        }

        // TESTE 1: Criar equipa com sucesso (Rank padrão "Unranked")
        /// <summary>
        /// Garante que uma nova equipa é criada corretamente quando o Rank padrão "Unranked" existe.
        /// Este teste verifica:
        /// - Que o repositório recebe um objeto <see cref="Teams"/> com Rank "Unranked".
        /// - Que o método <see cref="IUnityOfWork.SaveChangesAsync"/> é chamado.
        /// - Que o método retorna um GUID válido.
        /// </summary>
        [Test(Description = "CreateTeamAsync deve criar uma nova equipa com Rank 'Unranked' quando o Rank padrão existe")]
        public async Task CreateTeamAsync_Should_Create_Team_With_DefaultRank_Unranked()
        {
            // ARRANGE
            var dto = new CreateTeamDto
            {
                Name = "FC Teste",
                Description = "Equipa de teste",
                icon = new byte[] { 1, 2, 3, 4 },
                HomePitch = new PitchDto { Name = "Campo Central", Address = "Rua Principal" }
            };

            var unrankedRank = new Rank("Unranked", 0, 0, 0, 0, null!, null!);

            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync((Teams)null);
            _rankRepoMock.Setup(r => r.GetDefaultRankAsync()).ReturnsAsync(unrankedRank);
            _teamRepoMock.Setup(r => r.AddAsync(It.IsAny<Teams>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            var result = await _sut.CreateTeamAsync(dto);

            // ASSERT
            result.Should().NotBeEmpty("porque deve retornar o ID da equipa criada");
            _teamRepoMock.Verify(r => r.AddAsync(It.Is<Teams>(t => t.Rank.Name == "Unranked")), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // TESTE 2: Criar equipa falha se não existir Rank padrão
        /// <summary>
        /// Garante que o método <see cref="TeamService.CreateTeamAsync"/> lança uma exceção de validação
        /// se o Rank padrão não estiver disponível no sistema.
        /// Isto simula um erro de configuração ou base de dados inconsistente.
        /// </summary>
        [Test(Description = "CreateTeamAsync deve lançar ValidationException quando o Rank padrão não existe")]
        public async Task CreateTeamAsync_Should_Throw_When_DefaultRank_IsMissing()
        {
            // ARRANGE
            var dto = new CreateTeamDto
            {
                Name = "FC SemRank",
                Description = "Teste",
                icon = new byte[] { 9, 9, 9 },
                HomePitch = new PitchDto { Name = "Campo", Address = "Rua" }
            };

            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync((Teams)null);
            _rankRepoMock.Setup(r => r.GetDefaultRankAsync()).ReturnsAsync((Rank)null!);

            // ACT
            Func<Task> act = async () => await _sut.CreateTeamAsync(dto);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*classificação padrão*");
        }

        // TESTE 3: Atualizar equipa com sucesso
        /// <summary>
        /// Verifica que uma equipa é atualizada com sucesso quando o jogador é administrador
        /// e os dados fornecidos são válidos.
        /// </summary>
        [Test(Description = "UpdateTeamInfoAsync deve atualizar os dados da equipa com sucesso")]
        public async Task UpdateTeamInfoAsync_Should_Update_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Teams("Antigo Nome", "Desc", new byte[] { 1, 2, 3 }, new Pitch("Campo", "Rua"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var player = new Player
            {
                Id = Guid.NewGuid(),
                IsAdmin = true,
                IdTeam = team.Id
            };
            team.Members.Add(player);
            var dto = new UpdateTeamDto { Name = "Novo Nome", Description = "Nova desc" };

            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync((Teams)null);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.UpdateTeamInfoAsync(team.Id, dto, player.Id);

            // ASSERT
            team.Name.Should().Be("Novo Nome");
            team.Description.Should().Be("Nova desc");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // TESTE 4: Remover jogador da equipa
        /// <summary>
        /// Verifica que um jogador pode ser removido da equipa por um administrador.
        /// Testa a interação entre repositórios e o estado interno da entidade <see cref="Teams"/>.
        /// </summary>
        [Test(Description = "RemovePlayerFromTeamAsync deve remover o jogador da equipa com sucesso")]
        public async Task RemovePlayerFromTeamAsync_Should_Remove_Player()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Teams("FC Test", "desc", new byte[] { 1, 2, 3 }, new Pitch("campo", "morada"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var playerToRemove = new Player { Id = Guid.NewGuid(), Name = "Jogador 1", IdTeam = team.Id };
            var playerRemoving = new Player { Id = Guid.NewGuid(), Name = "Admin", IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(playerToRemove);
            team.Members.Add(playerRemoving);

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToRemove.Id)).ReturnsAsync(playerToRemove);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerRemoving.Id)).ReturnsAsync(playerRemoving);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.RemovePlayerFromTeamAsync(team.Id, playerToRemove.Id, playerRemoving.Id);

            // ASSERT
            team.Members.Should().NotContain(playerToRemove, "porque o jogador foi removido da equipa");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // TESTE 5: Promover jogador a administrador
        /// <summary>
        /// Garante que um jogador pode ser promovido a administrador com sucesso.
        /// Valida que a propriedade <see cref="Player.IsAdmin"/> é atualizada e que o UnitOfWork é chamado.
        /// </summary>
        [Test(Description = "PromotePlayerToAdminAsync deve promover um jogador a admin")]
        public async Task PromotePlayerToAdminAsync_Should_Promote_Player()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Teams("FC Test", "desc", new byte[] { 1, 2, 3 }, new Pitch("campo", "morada"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var playerToPromote = new Player
            {
                Id = Guid.NewGuid(),
                Name = "Jogador 1",
                IsAdmin = false,
                IdTeam = team.Id
            };
            var playerPromoting = new Player
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                IsAdmin = true,
                IdTeam = team.Id
            };
            team.Members.Add(playerToPromote);
            team.Members.Add(playerPromoting);

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToPromote.Id)).ReturnsAsync(playerToPromote);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerPromoting.Id)).ReturnsAsync(playerPromoting);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.PromotePlayerToAdminAsync(team.Id, playerToPromote.Id, playerPromoting.Id);

            // ASSERT
            playerToPromote.IsAdmin.Should().BeTrue("porque foi promovido a administrador");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}