using Microsoft.AspNetCore.RateLimiting;
using Api.Seguranca;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>Uploads de imagens (emblemas das equipas) para o Cloudinary.</summary>
    [ApiController]
    [Route("api/uploads")]
    [Produces("application/json")]
    public class UploadsController : ControllerBase
    {
        private readonly IAssinaturaCloudinary assinatura;

        public UploadsController(IAssinaturaCloudinary assinatura)
        {
            this.assinatura = assinatura;
        }

        /// <summary>
        /// Parâmetros assinados para um upload direto para o Cloudinary, restrito à pasta do utilizador,
        /// ao preset assinado e aos formatos JPG, PNG e WebP.
        /// </summary>
        /// <response code="200">Assinatura válida durante 1 hora.</response>
        /// <response code="401">Sem sessão.</response>
        /// <response code="503">O Cloudinary não está configurado no servidor.</response>
        [EnableRateLimiting(LimitacaoPedidos.Uploads)]
        [HttpPost("signature")]
        [ProducesResponseType(typeof(AssinaturaUploadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public IActionResult Signature()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!assinatura.Configurado)
            {
                return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Uploads indisponíveis", detail: "O envio de imagens não está configurado.");
            }

            return Ok(assinatura.Assinar(userId));
        }
    }
}
