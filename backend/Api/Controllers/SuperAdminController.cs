using Application.DTOs.SuperAdmin;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de Super Administradores da plataforma.
    /// Permite criar, consultar, atualizar e eliminar contas de Super Admin.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SuperAdminController : ControllerBase
    {
        #region Initialization
        private readonly ISuperAdminService superAdminService;
        private readonly IPlayerAuthorizationValidator playerAuthorizationValidator;
        private readonly IAuthService authService;

        /// <summary>
        /// Construtor do SuperAdminController.
        /// </summary>
        /// <param name="superAdminService">Serviço com a lógica de negócio dos Super Admins.</param>
        /// <param name="playerAuthorizationValidator">Validador de autorização (para garantir que o utilizador só edita a sua própria conta).</param>
        /// <param name="authService">Serviço de autenticação.</param>
        public SuperAdminController(ISuperAdminService superAdminService, IPlayerAuthorizationValidator playerAuthorizationValidator, IAuthService authService)
        {
            this.superAdminService = superAdminService;
            this.playerAuthorizationValidator = playerAuthorizationValidator;
            this.authService = authService;

        }

        #endregion

        #region EndPoints

        #region CRUD Super Admin
        /// <summary>
        /// Cria um novo Super Administrador.
        /// </summary>
        /// <remarks>
        /// Este endpoint é público. Cria o registo na base de dados e a conta de autenticação (Firebase), retornando os dados de login.
        /// </remarks>
        /// <param name="createSuperAdminDTO">Dados para criação do Super Admin (Nome, Email, Password, etc.).</param>
        /// <returns>Dados de login (Token) e ID do novo Super Admin.</returns>
        /// <response code="201">Super Admin criado com sucesso.</response>
        /// <response code="400">Dados inválidos (ex: email duplicado, idade inválida).</response>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)] // Substituir object pelo DTO de login
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSuperAdmin([FromBody] CreateSuperAdminDTO createSuperAdminDTO)
        {
            var newSadminId = await superAdminService.CreateSuperAdminAsync(createSuperAdminDTO);
            var createUserResult = await authService.LoginAsync(createSuperAdminDTO.Email, createSuperAdminDTO.Password);
            return CreatedAtAction(
                nameof(GetSuperAdmin),
                new { sadminId = newSadminId },
                createUserResult
                );
        }

        /// <summary>
        /// Elimina a conta de um Super Administrador.
        /// </summary>
        /// <remarks>
        /// O utilizador autenticado só pode eliminar a sua própria conta.
        /// </remarks>
        /// <param name="sadminId">O ID do Super Admin a eliminar.</param>
        /// <response code="204">Conta eliminada com sucesso.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        /// <response code="403">Utilizador tentou eliminar uma conta que não lhe pertence.</response>
        /// <response code="404">Super Admin não encontrado.</response>
        [HttpDelete("{sadminId}")] // Removi a constraint :required pois pode dar problemas com Swagger se não estiver configurada
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSuperAdmin(string sadminId)
        {
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), sadminId);
            await superAdminService.DeleteSuperAdminAsync(sadminId);

            return NoContent();
        }

        /// <summary>
        /// Obtém os detalhes de um Super Administrador pelo ID.
        /// </summary>
        /// <param name="sadminId">O ID do Super Admin.</param>
        /// <returns>Detalhes do Super Admin (Nome, Email, etc.).</returns>
        /// <response code="200">Detalhes retornados com sucesso.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        /// <response code="404">Super Admin não encontrado.</response>
        [HttpGet("{sadminId}")]
        [ProducesResponseType(typeof(SuperAdminDetailsDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSuperAdmin(string sadminId)
        {
            var superAdminDetails = await superAdminService.GetSuperAdminByIdAsync(sadminId);

            return Ok(superAdminDetails);
        }

        /// <summary>
        /// Atualiza os dados de um Super Administrador.
        /// </summary>
        /// <remarks>
        /// O utilizador autenticado só pode atualizar a sua própria conta.
        /// </remarks>
        /// <param name="sadminId">O ID do Super Admin a atualizar.</param>
        /// <param name="dto">Novos dados do Super Admin.</param>
        /// <response code="200">Atualização efetuada com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        /// <response code="403">Utilizador tentou atualizar uma conta que não lhe pertence.</response>
        /// <response code="404">Super Admin não encontrado.</response>
        [HttpPut("{sadminId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSuperAdmin(string sadminId, [FromBody] UpdateSuperAdminDTO dto)
        {
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), sadminId);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await superAdminService.UpdateSuperAdminAsync(sadminId, dto);

            return Ok("Super Admin information updated succesfully.");
        }
        #endregion

        #endregion

        #region Private Methods
        private string GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                throw new UnauthorizedAccessException("User ID not found in claims.");
            }

            return userId;
        }
        #endregion
    }
}
