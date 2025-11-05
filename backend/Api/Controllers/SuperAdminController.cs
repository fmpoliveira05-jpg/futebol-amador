using Application.DTOs.SuperAdmin;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuperAdminController : ControllerBase
    {
        private readonly ISuperAdminService superAdminService;

        public SuperAdminController(ISuperAdminService superAdminService)
        {
            this.superAdminService = superAdminService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSueprAdmin([FromBody] CreateSuperAdminDTO createSuperAdminDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newSadminId = await superAdminService.CreateSuperAdminAsync(createSuperAdminDTO);

            return CreatedAtAction(
                nameof(GetSuperAdmin),
                new { sadminId = newSadminId },
                createSuperAdminDTO
                );
        }

        [HttpDelete("{sadminId:guid}")]
        public async Task<IActionResult> DeleteSuperAdmin(string sadminId)
        {
            await superAdminService.DeleteSuperAdminAsync(sadminId);

            return NoContent();
        }

        [HttpGet("{sadminId:guid}")]
        public async Task<IActionResult> GetSuperAdmin(string sadminId)
        {
            var superAdminDetails = await superAdminService.GetSuperAdminByIdAsync(sadminId);

            return Ok(superAdminDetails);
        }

        [HttpPut("{sadminId:guid}")]
        public async Task<IActionResult> UpdateSuperAdmin(string sadminId, [FromBody] UpdateSuperAdminDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await superAdminService.UpdateSuperAdminAsync(sadminId, dto);

            return Ok("Super Admin information updated succesfully.");
        }
    }
}
