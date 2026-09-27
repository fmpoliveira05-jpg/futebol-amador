using Application.DTOs;

namespace Api.Seguranca
{
    /// <summary>
    /// Sessão do frontend web em cookies <c>HttpOnly</c> (o JavaScript nunca vê os tokens).
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><see cref="CookieSessao"/> (<c>__Host-</c>, <c>Path=/</c>): o ID token do Firebase (1 hora).
    /// O JwtBearer lê-o quando o pedido não traz o cabeçalho <c>Authorization</c>.</item>
    /// <item><see cref="CookieRefresh"/> (<c>__Secure-</c>, <c>Path=/api/User/refresh</c>): o refresh token,
    /// que só viaja no pedido de renovação.</item>
    /// </list>
    /// Os dois são <c>Secure</c>, <c>HttpOnly</c> e <c>SameSite</c> (<c>Strict</c> por omissão,
    /// <c>Auth:Cookies:SameSite</c>). Com <c>Strict</c>, o frontend e a API têm de estar no mesmo
    /// site (por exemplo <c>app.dominio.pt</c> e <c>api.dominio.pt</c>).
    /// Um pedido é "web" quando traz um cabeçalho <c>Origin</c> da lista <c>Cors:Origins</c>; os outros
    /// (a app Android) continuam a receber os tokens no corpo e a usar <c>Authorization: Bearer</c>.
    /// </remarks>
    public sealed class SessaoWeb
    {
        public const string CookieSessao = "__Host-fa_session";
        public const string CookieRefresh = "__Secure-fa_refresh";
        public const string CaminhoRefresh = "/api/User/refresh";

        /// <summary>Cabeçalho obrigatório nos pedidos que alteram dados com a sessão em cookie (ver <see cref="ProtecaoCsrf"/>).</summary>
        public const string CabecalhoCsrf = "X-Requested-With";
        public const string ValorCsrf = "FutebolAmador";

        private readonly HashSet<string> origens;
        private readonly SameSiteMode sameSite;
        private readonly TimeSpan duracaoRefresh;

        public SessaoWeb(IConfiguration configuration)
        {
            origens = new HashSet<string>(
                (configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:4200" })
                    .Select(o => o.TrimEnd('/')),
                StringComparer.OrdinalIgnoreCase);

            sameSite = Enum.TryParse<SameSiteMode>(configuration["Auth:Cookies:SameSite"], true, out var modo)
                ? modo
                : SameSiteMode.Strict;

            duracaoRefresh = TimeSpan.FromDays(configuration.GetValue("Auth:Cookies:RefreshDias", 7));
        }

        /// <summary>A origem pertence à lista de origens autorizadas (CORS).</summary>
        public bool OrigemPermitida(string? origem) =>
            !string.IsNullOrEmpty(origem) && origens.Contains(origem.TrimEnd('/'));

        /// <summary>O pedido vem do frontend web (browser numa origem autorizada).</summary>
        public bool EhPedidoWeb(HttpRequest request) => OrigemPermitida(request.Headers.Origin);

        /// <summary>Grava a sessão nos cookies.</summary>
        public void EscreverCookies(HttpResponse response, FirebaseLoginResponseDto sessao)
        {
            var segundos = int.TryParse(sessao.ExpiresIn, out var s) && s > 0 ? s : 3600;

            response.Cookies.Append(CookieSessao, sessao.IdToken, Opcoes("/", TimeSpan.FromSeconds(segundos)));

            if (!string.IsNullOrEmpty(sessao.RefreshToken))
            {
                response.Cookies.Append(CookieRefresh, sessao.RefreshToken, Opcoes(CaminhoRefresh, duracaoRefresh));
            }
        }

        /// <summary>Apaga os cookies da sessão.</summary>
        public void ApagarCookies(HttpResponse response)
        {
            response.Cookies.Delete(CookieSessao, Opcoes("/", null));
            response.Cookies.Delete(CookieRefresh, Opcoes(CaminhoRefresh, null));
        }

        /// <summary>
        /// Versão da resposta de login para o browser: sem ID token nem refresh token (ficam nos cookies).
        /// </summary>
        public static LoginResponseDto SemTokens(LoginResponseDto resposta)
        {
            var sessao = resposta.FirebaseLoginResponseDto;
            resposta.FirebaseLoginResponseDto = new FirebaseLoginResponseDto
            {
                LocalId = sessao?.LocalId ?? string.Empty,
                ExpiresIn = sessao?.ExpiresIn ?? string.Empty,
                Email = sessao?.Email ?? string.Empty,
                IdToken = string.Empty,
                RefreshToken = string.Empty,
            };
            return resposta;
        }

        private CookieOptions Opcoes(string caminho, TimeSpan? duracao) => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = sameSite,
            Path = caminho,
            IsEssential = true,
            MaxAge = duracao,
        };
    }
}
