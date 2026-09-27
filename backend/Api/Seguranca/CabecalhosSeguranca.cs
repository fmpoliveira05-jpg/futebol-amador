namespace Api.Seguranca
{
    /// <summary>
    /// Cabeçalhos de segurança em todas as respostas da API (incluindo erros).
    /// </summary>
    /// <remarks>
    /// A API só devolve JSON, por isso a CSP é a mais restritiva possível (<c>default-src 'none'</c>)
    /// e nenhuma resposta pode ser mostrada numa moldura. As respostas de <c>/api</c> também não ficam
    /// em cache (têm dados pessoais e tokens). Em desenvolvimento, o Swagger UI (<c>/swagger</c>) não
    /// leva a CSP, porque precisa de scripts e estilos.
    /// Os cabeçalhos são escritos em <c>OnStarting</c> para sobreviverem ao
    /// <c>UseExceptionHandler</c>, que limpa a resposta antes de escrever o erro.
    /// </remarks>
    public sealed class CabecalhosSeguranca
    {
        private readonly RequestDelegate next;
        private readonly bool desenvolvimento;

        public CabecalhosSeguranca(RequestDelegate next, IWebHostEnvironment ambiente)
        {
            this.next = next;
            desenvolvimento = ambiente.IsDevelopment();
        }

        public Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                Aplicar(context, desenvolvimento);
                return Task.CompletedTask;
            });

            return next(context);
        }

        /// <summary>Escreve os cabeçalhos na resposta.</summary>
        internal static void Aplicar(HttpContext context, bool desenvolvimento)
        {
            var cabecalhos = context.Response.Headers;
            var caminho = context.Request.Path;

            var swagger = desenvolvimento && caminho.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase);
            if (!swagger)
            {
                cabecalhos.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";
            }

            cabecalhos.XContentTypeOptions = "nosniff";
            cabecalhos.XFrameOptions = "DENY";
            cabecalhos["Referrer-Policy"] = "no-referrer";
            cabecalhos["Permissions-Policy"] =
                "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";
            cabecalhos["Cross-Origin-Opener-Policy"] = "same-origin";
            // same-site: o frontend web (noutro subdomínio do mesmo site) continua a poder usar a API.
            cabecalhos["Cross-Origin-Resource-Policy"] = "same-site";
            cabecalhos["X-Permitted-Cross-Domain-Policies"] = "none";

            if (caminho.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                // Só as consultas públicas em cache no servidor (ligas, classificação) podem ficar em
                // cache no cliente; tudo o resto (dados pessoais, sessão) é no-store.
                if (context.Response.StatusCode == StatusCodes.Status200OK && Api.Operacao.CachePublica.Usa(context.GetEndpoint()))
                {
                    var segundos = Api.Operacao.CachePublica.Segundos(context.RequestServices.GetRequiredService<IConfiguration>());
                    cabecalhos.CacheControl = $"public, max-age={segundos}";
                    cabecalhos.Remove("Pragma");
                }
                else
                {
                    cabecalhos.CacheControl = "no-store";
                    cabecalhos.Pragma = "no-cache";
                }
            }

            cabecalhos.Remove("Server");
            cabecalhos.Remove("X-Powered-By");
        }
    }
}
