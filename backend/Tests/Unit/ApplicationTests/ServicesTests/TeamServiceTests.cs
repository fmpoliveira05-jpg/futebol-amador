using Application.DTOs;
using Application.DTOs.MemberShip;
using Application.DTOs.Pitch;
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
    /// Estes testes validam o comportamento da camada de aplicação responsável pela gestão de equipas, gestão de jogadores e gestão de administradores.
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
        private readonly Mock<IMembershipRequestRepository> _membershipRequestRepoMock = new();
        private readonly Mock<IPlayerValidator> _playerValidatorMock = new();

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
        /// Classe auxiliar interna para criar instâncias de pedidos de adesão em contexto de teste.
        /// O construtor de <see cref="MembershipRequests"/> é protegido.
        /// </summary>
        private static MembershipRequests CreateMembershipRequest(Guid id, Guid playerId)
        {
            var request = (MembershipRequests)Activator.CreateInstance(typeof(MembershipRequests), nonPublic: true)!;
            request.GetType().GetProperty(nameof(MembershipRequests.Id))!.SetValue(request, id);
            request.GetType().GetProperty(nameof(MembershipRequests.IdPlayer))!.SetValue(request, playerId);
            return request;
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
            _rankRepoMock.Object,
            _membershipRequestRepoMock.Object,
            _playerValidatorMock.Object
            );
        }

        // TESTE T1GE1: Criar equipa com sucesso (Rank padrão "Unranked")
        /// <summary>
        /// Garante que uma nova equipa é criada corretamente quando o Rank padrão "Unranked" existe.
        /// Este teste verifica:
        /// - Que o repositório recebe um objeto <see cref="Teams"/> com Rank "Unranked".
        /// - Que o método <see cref="IUnityOfWork.SaveChangesAsync"/> é chamado.
        /// - Que o método retorna um GUID válido.
        /// - FALTA GARANTIR QUE O JOGADOR GANHA O ROLE DE ADMIN DA EQUIPA!!! O WILLKIE TEM DE ACRESCENTAR AO MÉTODO DELE!!!
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
            var adminPlayer = new Player { Id = Guid.NewGuid(), IsAdmin = true };
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminPlayer.Id)).ReturnsAsync(adminPlayer);

            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync((Teams)null);
            _rankRepoMock.Setup(r => r.GetDefaultRankAsync()).ReturnsAsync(unrankedRank);
            _teamRepoMock.Setup(r => r.AddAsync(It.IsAny<Teams>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            var result = await _sut.CreateTeamAsync(dto, adminPlayer.Id);

            // ASSERT
            result.Should().NotBeEmpty("porque deve retornar o ID da equipa criada");
            _teamRepoMock.Verify(r => r.AddAsync(It.Is<Teams>(t => t.Rank.Name == "Unranked")), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // TESTE T2GE1: Criar equipa falha se já existir uma com o mesmo nome
        /// <summary>
        /// Garante que o método <see cref="TeamService.CreateTeamAsync"/> lança uma exceção
        /// quando o nome da equipa já existe no sistema.
        /// Este teste valida que:
        /// - O repositório retorna uma equipa existente com o mesmo nome.
        /// - É lançada uma <see cref="ValidationException"/> pelo validador.
        /// - Nenhuma nova equipa é adicionada ao repositório.
        /// </summary>
        [Test(Description = "CreateTeamAsync deve lançar exceção se já existir uma equipa com o mesmo nome")]
        public async Task CreateTeamAsync_Should_Throw_When_Team_Name_Already_Exists()
        {
            // ARRANGE
            var dto = new CreateTeamDto
            {
                Name = "FC Repetido",
                Description = "Equipa duplicada",
                icon = new byte[] { 1, 1, 1 },
                HomePitch = new PitchDto { Name = "Campo Velho", Address = "Rua da Bola" }
            };
            var rank = new TestRank("Unranked");
            var existingTeam = new Teams(dto.Name, "Outra equipa", new byte[] { 9, 9 }, new Pitch("Campo Antigo", "Rua Antiga"), rank);
            var adminPlayer = new Player { Id = Guid.NewGuid(), IsAdmin = true };
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync(existingTeam);
            _rankRepoMock.Setup(r => r.GetDefaultRankAsync()).ReturnsAsync(rank);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminPlayer.Id)).ReturnsAsync(adminPlayer);

            // ACT
            Func<Task> act = async () => await _sut.CreateTeamAsync(dto, adminPlayer.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*já existe*", "porque o nome da equipa não pode ser duplicado");
            _teamRepoMock.Verify(r => r.AddAsync(It.IsAny<Teams>()), Times.Never, "porque não deve tentar adicionar uma equipa duplicada");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque nenhuma alteração deve ser persistida");
        }

        // TESTE T3GE1: Criar equipa falha quando o jogador já é administrador de outra equipa
        /// <summary>
        /// Garante que um jogador que já é administrador de uma equipa não pode criar uma nova.
        /// Este teste valida que:
        /// - O jogador já pertence a uma equipa e é administrador.
        /// - O método <see cref="TeamService.CreateTeamAsync"/> lança uma <see cref="ValidationException"/>.
        /// - Nenhuma nova equipa é adicionada nem persistida.
        /// </summary>
        [Test(Description = "CreateTeamAsync deve lançar exceção se o jogador já for administrador de uma equipa")]
        public async Task CreateTeamAsync_Should_Throw_When_Player_Is_Already_Admin()
        {
            // ARRANGE
            var rank = new TestRank("Unranked");
            var existingTeam = new Teams("FC Alpha", "Equipa atual", new byte[] { 1 }, new Pitch("Campo 1", "Rua 1"), rank);
            var adminPlayer = new Player
            {
                Id = Guid.NewGuid(),
                IdTeam = existingTeam.Id,
                IsAdmin = true
            };
            existingTeam.Members.Add(adminPlayer);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminPlayer.Id)).ReturnsAsync(adminPlayer);
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync("FC Nova")).ReturnsAsync((Teams)null);
            _rankRepoMock.Setup(r => r.GetDefaultRankAsync()).ReturnsAsync(rank);

            var dto = new CreateTeamDto
            {
                Name = "FC Nova",
                Description = "Nova equipa criada por admin indevido",
                icon = new byte[] { 5, 5, 5 },
                HomePitch = new PitchDto { Name = "Campo Novo", Address = "Rua Nova" }
            };

            // ACT
            Func<Task> act = async () => await _sut.CreateTeamAsync(dto, adminPlayer.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*ja possui uma equipa*", "porque um jogador admin não pode criar uma nova equipa");

            _teamRepoMock.Verify(r => r.AddAsync(It.IsAny<Teams>()), Times.Never, "porque a criação deve ser bloqueada");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        // TESTE T4GE1: Criar equipa falha se não existir Rank padrão
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
            var adminPlayer = new Player { Id = Guid.NewGuid(), IsAdmin = true };
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync((Teams)null);
            _rankRepoMock.Setup(r => r.GetDefaultRankAsync()).ReturnsAsync((Rank)null!);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminPlayer.Id)).ReturnsAsync(adminPlayer);

            // ACT
            Func<Task> act = async () => await _sut.CreateTeamAsync(dto, adminPlayer.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*classificação padrão*");
        }

        // T1GE2: Atualizar equipa com sucesso (sendo admin)
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

        // TESTE T2GE2: Atualizar equipa falha quando o jogador não é administrador (também falha para jogadores sem clube)
        /// <summary>
        /// Garante que um jogador sem permissões de administrador não pode atualizar os dados da equipa.
        /// Este teste verifica que:
        /// - É lançada uma <see cref="ValidationException"/> com a mensagem apropriada.
        /// - Nenhuma alteração é persistida na base de dados.
        /// </summary>
        [Test(Description = "UpdateTeamInfoAsync deve lançar exceção se o jogador não for administrador")]
        public async Task UpdateTeamInfoAsync_Should_Throw_If_Not_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Teams("Antigo Nome", "Desc", new byte[] { 1, 2, 3 }, new Pitch("Campo", "Rua"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var nonAdmin = new Player
            {
                Id = Guid.NewGuid(),
                IsAdmin = false,
                IdTeam = team.Id
            };
            team.Members.Add(nonAdmin);
            var dto = new UpdateTeamDto { Name = "Novo Nome", Description = "Nova desc" };
            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdmin.Id)).ReturnsAsync(nonAdmin);
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync((Teams)null);

            // ACT
            Func<Task> act = async () => await _sut.UpdateTeamInfoAsync(team.Id, dto, nonAdmin.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*", "porque apenas administradores podem atualizar os dados da equipa");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque a atualização não deve ser persistida");
        }

        // TESTE T3GE2: Atualizar equipa falha quando o admin pertence a outra equipa
        /// <summary>
        /// Garante que um administrador de outra equipa não consegue atualizar os dados
        /// de uma equipa à qual não pertence.
        /// Este teste valida a integridade das permissões de administrador entre equipas.
        /// </summary>
        [Test(Description = "UpdateTeamInfoAsync deve lançar exceção se o admin pertencer a outra equipa")]
        public async Task UpdateTeamInfoAsync_Should_Throw_If_Admin_From_Another_Team()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Teams("Team A", "Desc A", new byte[] { 1 }, new Pitch("Campo A", "Rua A"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var teamB = new Teams("Team B", "Desc B", new byte[] { 2 }, new Pitch("Campo B", "Rua B"), rank)
            {
                Id = Guid.NewGuid()
            };
            var adminOfTeamA = new Player
            {
                Id = Guid.NewGuid(),
                IdTeam = teamA.Id,
                IsAdmin = true
            };
            teamA.Members.Add(adminOfTeamA);
            var dto = new UpdateTeamDto { Name = "Novo Nome" };
            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(teamB.Id)).ReturnsAsync(teamB);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminOfTeamA.Id)).ReturnsAsync(adminOfTeamA);

            // ACT
            Func<Task> act = async () => await _sut.UpdateTeamInfoAsync(teamB.Id, dto, adminOfTeamA.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não pertence à equipa*", "porque o admin pertence a outra equipa e não deve ter acesso");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        // TESTE T4GE2: Atualizar equipa falha quando a equipa não existe
        /// <summary>
        /// Garante que o método lança uma exceção quando se tenta atualizar uma equipa inexistente.
        /// Este teste valida que:
        /// - O repositório retorna null.
        /// - É lançada uma <see cref="NotFoundException"/>.
        /// - Nenhuma alteração é persistida.
        /// </summary>
        [Test(Description = "UpdateTeamInfoAsync deve lançar exceção se a equipa não existir")]
        public async Task UpdateTeamInfoAsync_Should_Throw_If_Team_Not_Found()
        {
            // ARRANGE
            var rank = new TestRank();
            var dto = new UpdateTeamDto { Name = "Novo Nome" };
            var admin = new Player { Id = Guid.NewGuid(), IdTeam = Guid.NewGuid(), IsAdmin = true };
            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(It.IsAny<Guid>())).ReturnsAsync((Teams)null!);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);

            // ACT
            Func<Task> act = async () => await _sut.UpdateTeamInfoAsync(Guid.NewGuid(), dto, admin.Id);

            // ASSERT
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("*não existe*", "porque a equipa não existe na base de dados");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque não deve persistir alterações");
        }

        // TESTE T5GE2: Atualizar equipa falha quando o novo nome já pertence a outra equipa
        /// <summary>
        /// Garante que o método <see cref="TeamService.UpdateTeamInfoAsync"/> lança uma exceção
        /// quando o novo nome indicado já pertence a outra equipa do sistema.
        /// Este teste valida que:
        /// - O repositório encontra uma equipa existente com o mesmo nome.
        /// - O método lança uma <see cref="ValidationException"/> devido à duplicação de nome.
        /// - Nenhuma atualização é persistida.
        /// </summary>
        [Test(Description = "UpdateTeamInfoAsync deve lançar exceção se o novo nome já pertencer a outra equipa")]
        public async Task UpdateTeamInfoAsync_Should_Throw_When_NewName_Already_Exists()
        {
            // ARRANGE
            var rank = new TestRank();
            var teamA = new Teams("FC Original", "Desc A", new byte[] { 1 }, new Pitch("Campo A", "Rua A"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var teamB = new Teams("FC Existente", "Desc B", new byte[] { 2 }, new Pitch("Campo B", "Rua B"), rank)
            {
                Id = Guid.NewGuid()
            };
            var admin = new Player
            {
                Id = Guid.NewGuid(),
                IdTeam = teamA.Id,
                IsAdmin = true
            };
            teamA.Members.Add(admin);
            var dto = new UpdateTeamDto { Name = "FC Existente" };
            _teamRepoMock.Setup(r => r.GetTeamForUpdateAsync(teamA.Id)).ReturnsAsync(teamA);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);
            _teamRepoMock.Setup(r => r.GetTeamByNameAsync(dto.Name)).ReturnsAsync(teamB);

            // ACT
            Func<Task> act = async () => await _sut.UpdateTeamInfoAsync(teamA.Id, dto, admin.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*já existe*", "porque o nome da equipa já está a ser usado por outra equipa");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque a alteração não deve ser persistida");
        }

        // TESTE 3A: Eliminar equipa (sendo admin)
        /// <summary>
        /// Garante que um administrador pode eliminar uma equipa com sucesso.
        /// Este teste valida que:
        /// - O repositório de equipas é chamado para eliminar a entidade correta.
        /// - O método <see cref="IUnityOfWork.SaveChangesAsync"/> é executado uma única vez.
        /// - Não é lançada nenhuma exceção durante a operação.
        /// </summary>
        [Test(Description = "DeleteTeamAsync deve eliminar a equipa quando o utilizador é administrador")]
        public async Task DeleteTeamAsync_Should_Delete_When_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Teams("ToDelete", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank);
            var admin = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForDeletionAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.DeleteTeamAsync(team.Id, admin.Id);

            // ASSERT
            _teamRepoMock.Verify(r => r.DeleteTeam(team), Times.Once, "porque o repositório deve eliminar a equipa");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once, "porque a operação deve ser persistida na base de dados");
        }


        // TESTE 3B: Eliminar equipa (não sendo admin)
        /// <summary>
        /// Garante que um jogador comum (não administrador) não tem permissão para eliminar uma equipa.
        /// Este teste assegura que:
        /// - O método <see cref="TeamService.DeleteTeamAsync"/> lança uma <see cref="ValidationException"/>.
        /// - A exceção contém uma mensagem informando que o jogador não é administrador.
        /// - Nenhuma operação de persistência é realizada.
        /// </summary>
        [Test(Description = "DeleteTeamAsync deve lançar exceção quando o utilizador não é administrador")]
        public async Task DeleteTeamAsync_Should_Throw_If_Not_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Teams("FailDelete", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank);
            var player = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(player);
            _teamRepoMock.Setup(r => r.GetTeamForDeletionAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.DeleteTeamAsync(team.Id, player.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*", "porque apenas administradores podem eliminar equipas");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque não deve ser feita nenhuma alteração à base de dados");
        }

        // TESTE 4A: Remover jogador da equipa (sendo admin)
        /// <summary>
        /// Garante que um jogador pode ser removido de uma equipa com sucesso por um administrador.
        /// Este teste verifica:
        /// - Que o jogador removido deixa de pertencer à coleção <see cref="Teams.Members"/>.
        /// - Que o método <see cref="IUnityOfWork.SaveChangesAsync"/> é chamado uma vez.
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

        // TESTE 4B: Remover jogador da equipa falha quando o jogador que remove não é admin
        /// <summary>
        /// Garante que um jogador comum (não administrador) não pode remover outros membros da equipa.
        /// Este teste assegura que:
        /// - O método <see cref="TeamService.RemovePlayerFromTeamAsync"/> lança uma <see cref="ValidationException"/>.
        /// - Nenhum membro é removido da equipa.
        /// - A operação de persistência não é executada.
        /// </summary>
        [Test(Description = "RemovePlayerFromTeamAsync deve lançar exceção se o jogador não for administrador")]
        public async Task RemovePlayerFromTeamAsync_Should_Throw_If_NonAdmin_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Teams("FC Test", "desc", new byte[] { 1, 2, 3 }, new Pitch("campo", "morada"), rank)
            {
                Id = Guid.NewGuid(),
                Members = new List<Player>()
            };
            var playerToRemove = new Player { Id = Guid.NewGuid(), Name = "Jogador 1", IdTeam = team.Id };
            var playerRemoving = new Player { Id = Guid.NewGuid(), Name = "Jogador Comum", IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(playerToRemove);
            team.Members.Add(playerRemoving);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerToRemove.Id)).ReturnsAsync(playerToRemove);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerRemoving.Id)).ReturnsAsync(playerRemoving);

            // ACT
            Func<Task> act = async () => await _sut.RemovePlayerFromTeamAsync(team.Id, playerToRemove.Id, playerRemoving.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*", "porque apenas administradores podem remover jogadores da equipa");
            team.Members.Should().Contain(playerToRemove, "porque o jogador não deve ser removido");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "porque não deve haver persistência sem permissões");
        }

        // TESTE 5A: Promover jogador da equipa (sendo admin)
        /// <summary>
        /// Garante que um administrador pode promover um jogador comum a administrador.
        /// Este teste valida:
        /// - Que o jogador alvo passa a ter <see cref="Player.IsAdmin"/> igual a true.
        /// - Que a operação de persistência é executada com sucesso.
        /// </summary>
        [Test(Description = "PromotePlayerToAdminAsync deve promover um jogador comum a admin quando o promotor é admin")]
        public async Task PromotePlayerToAdminAsync_Should_Work_When_Admin_Promotes_Member()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Teams("FC Unity", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank);
            var admin = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = true };
            var member = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(admin);
            team.Members.Add(member);

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(member.Id)).ReturnsAsync(member);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.PromotePlayerToAdminAsync(team.Id, member.Id, admin.Id);

            // ASSERT
            member.IsAdmin.Should().BeTrue("porque o jogador foi promovido por um administrador");
        }


        // TESTE 5B: Promover jogador da equipa (sendo jogador)
        /// <summary>
        /// Garante que um jogador comum (não administrador) não pode promover outro jogador.
        /// Este teste verifica que:
        /// - O método lança uma <see cref="ValidationException"/> com a mensagem esperada.
        /// </summary>
        [Test(Description = "PromotePlayerToAdminAsync deve lançar exceção se o promotor não for admin")]
        public async Task PromotePlayerToAdminAsync_Should_Throw_If_NonAdmin_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Teams("Team", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank);
            var memberPromoting = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = false };
            var memberToPromote = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = false };
            team.Members.Add(memberPromoting);
            team.Members.Add(memberToPromote);

            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(memberPromoting.Id)).ReturnsAsync(memberPromoting);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(memberToPromote.Id)).ReturnsAsync(memberToPromote);

            // ACT
            Func<Task> act = async () => await _sut.PromotePlayerToAdminAsync(team.Id, memberToPromote.Id, memberPromoting.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*");
        }


        // TESTE 6A: Rebaixar admin da equipa (sendo admin)
        /// <summary>
        /// Verifica que um administrador pode rebaixar outro administrador a jogador comum.
        /// Este teste assegura que:
        /// - A flag <see cref="Player.IsAdmin"/> é alterada para false.
        /// - A operação de persistência é realizada com sucesso.
        /// </summary>
        [Test(Description = "DemoteAdminToPlayerAsync deve permitir que um admin rebaixe outro admin")]
        public async Task DemoteAdminToPlayerAsync_Should_Work_When_Admin_Relegates_Admin()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Teams("Team", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank);
            var admin1 = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = true };
            var admin2 = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(admin1);
            team.Members.Add(admin2);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin1.Id)).ReturnsAsync(admin1);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin2.Id)).ReturnsAsync(admin2);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.DemoteAdminToPlayerAsync(team.Id, admin2.Id, admin1.Id);

            // ASSERT
            admin2.IsAdmin.Should().BeFalse("porque foi rebaixado por outro administrador");
        }


        // TESTE 6B: Rebaixar jogador (sendo jogador)
        /// <summary>
        /// Garante que um jogador comum não pode rebaixar outro membro da equipa (seja admin ou não).
        /// Este teste valida que:
        /// - O método lança uma <see cref="ValidationException"/>.
        /// - A exceção contém a mensagem esperada.
        /// </summary>
        [Test(Description = "DemoteAdminToPlayerAsync deve lançar exceção se o jogador não for admin")]
        public async Task DemoteAdminToPlayerAsync_Should_Throw_If_NonAdmin_Tries()
        {
            // ARRANGE
            var rank = new TestRank();
            var team = new Teams("Team", "Desc", new byte[1], new Pitch("Campo", "Rua"), rank);
            var nonAdmin = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = false };
            var admin = new Player { Id = Guid.NewGuid(), IdTeam = team.Id, IsAdmin = true };
            team.Members.Add(nonAdmin);
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdmin.Id)).ReturnsAsync(nonAdmin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(admin.Id)).ReturnsAsync(admin);

            // ACT
            Func<Task> act = async () => await _sut.DemoteAdminToPlayerAsync(team.Id, admin.Id, nonAdmin.Id);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*");
        }


        // TESTE 7A: Obter equipa por ID
        /// <summary>
        /// Verifica que o método devolve corretamente os detalhes de uma equipa existente.
        /// Este teste confirma que:
        /// - O resultado é equivalente ao DTO retornado pelo repositório.
        /// </summary>
        [Test(Description = "GetTeamByIdAsync deve devolver os detalhes de uma equipa existente")]
        public async Task GetTeamByIdAsync_Should_Return_Details()
        {
            // ARRANGE
            var teamDetails = new TeamDetailsDto { Name = "Team X" };
            _teamRepoMock.Setup(r => r.GetTeamDetailsDtoAsync(It.IsAny<Guid>())).ReturnsAsync(teamDetails);

            // ACT
            var result = await _sut.GetTeamByIdAsync(Guid.NewGuid());

            // ASSERT
            result.Should().BeEquivalentTo(teamDetails);
        }


        // TESTE 7B: Obter equipa por ID inexistente
        /// <summary>
        /// Garante que o método lança uma <see cref="NotFoundException"/> quando a equipa não é encontrada.
        /// </summary>
        [Test(Description = "GetTeamByIdAsync deve lançar NotFoundException se a equipa não existir")]
        public async Task GetTeamByIdAsync_Should_Throw_If_NotFound()
        {
            // ARRANGE
            _teamRepoMock.Setup(r => r.GetTeamDetailsDtoAsync(It.IsAny<Guid>())).ReturnsAsync((TeamDetailsDto)null!);

            // ACT
            Func<Task> act = async () => await _sut.GetTeamByIdAsync(Guid.NewGuid());

            // ASSERT
            await act.Should().ThrowAsync<NotFoundException>();
        }


        // TESTE 8: Obter jogadores da equipa
        /// <summary>
        /// Verifica que o método devolve corretamente a lista de jogadores de uma equipa.
        /// Este teste assegura que:
        /// - Todos os membros da equipa são convertidos em DTOs <see cref="PlayerDetailsDTO"/>.
        /// - O número e os nomes dos jogadores estão corretos.
        /// </summary>
        [Test(Description = "GetTeamPlayersAsync deve devolver a lista de jogadores da equipa")]
        public async Task GetTeamPlayersAsync_Should_Return_Player_List()
        {
            // ARRANGE
            var team = new Teams("T", "Desc", new byte[1], new Pitch("Campo", "Rua"), new TestRank());
            var p1 = new Player { Name = "A", IdTeam = team.Id };
            var p2 = new Player { Name = "B", IdTeam = team.Id };
            team.Members.Add(p1);
            team.Members.Add(p2);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(team.Id)).ReturnsAsync(team);

            // ACT
            var result = await _sut.GetTeamPlayersAsync(team.Id);

            // ASSERT
            result.Should().HaveCount(2);
            result.Select(p => p.Name).Should().Contain(new[] { "A", "B" });
        }

        // ==========================================================
        // TESTE 9: Aceitar pedido de adesão (sendo admin)
        // ==========================================================
        /// <summary>
        /// Garante que um administrador pode aceitar um pedido de adesão com sucesso.
        /// Este teste verifica que:
        /// - O pedido é removido da lista de pedidos da equipa e do jogador.
        /// - O jogador é adicionado à lista de membros da equipa.
        /// - As alterações são persistidas na base de dados.
        /// </summary>
        [Test(Description = "AcceptMembershipRequestAsync deve aceitar o pedido de adesão com sucesso quando o utilizador é admin")]
        public async Task AcceptMembershipRequestAsync_Should_Add_Player_To_Team_When_Admin_Accepts()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var rank = new TestRank();
            var team = new Teams("FC Unity", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var request = CreateMembershipRequest(requestId, playerId);
            team.MembershipRequests = team.MembershipRequests ?? new List<MembershipRequests>();
            team.MembershipRequests.Add(request);
            var admin = new Player { Id = adminId, IdTeam = team.Id, IsAdmin = true };
            var player = new Player { Id = playerId, IdTeam = Guid.Empty, MembershipRequests = new List<MembershipRequests> { request } };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.AcceptMembershipRequestAsync(teamId, requestId, adminId);

            // ASSERT
            team.Members.Should().Contain(player);
            team.MembershipRequests.Should().BeEmpty();
            player.MembershipRequests.Should().BeEmpty();
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // ==========================================================
        // TESTE 10A: Rejeitar pedido de adesão (sendo admin)
        // ==========================================================
        /// <summary>
        /// Garante que um administrador pode rejeitar um pedido de adesão.
        /// Este teste verifica que:
        /// - O pedido é removido da equipa e do jogador.
        /// - As alterações são persistidas.
        /// </summary>
        [Test(Description = "RejectMembershipRequestAsync deve rejeitar o pedido de adesão com sucesso quando o utilizador é admin")]
        public async Task RejectMembershipRequestAsync_Should_Remove_Request_When_Admin_Rejects()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var rank = new TestRank();
            var team = new Teams("FC Reject", "desc", new byte[1], new Pitch("Campo", "Rua"), rank) { Id = teamId };
            var request = CreateMembershipRequest(requestId, playerId);
            team.MembershipRequests.Add(request);
            var admin = new Player { Id = adminId, IdTeam = team.Id, IsAdmin = true };
            var player = new Player { Id = playerId, IdTeam = Guid.Empty, MembershipRequests = new List<MembershipRequests> { request } };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            // ACT
            await _sut.RejectMembershipRequestAsync(teamId, requestId, adminId);

            // ASSERT
            team.MembershipRequests.Should().BeEmpty("porque o pedido foi removido da equipa");
            player.MembershipRequests.Should().BeEmpty("porque o pedido foi removido do jogador");
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }


        // ==========================================================
        // TESTE 10B: Rejeitar pedido de adesão (sendo não admin)
        // ==========================================================
        /// <summary>
        /// Garante que um jogador comum (não administrador) não pode rejeitar pedidos de adesão.
        /// </summary>
        [Test(Description = "RejectMembershipRequestAsync deve lançar exceção se o utilizador não for admin")]
        public async Task RejectMembershipRequestAsync_Should_Throw_If_Not_Admin()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var nonAdminId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var rank = new TestRank();
            var team = new Teams("FC Reject", "desc", new byte[1], new Pitch("Campo", "Rua"), rank)
            {
                Id = teamId,
                MembershipRequests = new List<MembershipRequests> { CreateMembershipRequest(requestId, playerId) }
            };
            var nonAdmin = new Player { Id = nonAdminId, IdTeam = team.Id, IsAdmin = false };
            var player = new Player { Id = playerId };
            team.Members.Add(nonAdmin);
            _teamRepoMock.Setup(r => r.GetTeamForMembershipRequestAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(nonAdminId)).ReturnsAsync(nonAdmin);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            // ACT
            Func<Task> act = async () => await _sut.RejectMembershipRequestAsync(teamId, requestId, nonAdminId);

            // ASSERT
            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*não é administrador*");
        }


        // ==========================================================
        // TESTE 11: Consultar pedidos de adesão (sendo admin)
        // ==========================================================
        /// <summary>
        /// Garante que um administrador pode consultar a lista de pedidos de adesão pendentes.
        /// Este teste verifica que:
        /// - O método devolve a lista de pedidos obtida do repositório.
        /// - Nenhuma exceção é lançada.
        /// </summary>
        [Test(Description = "GetMembershipRequestsAsync deve devolver os pedidos de adesão quando o utilizador é admin")]
        public async Task GetMembershipRequestsAsync_Should_Return_Requests_When_Admin()
        {
            // ARRANGE
            var teamId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var rank = new TestRank();
            var team = new Teams("FC Requests", "desc", new byte[1], new Pitch("Campo", "Rua"), rank)
            {
                Id = teamId
            };
            var admin = new Player { Id = adminId, IdTeam = team.Id, IsAdmin = true };
            var requests = new List<MemberShipRequestDto>
            {
                new MemberShipRequestDto { PlayerName = "Jogador 1" },
                new MemberShipRequestDto { PlayerName = "Jogador 2" }
            };
            team.Members.Add(admin);
            _teamRepoMock.Setup(r => r.GetTeamForMemberManagementAsync(teamId)).ReturnsAsync(team);
            _playerRepoMock.Setup(r => r.GetPlayerByIdAsync(adminId)).ReturnsAsync(admin);
            _teamRepoMock.Setup(r => r.GetMembershipRequestsDtoAsync(teamId)).ReturnsAsync(requests);

            // ACT
            var result = await _sut.GetMembershipRequestsAsync(teamId, adminId);

            // ASSERT
            result.Should().HaveCount(2, "porque há dois pedidos pendentes");
            result.Select(r => r.PlayerName).Should().Contain(new[] { "Jogador 1", "Jogador 2" });
        }

    }
}