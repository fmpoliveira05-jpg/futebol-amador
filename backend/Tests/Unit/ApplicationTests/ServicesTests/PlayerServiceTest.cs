using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Rank;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Moq;
using NUnit.Framework;

namespace Tests.Unit.ApplicationTests.ServicesTests
{
    [TestFixture]
    public class PlayerServiceTest
    {
        #region Variables
        private Mock<IPlayerRepository> playerRepoMock;
        private Mock<ITeamService> teamServiceMock;
        private Mock<IUserRepository> userRepoMock;
        private Mock<IUnityOfWork> uowMock;
        private Mock<IPlayerValidator> validatorMock;
        private Mock<ITeamRepository> teamRepoMock;
        private Mock<IMembershipRequestRepository> membershipReqRepoMock;
        private Mock<IPlayerAuthorizationValidator> authorizationValidatorMock;
        private Mock<IUserDataValidator> userDataValidator;
        private Mock<IAuthService> authService;

        private PlayerService service;

        #endregion

        #region SetUp
        [SetUp]
        public void SetUp()
        {
            playerRepoMock = new Mock<IPlayerRepository>();
            teamServiceMock = new Mock<ITeamService>();
            userRepoMock = new Mock<IUserRepository>();
            uowMock = new Mock<IUnityOfWork>();
            validatorMock = new Mock<IPlayerValidator>();
            teamRepoMock = new Mock<ITeamRepository>();
            membershipReqRepoMock = new Mock<IMembershipRequestRepository>();
            authorizationValidatorMock = new Mock<IPlayerAuthorizationValidator>();
            userDataValidator = new Mock<IUserDataValidator>();
            authService = new Mock<IAuthService>();

            service = new PlayerService(
                playerRepoMock.Object,
                teamRepoMock.Object,
                uowMock.Object,
                membershipReqRepoMock.Object,
                validatorMock.Object,
                userRepoMock.Object,
                authorizationValidatorMock.Object,
                teamServiceMock.Object,
                userDataValidator.Object,
                authService.Object
            );
        }
        #endregion

        #region Methods Support
        private CreatePlayerDto BuildValidDto()
        {
            return new CreatePlayerDto
            {
                Name = "João Silva",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
                Address = "Rua Exemplo 1, Lisboa",
                Email = "joao.silva@example.com",
                Phone = "912345678",
                Position = Position.FORWARD,
                Height = 180
            };
        }

        private Team BuildValidTeam(Guid? teamId = null)
        {
            return new Team
            {
                Id = teamId ?? Guid.NewGuid(),
                Name = "Equipa Teste",
                Members = new List<Player>()
            };
        }

        private Player BuildValidPlayer(string id = null, Team team = null, bool isAdmin = false, DateTime? creationDate = null)
        {
            var player = new Player
            {
                Id = id ?? $"player-id-{Guid.NewGuid().ToString("N")}",
                Name = "João Silva",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
                Address = "Rua Exemplo 1, Lisboa",
                Email = "joao.silva@example.com",
                Phone = "912345678",
                Position = Position.FORWARD,
                Height = 180,
                CreationDate = creationDate ?? DateTime.UtcNow.AddMonths(-6)
            };

            if (team != null)
            {
                player.Team = team;
                player.IdTeam = team.Id;
                player.IsAdmin = isAdmin;
                team.Members.Add(player);
            }

            return player;
        }
        private UpdatePlayerDto BuildValidUpdateDto()
        {
            return new UpdatePlayerDto
            {
                Name = "João Silva",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
                Address = "Rua Exemplo 1, Lisboa",
                Email = "joao.silva@example.com",
                Phone = "912345678",
                Position = Position.FORWARD,
                Height = 180
            };
        }

        private List<InfoTeamsDto> BuildMockTeamList()
        {
            return new List<InfoTeamsDto>
            {
                new InfoTeamsDto
                {
                    Id = Guid.NewGuid(), Name = "Leões de Lisboa", Description = "Equipa da capital",
                    Address = "Rua do Leão, Lisboa", PlayerCount = 10, AverageAge = 25.5f, CurrentPoints = 1500,
                    Rank = new InfoRankDto { IdRank = Guid.NewGuid(), Name = "Ouro" }
                },
                new InfoTeamsDto
                {
                    Id = Guid.NewGuid(), Name = "Dragões do Porto", Description = "Equipa do norte",
                    Address = "Avenida do Dragão, Porto", PlayerCount = 12, AverageAge = 28.0f, CurrentPoints = 1450,
                    Rank = new InfoRankDto { IdRank = Guid.NewGuid(), Name = "Ouro" }
                },
                new InfoTeamsDto
                {
                    Id = Guid.NewGuid(), Name = "Águias de Braga", Description = "Equipa de Braga",
                    Address = "Praça da Águia, Braga", PlayerCount = 8, AverageAge = 22.1f, CurrentPoints = 1200,
                    Rank = new InfoRankDto { IdRank = Guid.NewGuid(), Name = "Prata" }
                },
                    new InfoTeamsDto
                {
                    Id = Guid.NewGuid(), Name = "Panteras de Coimbra", Description = "Equipa de estudantes",
                    Address = "Largo da Pantera, Coimbra", PlayerCount = 15, AverageAge = 30.5f, CurrentPoints = 800,
                    Rank = new InfoRankDto { IdRank = Guid.NewGuid(), Name = "Bronze" }
                },
                new InfoTeamsDto
                {
                    Id = Guid.NewGuid(), Name = "Lobos de Faro", Description = "Equipa do sul",
                    Address = "Rua do Lobo, Faro", PlayerCount = 9, AverageAge = 24.0f, CurrentPoints = 1100,
                    Rank = new InfoRankDto { IdRank = Guid.NewGuid(), Name = "Prata" }
                },
                    new InfoTeamsDto
                {
                    Id = Guid.NewGuid(), Name = "Falcões de Viseu", Description = "Equipa da beira",
                    Address = "Avenida do Falcão, Viseu", PlayerCount = 11, AverageAge = 26.2f, CurrentPoints = 750,
                    Rank = new InfoRankDto { IdRank = Guid.NewGuid(), Name = "Bronze" }
                },
                new InfoTeamsDto
                {
                    Id = Guid.NewGuid(), Name = "Tigres de Aveiro", Description = "Equipa da ria",
                    Address = "Cais do Tigre, Aveiro", PlayerCount = 10, AverageAge = 23.8f, CurrentPoints = 1150,
                    Rank = new InfoRankDto { IdRank = Guid.NewGuid(), Name = "Prata" }
                },
                new InfoTeamsDto
                {
                    Id = Guid.NewGuid(), Name = "Tubarões de Setúbal", Description = "Equipa do sado",
                    Address = "Doca do Tubarão, Setúbal", PlayerCount = 13, AverageAge = 29.1f, CurrentPoints = 600,
                    Rank = new InfoRankDto { IdRank = Guid.NewGuid(), Name = "Bronze" }
                },
                new InfoTeamsDto
                {
                    Id = Guid.NewGuid(), Name = "Gigantes do Porto", Description = "Segunda equipa do Porto",
                    Address = "Rua dos Gigantes, Porto", PlayerCount = 7, AverageAge = 21.5f, CurrentPoints = 1300,
                    Rank = new InfoRankDto { IdRank = Guid.NewGuid(), Name = "Prata" }
                },
                new InfoTeamsDto
                {
                    Id = Guid.NewGuid(), Name = "Estrelas de Lisboa", Description = "Segunda equipa de Lisboa",
                    Address = "Avenida das Estrelas, Lisboa", PlayerCount = 14, AverageAge = 27.7f, CurrentPoints = 900,
                    Rank = new InfoRankDto { IdRank = Guid.NewGuid(), Name = "Bronze" }
                },
                new InfoTeamsDto
                {
                    Id = Guid.NewGuid(), Name = "Equipa Vazia", Description = "Sem membros",
                    Address = "Rua Deserta, Nenhures", PlayerCount = 0, AverageAge = 0.0f, CurrentPoints = 0,
                    Rank = new InfoRankDto { IdRank = Guid.NewGuid(), Name = "Bronze" }
                }
            };
        }
        #endregion

        #region Tests

        #region Tests CreatePlayerAsync

        [Test(Description = "Caminho feliz: cria um jogador válido e retorna o Id gerado")]
        public async Task CreatePlayerAsync_WithValidDto_CreatesPlayerAndReturnsId()
        {
            var dto = BuildValidDto();
            var authId = "simulated-user-id-123"; 

            string createdId = null;
            playerRepoMock.Setup(r => r.AddAsync(It.IsAny<Player>()))
                .Callback<Player>(p =>
                {
                    p.Id = authId; 
                    createdId = p.Id;
                })
                .Returns(Task.CompletedTask);

            uowMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            var resultId = await service.CreatePlayerAsync(dto);

            Assert.That(resultId, Is.EqualTo(authId));

            playerRepoMock.Verify(r => r.AddAsync(It.Is<Player>(p =>
                p.Id == authId && 
                p.Email == dto.Email && 
                p.Phone == dto.Phone &&
                p.Name == dto.Name
            )), Times.Once);

            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region Tests DeltePlayerAsync

        [Test(Description = "Caminho feliz: elimina um jogador existente com sucesso")]
        public async Task DeletePlayerAsync_PlayerExists_DeletesPlayerSuccessfully()
        {
            var playerId = "player-to-delete-1";
            var existingPlayer = new Player
            {
                Id = playerId,
                Name = "Jogador Para Eliminar"
            };

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId))
                    .ReturnsAsync(existingPlayer);

            playerRepoMock.Setup(r => r.DeletePlayer(existingPlayer))
                    .Verifiable();

            uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            await service.DeletePlayerAsync(playerId);

            playerRepoMock.Verify(r => r.GetPlayerByIdAsync(playerId), Times.Once);
            playerRepoMock.Verify(r => r.DeletePlayer(existingPlayer), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "Validação: lança NotFoundException quando o jogador não é encontrado")]
        public void DeletePlayerAsync_PlayerNotFound_ThrowsException()
        {
            var playerId = "player-not-found-1";

            // ARRANGE: O repositório devolve null
            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId))
                          .ReturnsAsync((Player)null);

            // !! ARRANGE (A CORREÇÃO): Configure o MOCK do validador !!
            // Diga ao mock para lançar a exceção quando receber null.
            // (Estou a assumir que o seu mock se chama playerValidatorMock)
            validatorMock
                .Setup(v => v.DeletePlayerValidator(It.IsAny<Player>())) // ou .Setup(v => v.DeletePlayerValidator(null))
                .Throws(new NotFoundException("O Player não existe"));

            // ACT / ASSERT: 
            // Mude de DoesNotThrowAsync para ThrowsAsync
            Assert.ThrowsAsync<NotFoundException>(async () =>
                await service.DeletePlayerAsync(playerId));

            // VERIFY: Verifica comportamento
            // Estas verificações agora vão passar, porque a exceção
            // é lançada ANTES de DeletePlayer ou SaveChanges serem chamados.
            playerRepoMock.Verify(r => r.GetPlayerByIdAsync(playerId), Times.Once);
            playerRepoMock.Verify(r => r.DeletePlayer(It.IsAny<Player>()), Times.Never);
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region Tests GetPlayerByIdAsync

        [Test(Description = "Caminho feliz: encontra um jogador e retorna os seus detalhes (DTO)")]
        public async Task GetPlayerByIdAsync_PlayerExists_ReturnsPlayerDetailsDto()
        {
            var playerId = "player-to-get-1";
            var teamId = Guid.NewGuid();
            var mockPlayer = new Player
            {
                Id = playerId,
                Name = "Jogador Teste",
                DateOfBirth = new DateOnly(1995, 5, 10),
                Address = "Rua dos Testes, 123",
                Position = Position.MIDFIELDER,
                Height = 178,
                IdTeam = teamId
            };

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId))
                    .ReturnsAsync(mockPlayer);

            validatorMock.Setup(v => v.GetPlayerByIdValidator(mockPlayer)).Verifiable(); 

            var resultDto = await service.GetPlayerByIdAsync(playerId);

            Assert.That(resultDto, Is.Not.Null);
            Assert.That(resultDto, Is.TypeOf<PlayerDetailsDto>());
            Assert.That(resultDto.Name, Is.EqualTo(mockPlayer.Name));
            Assert.That(resultDto.IdTeam, Is.EqualTo(mockPlayer.IdTeam));

            playerRepoMock.Verify(r => r.GetPlayerByIdAsync(playerId), Times.Once);
            validatorMock.Verify(v => v.GetPlayerByIdValidator(mockPlayer), Times.Once);
        }

        [Test(Description = "Validação: lança Exception quando o jogador não é encontrado")]
        public void GetPlayerByIdAsync_PlayerNotFound_ThrowsException()
        {
            var playerId = "player-not-found-2";
            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId))
                          .ReturnsAsync((Player)null);

            Assert.ThrowsAsync<NullReferenceException>(async () =>
                await service.GetPlayerByIdAsync(playerId));

            playerRepoMock.Verify(r => r.GetPlayerByIdAsync(playerId), Times.Once);
        }

        #endregion

        #region Tests UpdatePlayerAsync
        [Test(Description = "Caminho feliz: atualiza dados válidos do jogador")]
        public async Task UpdatePlayerAsync_WithValidChanges_UpdatesPlayerAndSaves()
        {
            var playerId = "player-to-update-1";
            var player = BuildValidPlayer(playerId);
            var dto = BuildValidUpdateDto();

            dto.Name = "João Silva Alterado";
            dto.Address = "Morada Nova";

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync(player); 
            userRepoMock.Setup(r => r.GetUserByPhoneAsync(dto.Phone)).ReturnsAsync(player); 

            validatorMock.Setup(v => v.UpdatePlayerValidator(
                It.IsAny<UpdatePlayerDto>(),
                It.IsAny<Player>(),
                It.IsAny<User[]>()
            )).Verifiable();
            validatorMock.Setup(v => v.ValidateHasChangeDataPlayer(true)).Verifiable(); 

            uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            await service.UpdatePlayerAsync(playerId, dto);

            Assert.That(player.Name, Is.EqualTo(dto.Name));
            Assert.That(player.Address, Is.EqualTo(dto.Address));

            playerRepoMock.Verify(r => r.GetPlayerByIdAsync(playerId), Times.Once);
            userRepoMock.Verify(r => r.GetUserByEmailAsync(dto.Email), Times.Once);
            userRepoMock.Verify(r => r.GetUserByPhoneAsync(dto.Phone), Times.Once);
            validatorMock.Verify(v => v.UpdatePlayerValidator(
                It.IsAny<UpdatePlayerDto>(),
                It.IsAny<Player>(),
                It.IsAny<User[]>()),
                Times.Once);
            validatorMock.Verify(v => v.ValidateHasChangeDataPlayer(true), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "Caminho feliz: atualiza o email para um que está disponível")]
        public async Task UpdatePlayerAsync_WithValidNewEmail_UpdatesPlayerAndSaves()
        {
            var playerId = "player-to-update-2";
            var player = BuildValidPlayer(playerId); 
            var dto = BuildValidUpdateDto();
            dto.Email = "novo.email@example.com"; 

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);

            userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync((Player)null); 
            userRepoMock.Setup(r => r.GetUserByPhoneAsync(dto.Phone)).ReturnsAsync(player); 

            validatorMock.Setup(v => v.UpdatePlayerValidator(It.IsAny<UpdatePlayerDto>(), It.IsAny<Player>(), It.IsAny<User[]>())).Verifiable();
            validatorMock.Setup(v => v.ValidateHasChangeDataPlayer(true)).Verifiable(); 
            uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            await service.UpdatePlayerAsync(playerId, dto);

            Assert.That(player.Email, Is.EqualTo(dto.Email));
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "Validação: lança exceção quando o jogador a atualizar não existe")]
        public void UpdatePlayerAsync_PlayerNotFound_ThrowsValidationException()
        {
            var playerId = "player-not-found-3";
            var dto = BuildValidUpdateDto();
            string exceptionMessage = "Player doesn't exist.";

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync((Player)null);
            userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync((Player)null);
            userRepoMock.Setup(r => r.GetUserByPhoneAsync(dto.Phone)).ReturnsAsync((Player)null);

            validatorMock.Setup(v => v.UpdatePlayerValidator(
                It.IsAny<UpdatePlayerDto>(),
                null, 
                It.IsAny<User[]>()
            )).Throws(new ValidationException(exceptionMessage));

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await service.UpdatePlayerAsync(playerId, dto));

            Assert.That(ex.Message, Is.EqualTo(exceptionMessage));
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Validação: lança exceção quando o novo email já está em uso por outro jogador")]
        public void UpdatePlayerAsync_EmailAlreadyInUse_ThrowsValidationException()
        {
            var playerId = "player-to-update-3";
            var player = BuildValidPlayer(playerId);
            var otherPlayer = BuildValidPlayer("other-player");
            otherPlayer.Email = "email.usado@example.com";

            var dto = BuildValidUpdateDto();
            dto.Email = "email.usado@example.com"; 
            string exceptionMessage = $"The email '{dto.Email}' is already in use.";

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync(otherPlayer); 
            userRepoMock.Setup(r => r.GetUserByPhoneAsync(dto.Phone)).ReturnsAsync(player); 

            validatorMock.Setup(v => v.UpdatePlayerValidator(
                It.IsAny<UpdatePlayerDto>(),
                It.IsAny<Player>(),
                It.IsAny<User[]>()
            )).Throws(new ValidationException(exceptionMessage));

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await service.UpdatePlayerAsync(playerId, dto));

            Assert.That(ex.Message, Is.EqualTo(exceptionMessage));
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Validação: lança exceção quando não há nenhuma alteração nos dados")]
        public void UpdatePlayerAsync_NoChangesMade_ThrowsValidationException()
        {
            var playerId = "player-no-changes-1";
            var player = BuildValidPlayer(playerId);
            var dto = BuildValidUpdateDto();

            string exceptionMessage = "Não foi atualizado nenhuma informação do utilizador.";

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync(player);
            userRepoMock.Setup(r => r.GetUserByPhoneAsync(dto.Phone)).ReturnsAsync(player);

            validatorMock.Setup(v => v.UpdatePlayerValidator(It.IsAny<UpdatePlayerDto>(), It.IsAny<Player>(), It.IsAny<User[]>())).Verifiable();

            validatorMock.Setup(v => v.ValidateHasChangeDataPlayer(false))
                   .Throws(new ValidationException(exceptionMessage));

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await service.UpdatePlayerAsync(playerId, dto));

            Assert.That(ex.Message, Is.EqualTo(exceptionMessage));
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Validação: lança ValidationException quando a morada tem formato inválido no update")]
        public void UpdatePlayerAsync_InvalidAddressFormat_ThrowsValidationException()
        {
            var playerId = "player-invalid-address-1";
            var player = BuildValidPlayer(playerId);
            var dto = BuildValidUpdateDto();
            dto.Address = "Avenida sem cidade";
            string expectedError = "Formato de endereço inválido";

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync(player);
            userRepoMock.Setup(r => r.GetUserByPhoneAsync(dto.Phone)).ReturnsAsync(player);

            validatorMock.Setup(v => v.UpdatePlayerValidator(
                It.IsAny<UpdatePlayerDto>(),
                It.IsAny<Player>(),
                It.IsAny<User[]>()
            )).Throws(new ValidationException(expectedError));

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await service.UpdatePlayerAsync(playerId, dto));

            Assert.That(ex.Message, Does.Contain(expectedError));
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Validação: lança exceção para vários campos inválidos")]
        [TestCase("invalid-email.com", "912345678", 180, "Email format is invalid")]
        [TestCase("valid@email.com", "12345", 180, "Phone number must have 9 digits")]
        [TestCase("valid@email.com", "91234567A", 180, "must only contain digits")]
        [TestCase("valid@email.com", "012345678", 180, "cannot start with '0'")]
        [TestCase("valid@email.com", "912345678", 99, "Height value is invalid")]
        [TestCase("valid@email.com", "912345678", 251, "Height value is invalid")]
        public async Task UpdatePlayerAsync_InvalidDtoData_ThrowsValidationException(string email, string phone, int height, string expectedError)
        {
            var playerId = "player-invalid-dto-1";
            var player = BuildValidPlayer(playerId);
            var dto = BuildValidUpdateDto();

            dto.Email = email;
            dto.Phone = phone;
            dto.Height = height;

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
            userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
            userRepoMock.Setup(r => r.GetUserByPhoneAsync(dto.Phone)).ReturnsAsync((User?)null);

            validatorMock.Setup(v => v.UpdatePlayerValidator(
                It.IsAny<UpdatePlayerDto>(),
                It.IsAny<Player>(),
                It.IsAny<User[]>()
            )).Throws(new ValidationException(expectedError));

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await service.UpdatePlayerAsync(playerId, dto));

            Assert.That(ex.Message, Does.Contain(expectedError));
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #region Tests LeaveTeamAsync

        [Test(Description = "Validação: lança exceção quando o jogador não é encontrado")]
        public void LeaveTeam_PlayerNotFound_ThrowsValidationException()
        {
            var playerId = "player-not-found-4";
            string exceptionMessage = "Player doesn't exist.";

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync((Player)null);

            validatorMock.Setup(v => v.LeaveTeamValidator(null))
                    .Throws(new ValidationException(exceptionMessage));

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                                await service.LeaveTeam(playerId));

            Assert.That(ex.Message, Is.EqualTo(exceptionMessage));
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Validação: lança exceção quando o jogador não pertence a nenhuma equipa")]
        public void LeaveTeam_PlayerNotInTeam_ThrowsValidationException()
        {
            var player = BuildValidPlayer(id: "player-no-team-1", team: null);
            string exceptionMessage = "Player does not belong to any team.";

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);

            validatorMock.Setup(v => v.LeaveTeamValidator(player))
                    .Throws(new ValidationException(exceptionMessage));

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                                await service.LeaveTeam(player.Id));

            Assert.That(ex.Message, Is.EqualTo(exceptionMessage));
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Caminho feliz: jogador normal (não-admin) sai da equipa")]
        public async Task LeaveTeam_RegularMember_LeavesSuccessfully()
        {
            var team = BuildValidTeam();
            var player = BuildValidPlayer(id: "player-leaving-1", team: team, isAdmin: false);
            string expectedTeamName = team.Name;

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);
            validatorMock.Setup(v => v.LeaveTeamValidator(player)).Verifiable();
            playerRepoMock.Setup(r => r.UpdatePlayer(player)).Verifiable();
            uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var resultTeamName = await service.LeaveTeam(player.Id);

            Assert.That(resultTeamName, Is.EqualTo(expectedTeamName));
            Assert.That(player.IdTeam, Is.Null);
            Assert.That(player.Team, Is.Null);
            Assert.That(player.IsAdmin, Is.False);
            Assert.That(team.Members, Does.Not.Contain(player));

            playerRepoMock.Verify(r => r.UpdatePlayer(player), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            teamServiceMock.Verify(s => s.DeleteTeamAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test(Description = "Caminho feliz: Admin sai da equipa, mas existe outro admin")]
        public async Task LeaveTeam_AdminLeaves_AnotherAdminExists_LeavesSuccessfully()
        {
            var team = BuildValidTeam();
            var playerLeaving = BuildValidPlayer(id: "admin-leaving-1", team: team, isAdmin: true);
            var otherAdmin = BuildValidPlayer(id: "admin-staying-1", team: team, isAdmin: true);
            string expectedTeamName = team.Name;

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerLeaving.Id)).ReturnsAsync(playerLeaving);
            validatorMock.Setup(v => v.LeaveTeamValidator(playerLeaving)).Verifiable();
            playerRepoMock.Setup(r => r.UpdatePlayer(playerLeaving)).Verifiable();
            uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var resultTeamName = await service.LeaveTeam(playerLeaving.Id);

            Assert.That(resultTeamName, Is.EqualTo(expectedTeamName));
            Assert.That(playerLeaving.IdTeam, Is.Null);
            Assert.That(playerLeaving.IsAdmin, Is.False); 
            Assert.That(otherAdmin.IsAdmin, Is.True);
            Assert.That(team.Members, Does.Not.Contain(playerLeaving)); 

            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            teamServiceMock.Verify(s => s.DeleteTeamAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test(Description = "Caminho feliz: Último admin sai, promove o membro mais antigo (que não ele próprio)")]
        public async Task LeaveTeam_LastAdminLeaves_OtherMembersExist_PromotesOldestMember()
        {
            var team = BuildValidTeam();
            var oldestMember = BuildValidPlayer(id: "oldest-member-1", team: team, isAdmin: false, creationDate: DateTime.UtcNow.AddYears(-2));
            var otherMember = BuildValidPlayer(id: "other-member-1", team: team, isAdmin: false, creationDate: DateTime.UtcNow.AddYears(-1));
            var playerLeaving = BuildValidPlayer(id: "last-admin-leaving-1", team: team, isAdmin: true, creationDate: DateTime.UtcNow.AddMonths(-6));

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerLeaving.Id)).ReturnsAsync(playerLeaving);
            validatorMock.Setup(v => v.LeaveTeamValidator(playerLeaving)).Verifiable();
            playerRepoMock.Setup(r => r.UpdatePlayer(playerLeaving)).Verifiable();
            uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            await service.LeaveTeam(playerLeaving.Id);

            Assert.That(playerLeaving.IdTeam, Is.Null);
            Assert.That(playerLeaving.IsAdmin, Is.False);
            Assert.That(oldestMember.IsAdmin, Is.True); 
            Assert.That(otherMember.IsAdmin, Is.False);
            Assert.That(team.Members, Does.Not.Contain(playerLeaving)); 

            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            teamServiceMock.Verify(s => s.DeleteTeamAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test(Description = "Caminho feliz: Último admin e único membro sai, eliminando a equipa")]
        public async Task LeaveTeam_LastAdminAndOnlyMember_DeletesTeamSuccessfully()
        {
            var team = BuildValidTeam();
            var playerLeaving = BuildValidPlayer(id: "only-member-leaving-1", team: team, isAdmin: true);
            string expectedTeamName = team.Name;

            Assert.That(team.Members.Count, Is.EqualTo(1)); 

            playerRepoMock.Setup(r => r.GetPlayerByIdAsync(playerLeaving.Id)).ReturnsAsync(playerLeaving);
            validatorMock.Setup(v => v.LeaveTeamValidator(playerLeaving)).Verifiable();

            teamServiceMock.Setup(s => s.DeleteTeamAsync(team.Id, playerLeaving.Id))
                   .Returns(Task.CompletedTask)
                   .Verifiable();

            playerRepoMock.Setup(r => r.UpdatePlayer(playerLeaving)).Verifiable();
            uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1).Verifiable();

            var resultTeamName = await service.LeaveTeam(playerLeaving.Id);

            Assert.That(resultTeamName, Is.EqualTo(expectedTeamName));
            Assert.That(playerLeaving.IdTeam, Is.Null);
            Assert.That(team.Members, Is.Empty); 

            teamServiceMock.Verify(s => s.DeleteTeamAsync(team.Id, playerLeaving.Id), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region Test ListTeams
        [Test(Description = "Caminho feliz: retorna a lista completa de equipas do repositório")]
        public async Task GetListTeams_WhenTeamsExist_ReturnsListOfTeams()
        {
            var mockList = BuildMockTeamList();
            teamRepoMock.Setup(r => r.GetListTeamsPlayer()).ReturnsAsync(mockList);

            var result = await service.GetListTeams();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(mockList.Count));
            Assert.That(result[0].Name, Is.EqualTo("Leões de Lisboa"));
            Assert.That(result[2].Rank.Name, Is.EqualTo("Prata"));
            Assert.That(result, Is.EqualTo(mockList));
            teamRepoMock.Verify(r => r.GetListTeamsPlayer(), Times.Once);
        }

        [Test(Description = "Caminho feliz: retorna uma lista vazia se o repositório não encontrar equipas")]
        public async Task GetListTeams_WhenNoTeamsExist_ReturnsEmptyList()
        {
            var emptyList = new List<InfoTeamsDto>();
            teamRepoMock.Setup(r => r.GetListTeamsPlayer()).ReturnsAsync(emptyList);

            var result = await service.GetListTeams();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
            teamRepoMock.Verify(r => r.GetListTeamsPlayer(), Times.Once);
        }

        #endregion

        #region Tests GetTeamListWithFilters

        [Test(Description = "Caminho feliz: retorna uma lista filtrada quando os filtros são válidos")]
        public async Task GetTeamListWithFilters_WithValidFilters_ReturnsFilteredList()
        {
            var filter = new FilterListTeamDto
            {
                NameRank = "Ouro"
            };

            var fullList = BuildMockTeamList();
            var expectedFilteredList = fullList
                .Where(t => t.Rank.Name == "Ouro")
                .ToList();

            Assert.That(expectedFilteredList.Count, Is.EqualTo(2));

            validatorMock.Setup(v => v.ValidateFiltersListTeams(filter)).Verifiable();

            teamRepoMock.Setup(r => r.GetListTeamsPlayersWithFilters(filter))
            .ReturnsAsync(expectedFilteredList);

            var result = await service.GetTeamListWithFilters(filter);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result, Is.EqualTo(expectedFilteredList));

            validatorMock.Verify(v => v.ValidateFiltersListTeams(filter), Times.Once);
            teamRepoMock.Verify(r => r.GetListTeamsPlayersWithFilters(filter), Times.Once);
        }

        [Test(Description = "Caminho feliz: retorna a lista completa quando os filtros estão vazios")]
        public async Task GetTeamListWithFilters_WithEmptyFilter_ReturnsFullList()
        {
            var filter = new FilterListTeamDto();
            var fullList = BuildMockTeamList();

            validatorMock.Setup(v => v.ValidateFiltersListTeams(filter)).Verifiable();

            teamRepoMock.Setup(r => r.GetListTeamsPlayersWithFilters(filter))
            .ReturnsAsync(fullList);

            var result = await service.GetTeamListWithFilters(filter);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(fullList.Count));
            validatorMock.Verify(v => v.ValidateFiltersListTeams(filter), Times.Once);
            teamRepoMock.Verify(r => r.GetListTeamsPlayersWithFilters(filter), Times.Once);
        }

        [Test(Description = "Caminho feliz: retorna uma lista filtrada quando um filtro complexo (vários campos) é usado")]
        public async Task GetTeamListWithFilters_WithComplexValidFilters_ReturnsFilteredList()
        {
            var complexFilter = new FilterListTeamDto
            {
                City = "Lisboa",
                MinAge = 25,
                MaxAge = 30,
                MinNumberPoints = 1000
            };

            var fullList = BuildMockTeamList();

            var expectedResult = fullList.Where(t =>
            t.Address.Contains("Lisboa") &&
            t.AverageAge >= 25 &&
            t.AverageAge <= 30 &&
            t.CurrentPoints >= 1000
                ).ToList();

            Assert.That(expectedResult.Count, Is.EqualTo(1));
            Assert.That(expectedResult[0].Name, Is.EqualTo("Leões de Lisboa"));

            validatorMock.Setup(v => v.ValidateFiltersListTeams(complexFilter)).Verifiable();

            teamRepoMock.Setup(r => r.GetListTeamsPlayersWithFilters(complexFilter))
            .ReturnsAsync(expectedResult);

            var result = await service.GetTeamListWithFilters(complexFilter);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result, Is.EqualTo(expectedResult));

            validatorMock.Verify(v => v.ValidateFiltersListTeams(complexFilter), Times.Once);
            teamRepoMock.Verify(r => r.GetListTeamsPlayersWithFilters(complexFilter), Times.Once);
        }

        [Test(Description = "Validação: lança InvalidOperationException para intervalos de filtro inválidos")]
        [TestCase("Points", "O numero minimo de pontos de uma equipa, não deve ser superior ao numero maximo")]
        [TestCase("Age", "O numero minimo de idade minima tem de ser inferior à idade media maxima")]
        [TestCase("Players", "O número minimo de membros deve ser superior ao numero maximo de membros")]
        public void GetTeamListWithFilters_WithInvalidFilterRanges_ThrowsInvalidOperationException(
             string filterType, string expectedError)
        {
            var filter = new FilterListTeamDto();

            switch (filterType)
            {
                case "Points":
                    filter.MinNumberPoints = 1000;
                    filter.MaxNumberPoints = 500;
                    break;
                case "Age":
                    filter.MinAge = 30;
                    filter.MaxAge = 25;
                    break;
                case "Players":
                    filter.MinNumberPlayers = 10;
                    filter.MaxNumberPlayers = 5;
                    break;
            }

            validatorMock.Setup(v => v.ValidateFiltersListTeams(filter))
                   .Throws(new InvalidOperationException(expectedError));

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.GetTeamListWithFilters(filter));

            Assert.That(ex.Message, Is.EqualTo(expectedError));

            teamRepoMock.Verify(r => r.GetListTeamsPlayersWithFilters(It.IsAny<FilterListTeamDto>()), Times.Never);
        }

        #endregion

        #endregion
    }
}