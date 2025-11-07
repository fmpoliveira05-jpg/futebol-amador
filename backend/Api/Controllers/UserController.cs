using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        #region Inicializar 
        private readonly IAuthService authService;
        
        public UserController(IAuthService authService) { 
            this.authService = authService;

        }
        #endregion

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginData)
        {
            return Ok(await authService.LoginAsync(loginData.Email, loginData.Password));
        }

        [HttpGet]
        [Route("logout")]
        [Authorize]
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

        [HttpDelete]
        [Route("delete")]
        [Authorize]
        public IActionResult DeleteUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            authService.DeleteUser(userId);
            return NoContent();
        }

        [HttpGet]
        [Route("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromQuery] string currentPassword, [FromQuery] string newPassword)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await authService.ChangePasswordAsync(userId, currentPassword, newPassword);
            return NoContent();
        }

    }
}
