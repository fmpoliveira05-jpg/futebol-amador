using Application.DTOs.SuperAdmin;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SuperAdminController : ControllerBase
    {
        #region Initialization
        private readonly ISuperAdminService superAdminService;
        private readonly IPlayerAuthorizationValidator playerAuthorizationValidator;
        private readonly IAuthService authService;


        public SuperAdminController(ISuperAdminService superAdminService, IPlayerAuthorizationValidator playerAuthorizationValidator, IAuthService authService)
        {
            this.superAdminService = superAdminService;
            this.playerAuthorizationValidator = playerAuthorizationValidator;
            this.authService = authService;

        }

        #endregion

        #region endPoints

        #region CRUD Super Admin
        [HttpPost]
        [AllowAnonymous]
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

        [HttpDelete("{sadminId:required}")]
        public async Task<IActionResult> DeleteSuperAdmin(string sadminId)
        {
            playerAuthorizationValidator.ValidateUserIdIsSameUrl(GetCurrentUserId(), sadminId);
            await superAdminService.DeleteSuperAdminAsync(sadminId);

            return NoContent();
        }

        [HttpGet("{sadminId:required}")]
        public async Task<IActionResult> GetSuperAdmin(string sadminId)
        {
            var superAdminDetails = await superAdminService.GetSuperAdminByIdAsync(sadminId);

            return Ok(superAdminDetails);
        }

        [HttpPut("{sadminId:required}")]
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
