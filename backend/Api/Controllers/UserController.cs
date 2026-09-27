using Api.Seguranca;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de autenticação e perfil do utilizador.
    /// </summary>
    /// <remarks>
    /// Os pedidos do frontend web (com <c>Origin</c> autorizado) recebem a sessão em cookies
    /// <c>HttpOnly</c> e a resposta não traz tokens (ver <see cref="SessaoWeb"/>); a app Android
    /// recebe os tokens no corpo e usa <c>Authorization: Bearer</c>.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UserController : Controller
    {
        /// <summary>Resposta igual para qualquer pedido de reenvio/recuperação (não revela contas).</summary>
        public const string MensagemEnvioGenerica =
            "Se os dados estiverem certos, vais receber um e-mail dentro de alguns minutos.";

        #region Inicializar 
        private readonly IAuthService authService;
        private readonly SessaoWeb sessaoWeb;
        private readonly IRevogacaoTokens revogacao;

        public UserController(IAuthService authService, SessaoWeb sessaoWeb, IRevogacaoTokens revogacao)
        {
            this.authService = authService;
            this.sessaoWeb = sessaoWeb;
            this.revogacao = revogacao;
        }
        #endregion

        /// <summary>
        /// Realiza o login de um utilizador na aplicação.
        /// </summary>
        /// <remarks>
        /// Envia as credenciais (email e password) para obter um token de autenticação JWT e os dados do perfil do utilizador.
        /// No frontend web, os tokens ficam em cookies HttpOnly e não vêm no corpo.
        /// </remarks>
        /// <param name="loginData">Objeto contendo o email e a password do utilizador.</param>
        /// <returns>Um objeto <see cref="LoginResponseDto"/> com o token e os dados do utilizador.</returns>
        /// <response code="200">Login efetuado com sucesso.</response>
        /// <response code="401">Credenciais inválidas.</response>
        /// <response code="403">E-mail por confirmar (<c>codigo: email_nao_verificado</c>).</response>
        /// <response code="429">Demasiadas tentativas.</response>
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        [EnableRateLimiting(LimitacaoPedidos.Autenticacao)]
        [ExigirTurnstile]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginData)
        {
            var resposta = await authService.LoginAsync(loginData.Email, loginData.Password);
            return Ok(EntregarSessao(resposta));
        }

        /// <summary>
        /// Renova a sessão web com o refresh token guardado no cookie <c>__Secure-fa_refresh</c>.
        /// </summary>
        /// <response code="204">Cookies renovados.</response>
        /// <response code="401">Sem sessão ou sessão revogada (os cookies são apagados).</response>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting(LimitacaoPedidos.Sessao)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue(SessaoWeb.CookieRefresh, out var refreshToken) || string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized();
            }

            try
            {
                var sessao = await authService.RenovarSessaoAsync(refreshToken);
                sessaoWeb.EscreverCookies(Response, sessao);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                sessaoWeb.ApagarCookies(Response);
                return Unauthorized();
            }
        }

        /// <summary>
        /// Termina a sessão do utilizador (Logout).
        /// </summary>
        /// <remarks>
        /// Revoga os refresh tokens do utilizador no Firebase (todas as sessões, em todos os
        /// dispositivos) e apaga os cookies da sessão web. É <c>POST</c>: um <c>GET</c> podia ser
        /// disparado por uma imagem noutro site.
        /// </remarks>
        /// <response code="204">Sessão terminada com sucesso.</response>
        /// <response code="401">Utilizador não está autenticado.</response>
        [HttpPost]
        [Route("logout")]
        [Authorize(Policy = PoliticasAutorizacao.SemVerificacaoEmail)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await authService.LogoutAsync(userId);
            revogacao.Invalidar(userId);
            sessaoWeb.ApagarCookies(Response);
            return NoContent();
        }

        /// <summary>
        /// Volta a enviar a mensagem de confirmação do e-mail.
        /// </summary>
        /// <remarks>
        /// Pede o e-mail e a palavra-passe (quem ainda não confirmou o e-mail não tem sessão). A resposta
        /// é sempre a mesma, estejam os dados certos ou não.
        /// </remarks>
        /// <response code="202">Pedido aceite.</response>
        /// <response code="429">Demasiados pedidos.</response>
        [HttpPost("resend-verification")]
        [AllowAnonymous]
        [EnableRateLimiting(LimitacaoPedidos.Email)]
        [ExigirTurnstile]
        [ProducesResponseType(typeof(MensagemDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ResendVerification([FromBody] ReenviarVerificacaoDto dto)
        {
            await authService.ReenviarVerificacaoEmailAsync(dto.Email, dto.Password);
            return Accepted(new MensagemDto { Mensagem = MensagemEnvioGenerica });
        }

        /// <summary>
        /// Pede a mensagem de recuperação da palavra-passe (enviada pelo Firebase).
        /// </summary>
        /// <remarks>A resposta é sempre a mesma, exista ou não uma conta com o e-mail.</remarks>
        /// <response code="202">Pedido aceite.</response>
        /// <response code="429">Demasiados pedidos.</response>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [EnableRateLimiting(LimitacaoPedidos.Email)]
        [ExigirTurnstile]
        [ProducesResponseType(typeof(MensagemDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ForgotPassword([FromBody] RecuperarPalavraPasseDto dto)
        {
            await authService.PedirRecuperacaoPalavraPasseAsync(dto.Email);
            return Accepted(new MensagemDto { Mensagem = MensagemEnvioGenerica });
        }

        /// <summary>
        /// Altera a palavra-passe do utilizador autenticado, depois de confirmar a atual.
        /// </summary>
        /// <remarks>
        /// As outras sessões terminam. No frontend web, os cookies passam logo para a sessão nova.
        /// </remarks>
        /// <response code="204">Palavra-passe alterada.</response>
        /// <response code="400">Palavra-passe atual errada ou nova palavra-passe inválida.</response>
        /// <response code="401">Sem sessão.</response>
        /// <response code="429">Demasiadas tentativas.</response>
        [HttpPut("password")]
        [EnableRateLimiting(LimitacaoPedidos.PalavraPasse)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var novaSessao = await authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
            revogacao.Invalidar(userId);

            if (sessaoWeb.EhPedidoWeb(Request))
            {
                if (novaSessao != null)
                {
                    sessaoWeb.EscreverCookies(Response, novaSessao);
                }
                else
                {
                    sessaoWeb.ApagarCookies(Response);
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Obtém o perfil completo do utilizador autenticado.
        /// </summary>
        /// <remarks>
        /// Retorna todos os detalhes do utilizador (Player ou SuperAdmin) com base no token JWT fornecido.
        /// O frontend web usa-o para confirmar que a sessão (cookie) continua válida.
        /// </remarks>
        /// <returns>Um objeto <see cref="LoginResponseDto"/> com os detalhes do perfil.</returns>
        /// <response code="200">Perfil obtido com sucesso.</response>
        /// <response code="401">Utilizador não está autenticado ou ID inválido.</response>
        /// <response code="404">Utilizador não encontrado na base de dados.</response>
        [HttpGet]
        [Route("get-profile")]
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

        /// <summary>
        /// Entrega a sessão ao cliente: no browser vai para cookies e sai do corpo; na app fica no corpo.
        /// </summary>
        private LoginResponseDto EntregarSessao(LoginResponseDto resposta)
        {
            if (!sessaoWeb.EhPedidoWeb(Request) || resposta.FirebaseLoginResponseDto == null)
            {
                return resposta;
            }

            sessaoWeb.EscreverCookies(Response, resposta.FirebaseLoginResponseDto);
            return SessaoWeb.SemTokens(resposta);
        }
    }
}
