using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Moq;
using NUnit.Framework;

namespace Tests.Unit.ApplicationTests.ServicesTests.PlayerServiceTests
{
    [TestFixture]
    public class CreatePlayerTest
    {
        private Mock<IPlayerRepository> playerRepoMock;
        private Mock<ITeamRepository> teamRepoMock;
        private Mock<IUnityOfWork> unitOfWorkMock;
        private PlayerService service;

        [SetUp]
        public void Setup()
        {
            playerRepoMock = new Mock<IPlayerRepository>();
            teamRepoMock = new Mock<ITeamRepository>();
            unitOfWorkMock = new Mock<IUnityOfWork>();

            service = new PlayerService(
                playerRepoMock.Object,
                teamRepoMock.Object,
                unitOfWorkMock.Object
            );
        }

        private CreatePlayerDTO ValidDto()
        {
            return new CreatePlayerDTO
            {
                Name = "Joao Silva",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
                Address = "Rua A, Guimarães",
                Email = "joao.silva@example.com",
                Password = "StrongP@ss1",
                Phone = "912345678",
                Position = Position.MIDFIELDER,
                Height = 180
            };
        }

        [Test(Description = "Criar uma utilizador com dados válidos")]
        public async Task CreatePlayerAsync_ValidInput_AddsPlayerAndSaves()
        {
            var dto = ValidDto();

            playerRepoMock
                .Setup(r => r.GetPlayerByEmailAsync(dto.Email))
                .ReturnsAsync((Player?)null);

            //Ainda não existe
            /*
            playerRepoMock
                .Setup(r => r.GetPlayerByPhoneAsync(dto.Phone))
                .ReturnsAsync((Player?)null);
            */
            
            playerRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Player>()))
                .Returns(Task.CompletedTask);

            unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            var result = await service.CreatePlayerAsync(dto);

            Assert.That(result, Is.Not.EqualTo(Guid.Empty));
            playerRepoMock.Verify(r => r.AddAsync(It.IsAny<Player>()), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "Tentar criar um utilizador com algum campo obrigatório não introduzido")]
        [TestCase(null, "joaosilva@gmail.com", "Password1!", "912345678", "Rua Principal", Position.FORWARD, 180, TestName = "Nome nulo")]
        [TestCase("Joao", null, "Password1!", "912345678", "Rua Principal", Position.FORWARD, 180, TestName = "Email nulo")]
        [TestCase("Joao", "joaosilva@gmail.com", null, "912345678", "Rua Principal", Position.FORWARD, 180, TestName = "Password nula")]
        [TestCase("Joao", "joaosilva@gmail.com", "Password1!", null, "Rua Principal", Position.FORWARD, 180, TestName = "Telefone nulo")]
        [TestCase("Joao", "joaosilva@gmail.com", "Password1!", "912345678", null, Position.FORWARD, 180, TestName = "Endereço nulo")]
        [TestCase("Joao", "joaosilva@gmail.com", "Password1!", "912345678", "Rua Principal", null, 180, TestName = "Posição nula")]
        public void CreatePlayerAsync_MissingRequiredFields_ThrowsArgumentException(
        string name,
        string email,
        string password,
        string phone,
        string address,
        Position position,
        int height)
        {
            var dto = new CreatePlayerDTO
            {
                Name = name,
                Email = email,
                Password = password,
                Phone = phone,
                Address = address,
                Position = position,
                Height = height,
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25))
            };

            Assert.ThrowsAsync<ArgumentException>(async () => await service.CreatePlayerAsync(dto));
        }

        [Test(Description = "Teste para tentar criar um utilizador com um email duplicado")]
        public void CreatePlayerAsync_DuplicateEmail_ThrowsArgumentException()
        {
            var dto = ValidDto();

            playerRepoMock
                .Setup(r => r.GetPlayerByEmailAsync(dto.Email))
                .ReturnsAsync(new Player { Id = Guid.NewGuid(), Email = dto.Email });

            //Ainda não existe dados duplicados
            /*
             playerRepoMock
                .Setup(r => r.GetPlayerByPhoneAsync(dto.Phone))
                .ReturnsAsync((Player?)null);
             */

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.CreatePlayerAsync(dto));

            playerRepoMock.Verify(r => r.AddAsync(It.IsAny<Player>()), Times.Never);
        }

        [Test(Description = "Tentar criar um utilizador com um numero que já exsite")]
        public void CreatePlayerAsync_DuplicatePhone_ThrowsArgumentException()
        {
            var dto = ValidDto();

            playerRepoMock
                .Setup(r => r.GetPlayerByEmailAsync(dto.Email))
                .ReturnsAsync((Player?)null);

            //Ainda não existe esse metodo
            /*
            playerRepoMock
                .Setup(r => r.GetPlayerByPhoneAsync(dto.Phone))
                .ReturnsAsync(new Player { Id = Guid.NewGuid(), Phone = dto.Phone });
            */
            
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.CreatePlayerAsync(dto));

            playerRepoMock.Verify(r => r.AddAsync(It.IsAny<Player>()), Times.Never);
        }

        [Test(Description = "Teste para tentar criar um user com uma altura inválida")]
        [TestCase(ModelConstants.PlayerConst.MinHeight - 10, Description = "Altura mímina do player inválida")]
        [TestCase(ModelConstants.PlayerConst.MaxHeight + 10, Description = "Altura máxima do player inválida")]
        public void CreatePlayerAsync_InvalidHeight_ThrowsArgumentException(int height)
        {
            var dto = ValidDto();
            dto.Height = height;

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.CreatePlayerAsync(dto));
        }

        [Test(Description = "Criar um utilizador com um telefone inválido")]
        [TestCase("099999999", Description = "Número de telefone não pode começar com um 0")]
        [TestCase("1000000000", Description = "Número com 10 caracteres inválido (número de caracteres tem de ser exatamente 9)")]
        [TestCase("abcdefghij", Description = "Número de telefone não contêm letras, inválido")]
        public void CreatePlayerAsync_InvalidPhone_ThrowsArgumentException(string phone)
        {
            var dto = ValidDto();
            dto.Phone = phone;

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.CreatePlayerAsync(dto));
        }

        [Test(Description = "Teste criar um utilizador com email inválido")]
        [TestCase("JOAO@GMAIL.COM", Description = "Email invalido com letras maiusculas")]
        [TestCase("joao@@gmail.com", Description = "Email invalido com mais do que um @")]
        [TestCase("joao.gmail.com", Description = "Email invalido sem @")]
        [TestCase("joao@", Description = "Email invalido sem dominio")]
        public void CreatePlayerAsync_InvalidEmail_ThrowsArgumentException(string email)
        {
            var dto = ValidDto();
            dto.Email = email;

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.CreatePlayerAsync(dto));
        }

        [Test(Description = "Criar um utilizador com uma password inválida")]
        [TestCase("pass", Description = "Password muito curta (menor que 8 caracteres)")]
        [TestCase("passPasspass13.78654", Description = "Password muito longa (maior que 16 caracteres)")]
        [TestCase("passwordsemnumero", Description = "A password não possui pelo menos 1 numero")]
        [TestCase("PasswordSemEspecial", Description = "A password não contêm caracteres esepeciais")]
        [TestCase("password1!", Description = "A password não possui nenhuma letra maiuscula")]
        public void CreatePlayerAsync_InvalidPassword_ThrowsArgumentException(string password)
        {
            var dto = ValidDto();
            dto.Password = password;

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.CreatePlayerAsync(dto));
        }

        [Test(Description = "Tentar criar um utilizador menor de idade ou com mais de 70 anos")]
        [TestCase(-17)] 
        [TestCase(-80)] 
        public void CreatePlayerAsync_InvalidAge_ThrowsArgumentException(int years)
        {
            var dto = ValidDto();
            dto.DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(years));

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.CreatePlayerAsync(dto));
        }
    }
}
