using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de autenticação e perfil do utilizador.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UserController : Controller
    {
        #region Inicializar 
        private readonly IAuthService authService;

        public UserController(IAuthService authService)
        {
            this.authService = authService;

        }
        #endregion

        /// <summary>
        /// Realiza o login de um utilizador na aplicação.
        /// </summary>
        /// <remarks>
        /// Envia as credenciais (email e password) para obter um token de autenticação JWT e os dados do perfil do utilizador.
        /// </remarks>
        /// <param name="loginData">Objeto contendo o email e a password do utilizador.</param>
        /// <returns>Um objeto <see cref="LoginResponseDto"/> com o token e os dados do utilizador.</returns>
        /// <response code="200">Login efetuado com sucesso.</response>
        /// <response code="400">Credenciais inválidas ou erro na autenticação.</response>
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginData)
        {
            return Ok(await authService.LoginAsync(loginData.Email, loginData.Password));
        }

        /// <summary>
        /// Termina a sessão do utilizador (Logout).
        /// </summary>
        /// <remarks>
        /// Invalida os tokens de refresh associados ao utilizador atual. Requer autenticação.
        /// </remarks>
        /// <response code="200">Sessão terminada com sucesso.</response>
        /// <response code="401">Utilizador não está autenticado.</response>
        [HttpGet]
        [Route("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            await authService.LogoutAsync(userId);
            return Ok();
        }

        /*
        [HttpDelete]
        [Route("delete")]
        [Authorize]
        public IActionResult DeleteUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            authService.DeleteUserAsync(userId);
            return NoContent();
        }
        */

        /// <summary>
        /// Altera a palavra-passe do utilizador autenticado.
        /// </summary>
        /// <remarks>
        /// Permite ao utilizador alterar a sua password fornecendo a atual e a nova.
        /// </remarks>
        /// <param name="currentPassword">A palavra-passe atual do utilizador.</param>
        /// <param name="newPassword">A nova palavra-passe que o utilizador deseja definir.</param>
        /// <response code="204">Palavra-passe alterada com sucesso.</response>
        /// <response code="400">A password atual está incorreta ou ocorreu um erro na validação.</response>
        /// <response code="401">Utilizador não está autenticado.</response>
        [HttpGet]
        [Route("ChangePassword")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromQuery] string currentPassword, [FromQuery] string newPassword)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await authService.ChangePasswordAsync(userId, currentPassword, newPassword);
            return NoContent();
        }

        /// <summary>
        /// Obtém o perfil completo do utilizador autenticado.
        /// </summary>
        /// <remarks>
        /// Retorna todos os detalhes do utilizador (Player ou SuperAdmin) com base no token JWT fornecido.
        /// </remarks>
        /// <returns>Um objeto <see cref="LoginResponseDto"/> com os detalhes do perfil.</returns>
        /// <response code="200">Perfil obtido com sucesso.</response>
        /// <response code="401">Utilizador não está autenticado ou ID inválido.</response>
        /// <response code="404">Utilizador não encontrado na base de dados.</response>
        [HttpGet]
        [Route("get-profile")]
        [Authorize]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var playerDetails = await authService.GetFullUserData(userId);
            return Ok(playerDetails);

        }
    }
}
