using Application.DTOs.Rank;
using Application.DTOs.SuperAdmin;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Validators;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Exceptions;

namespace Tests.Unit.ApplicationTests.ServicesTests
{
    [TestFixture]
    internal class SuperAdminServiceTests
    {
        #region Variables
        private Mock<ISuperAdminRepository> sAdminRepoMock;
        private Mock<IUserRepository> userRepoMock;
        private Mock<IUnityOfWork> uowMock;
        private Mock<ISuperAdminValidator> sAdminValidatorMock;

        private SuperAdminService service;
        #endregion

        #region SetUp
        [SetUp]
        public void SetUp()
        {
            sAdminRepoMock = new Mock<ISuperAdminRepository>();
            userRepoMock = new Mock<IUserRepository>();
            uowMock = new Mock<IUnityOfWork>();
            sAdminValidatorMock = new Mock<ISuperAdminValidator>();

            service = new SuperAdminService(
                sAdminRepoMock.Object,
                userRepoMock.Object,
                uowMock.Object,
                sAdminValidatorMock.Object
                );
        }
        #endregion

        #region Methods Support
        private CreateSuperAdminDTO BuildValidDto()
        {
            return new CreateSuperAdminDTO
            {
                Name = "João Silva",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
                Address = "Rua Exemplo 1, Lisboa",
                Email = "joao.silva@example.com",
                Phone = "912345678",
            };
        }

        private SuperAdmin BuildValidSuperAdmin(string id = null, DateTime? creationDate = null)
        {
            var superAdmin = new SuperAdmin
            {
                Id = id ?? $"superAdmin-id-{Guid.NewGuid().ToString("N")}",
                Name = "João Silva",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
                Address = "Rua Exemplo 1, Lisboa",
                Email = "joao.silva@example.com",
                Phone = "912345678",
                CreationDate = creationDate ?? DateTime.UtcNow.AddMonths(-6)
            };

            return superAdmin;
        }
        private UpdateSuperAdminDTO BuildValidUpdateDto()
        {
            return new UpdateSuperAdminDTO
            {
                Name = "João Silva",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
                Address = "Rua Exemplo 1, Lisboa",
                Email = "joao.silva@example.com",
                Phone = "912345678",
            };
        }
        #endregion

        #region Tests

        #region Tests CreateSuperAdminAsync

        [Test]
        public async Task CreateSuperAdminAsync_WithValidDto_CreatesSuperAdminAndReturnsId()
        {
            var dto = BuildValidDto();
            var authId = "simulated-user-id-123";

            string createdId = null;
            sAdminRepoMock.Setup(r => r.AddAsync(It.IsAny<SuperAdmin>()))
                .Callback<SuperAdmin>(sa =>
                {
                    sa.Id = authId;
                    createdId = sa.Id;
                })
                .Returns(Task.CompletedTask);

            uowMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            var resultId = await service.CreateSuperAdminAsync(dto);

            Assert.That(resultId, Is.EqualTo(authId));

            sAdminRepoMock.Verify(r => r.AddAsync(It.Is<SuperAdmin>(sa =>
                sa.Id == authId &&
                sa.Email == dto.Email &&
                sa.Phone == dto.Phone &&
                sa.Name == dto.Name
            )), Times.Once);

            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region Tests DeleteSuperAdminAsync

        [Test]
        public async Task DeleteSuperAdminAsync_SuperAdminExists_DeletesSuperAdminSuccessfully()
        {
            var sAdminId = "superAdmin-to-delete-1";
            var existingSAdmin = new SuperAdmin
            {
                Id = sAdminId,
                Name = "Jogador Para Eliminar"
            };

            sAdminRepoMock.Setup(r => r.GetSuperAdminByIdAsync(sAdminId))
                    .ReturnsAsync(existingSAdmin);

            sAdminRepoMock.Setup(r => r.DeleteSuperAdmin(existingSAdmin))
                    .Verifiable();

            uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            await service.DeleteSuperAdminAsync(sAdminId);

            sAdminRepoMock.Verify(r => r.GetSuperAdminByIdAsync(sAdminId), Times.Once);
            sAdminRepoMock.Verify(r => r.DeleteSuperAdmin(existingSAdmin), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void DeleteSuperAdminAsync_SuperAdminNotFound_ThrowsException()
        {
            var sAdminId = "superAdmin-not-found-1";

            sAdminRepoMock.Setup(r => r.GetSuperAdminByIdAsync(sAdminId))
                .ReturnsAsync((SuperAdmin)null);

            sAdminValidatorMock.Setup(v => v.DeleteSuperAdminValidator(null))
                .Throws(new ArgumentException($"Super Admin with ID {sAdminId} not found."));

            Assert.ThrowsAsync<ArgumentException>(() => service.DeleteSuperAdminAsync(sAdminId));

            sAdminRepoMock.Verify(r => r.GetSuperAdminByIdAsync(sAdminId), Times.Once);
            sAdminRepoMock.Verify(r => r.DeleteSuperAdmin(It.IsAny<SuperAdmin>()), Times.Never);
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region Tests GetSuperAdminByIdAsync

        [Test(Description = "Caminho feliz: encontra um jogador e retorna os seus detalhes (DTO)")]
        public async Task GetSuperAdminByIdAsync_SuperAdminExists_ReturnsSuperAdminDetailsDto()
        {
            var sAdminId = "superAdmin-to-get-1";
            var teamId = Guid.NewGuid();
            var mockSAdmin = new SuperAdmin
            {
                Id = sAdminId,
                Name = "Teste",
                DateOfBirth = new DateOnly(1995, 5, 10),
                Address = "Rua dos Testes, 123",
            };

            sAdminRepoMock.Setup(r => r.GetSuperAdminByIdAsync(sAdminId))
                    .ReturnsAsync(mockSAdmin);

            sAdminValidatorMock.Setup(v => v.GetSuperAdminByIdValidator(mockSAdmin)).Verifiable();

            var resultDto = await service.GetSuperAdminByIdAsync(sAdminId);

            Assert.That(resultDto, Is.Not.Null);
            Assert.That(resultDto, Is.TypeOf<SuperAdminDetailsDTO>());
            Assert.That(resultDto.Name, Is.EqualTo(mockSAdmin.Name));

            sAdminRepoMock.Verify(r => r.GetSuperAdminByIdAsync(sAdminId), Times.Once);
            sAdminValidatorMock.Verify(v => v.GetSuperAdminByIdValidator(mockSAdmin), Times.Once);
        }

        [Test]
        public void GetSuperAdminByIdAsync_SuperAdminNotFound_ThrowsException()
        {
            var sAdminId = "superAdmin-not-found-2";

            sAdminRepoMock.Setup(r => r.GetSuperAdminByIdAsync(sAdminId))
                .ReturnsAsync((SuperAdmin)null);

            sAdminValidatorMock.Setup(v => v.GetSuperAdminByIdValidator(null))
                .Throws(new ArgumentException($"SuperAdmin with ID {sAdminId} not found."));

            Assert.ThrowsAsync<ArgumentException>(() => service.GetSuperAdminByIdAsync(sAdminId));

            sAdminRepoMock.Verify(r => r.GetSuperAdminByIdAsync(sAdminId), Times.Once);
            sAdminValidatorMock.Verify(v => v.GetSuperAdminByIdValidator(null), Times.Once);
        }


        #endregion

        #region Tests UpdateSuperAdminAsync
        [Test(Description = "Caminho feliz: atualiza dados válidos do jogador")]
        public async Task UpdateSuperAdminAsync_WithValidChanges_UpdatesSuperAdminAndSaves()
        {
            var sAdminId = "superAdmin-to-update-1";
            var superAdmin = BuildValidSuperAdmin(sAdminId);
            var dto = BuildValidUpdateDto();

            dto.Name = "João Silva Alterado";
            dto.Address = "Morada Nova";

            sAdminRepoMock.Setup(r => r.GetSuperAdminByIdAsync(sAdminId)).ReturnsAsync(superAdmin);
            userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync(superAdmin);
            userRepoMock.Setup(r => r.GetUserByPhoneAsync(dto.Phone)).ReturnsAsync(superAdmin);

            sAdminValidatorMock.Setup(v => v.UpdateSuperAdminValidator(
                It.IsAny<UpdateSuperAdminDTO>(),
                It.IsAny<SuperAdmin>(),
                It.IsAny<User[]>()
            )).Verifiable();

            uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            await service.UpdateSuperAdminAsync(sAdminId, dto);

            Assert.That(superAdmin.Name, Is.EqualTo(dto.Name));
            Assert.That(superAdmin.Address, Is.EqualTo(dto.Address));

            sAdminRepoMock.Verify(r => r.GetSuperAdminByIdAsync(sAdminId), Times.Once);
            userRepoMock.Verify(r => r.GetUserByEmailAsync(dto.Email), Times.Once);
            userRepoMock.Verify(r => r.GetUserByPhoneAsync(dto.Phone), Times.Once);
            sAdminValidatorMock.Verify(v => v.UpdateSuperAdminValidator(
                It.IsAny<UpdateSuperAdminDTO>(),
                It.IsAny<SuperAdmin>(),
                It.IsAny<User[]>()),
                Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "Caminho feliz: atualiza o email para um que está disponível")]
        public async Task UpdateSuperAdminAsync_WithValidNewEmail_UpdatesSuperAdminAndSaves()
        {
            var sAdminId = "superAdmin-to-update-2";
            var superAdmin = BuildValidSuperAdmin(sAdminId);
            var dto = BuildValidUpdateDto();
            dto.Email = "novo.email@example.com";

            sAdminRepoMock.Setup(r => r.GetSuperAdminByIdAsync(sAdminId)).ReturnsAsync(superAdmin);

            userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync((SuperAdmin)null);
            userRepoMock.Setup(r => r.GetUserByPhoneAsync(dto.Phone)).ReturnsAsync(superAdmin);

            sAdminValidatorMock.Setup(v => v.UpdateSuperAdminValidator(It.IsAny<UpdateSuperAdminDTO>(), It.IsAny<SuperAdmin>(), It.IsAny<User[]>())).Verifiable();
            uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            await service.UpdateSuperAdminAsync(sAdminId, dto);

            Assert.That(superAdmin.Email, Is.EqualTo(dto.Email));
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test(Description = "Validação: lança exceção quando o jogador a atualizar não existe")]
        public void UpdateSuperAdminAsync_SuperAdminNotFound_ThrowsValidationException()
        {
            var sAdminId = "superAdmin-not-found-3";
            var dto = BuildValidUpdateDto();
            string exceptionMessage = "SuperAdmin doesn't exist.";

            sAdminRepoMock.Setup(r => r.GetSuperAdminByIdAsync(sAdminId)).ReturnsAsync((SuperAdmin)null);
            userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync((SuperAdmin)null);
            userRepoMock.Setup(r => r.GetUserByPhoneAsync(dto.Phone)).ReturnsAsync((SuperAdmin)null);

            sAdminValidatorMock.Setup(v => v.UpdateSuperAdminValidator(
                It.IsAny<UpdateSuperAdminDTO>(),
                null,
                It.IsAny<User[]>()
            )).Throws(new ValidationException(exceptionMessage));

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await service.UpdateSuperAdminAsync(sAdminId, dto));

            Assert.That(ex.Message, Is.EqualTo(exceptionMessage));
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Validação: lança exceção quando o novo email já está em uso por outro jogador")]
        public void UpdateSuperAdminAsync_EmailAlreadyInUse_ThrowsValidationException()
        {
            var sAdminId = "superAdmin-to-update-3";
            var superAdmin = BuildValidSuperAdmin(sAdminId);
            var otherSuperAdmin = BuildValidSuperAdmin("other-superAdmin");
            otherSuperAdmin.Email = "email.usado@example.com";

            var dto = BuildValidUpdateDto();
            dto.Email = "email.usado@example.com";
            string exceptionMessage = $"The email '{dto.Email}' is already in use.";

            sAdminRepoMock.Setup(r => r.GetSuperAdminByIdAsync(sAdminId)).ReturnsAsync(superAdmin);
            userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync(otherSuperAdmin);
            userRepoMock.Setup(r => r.GetUserByPhoneAsync(dto.Phone)).ReturnsAsync(superAdmin);

            sAdminValidatorMock.Setup(v => v.UpdateSuperAdminValidator(
                It.IsAny<UpdateSuperAdminDTO>(),
                It.IsAny<SuperAdmin>(),
                It.IsAny<User[]>()
            )).Throws(new ValidationException(exceptionMessage));

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await service.UpdateSuperAdminAsync(sAdminId, dto));

            Assert.That(ex.Message, Is.EqualTo(exceptionMessage));
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public void UpdateSuperAdminAsync_NoChangesMade_ThrowsValidationException()
        {
            var sAdminId = "superAdmin-no-changes-1";
            var superAdmin = BuildValidSuperAdmin(sAdminId);
            var dto = BuildValidUpdateDto();

            sAdminRepoMock.Setup(r => r.GetSuperAdminByIdAsync(sAdminId)).ReturnsAsync(superAdmin);
            userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync(superAdmin);
            userRepoMock.Setup(r => r.GetUserByPhoneAsync(dto.Phone)).ReturnsAsync(superAdmin);

            sAdminValidatorMock.Setup(v => v.UpdateSuperAdminValidator(dto, superAdmin, It.IsAny<User[]>()))
                .Throws(new ValidationException("Não foi atualizado nenhuma informação do utilizador."));

            Assert.ThrowsAsync<ValidationException>(() => service.UpdateSuperAdminAsync(sAdminId, dto));

            uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test(Description = "Validação: lança ValidationException quando a morada tem formato inválido no update")]
        public void UpdateSuperAdminAsync_InvalidAddressFormat_ThrowsValidationException()
        {
            var sAdminId = "superAdmin-invalid-address-1";
            var superAdmin = BuildValidSuperAdmin(sAdminId);
            var dto = BuildValidUpdateDto();
            dto.Address = "Avenida sem cidade";
            string expectedError = "Formato de endereço inválido";

            sAdminRepoMock.Setup(r => r.GetSuperAdminByIdAsync(sAdminId)).ReturnsAsync(superAdmin);
            userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync(superAdmin);
            userRepoMock.Setup(r => r.GetUserByPhoneAsync(dto.Phone)).ReturnsAsync(superAdmin);

            sAdminValidatorMock.Setup(v => v.UpdateSuperAdminValidator(
                It.IsAny<UpdateSuperAdminDTO>(),
                It.IsAny<SuperAdmin>(),
                It.IsAny<User[]>()
            )).Throws(new ValidationException(expectedError));

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await service.UpdateSuperAdminAsync(sAdminId, dto));

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
        public async Task UpdateSuperAdminAsync_InvalidDtoData_ThrowsValidationException(string email, string phone, int height, string expectedError)
        {
            var sAdminId = "superAdmin-invalid-dto-1";
            var superAdmin = BuildValidSuperAdmin(sAdminId);
            var dto = BuildValidUpdateDto();

            dto.Email = email;
            dto.Phone = phone;

            sAdminRepoMock.Setup(r => r.GetSuperAdminByIdAsync(sAdminId)).ReturnsAsync(superAdmin);
            userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
            userRepoMock.Setup(r => r.GetUserByPhoneAsync(dto.Phone)).ReturnsAsync((User?)null);

            sAdminValidatorMock.Setup(v => v.UpdateSuperAdminValidator(
                It.IsAny<UpdateSuperAdminDTO>(),
                It.IsAny<SuperAdmin>(),
                It.IsAny<User[]>()
            )).Throws(new ValidationException(expectedError));

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await service.UpdateSuperAdminAsync(sAdminId, dto));

            Assert.That(ex.Message, Does.Contain(expectedError));
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        #endregion

        #endregion
    }
}
