using Microsoft.AspNetCore.OutputCaching;

namespace Api.Operacao
{
    /// <summary>
    /// Cache no servidor (Output Caching) das consultas públicas mais pedidas: ligas, classificação,
    /// jornadas e táticas. São iguais para todos os utilizadores, por isso ficam em cache mesmo nos
    /// pedidos com sessão.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Duração: <c>Cache:PublicaSegundos</c> (60 s). O cliente recebe
    /// <c>Cache-Control: public, max-age</c> com o mesmo valor; todas as outras respostas da API
    /// levam <c>no-store</c>.</item>
    /// <item>Invalidação: qualquer POST/PUT/DELETE com sucesso na API apaga a cache (etiqueta
    /// <see cref="Etiqueta"/>). As alterações feitas pelos serviços em segundo plano (sorteio e fecho
    /// das épocas) aparecem no máximo ao fim da duração.</item>
    /// <item>Em memória, por instância. Com várias instâncias, cada uma tem a sua cache (continua
    /// correto, porque a duração é curta).</item>
    /// </list>
    /// </remarks>
    public static class CachePublica
    {
        public const string Politica = "publica";
        public const string Etiqueta = "dados-publicos";

        public static int Segundos(IConfiguration configuracao) => configuracao.GetValue("Cache:PublicaSegundos", 60);

        public static IServiceCollection AddCachePublica(this IServiceCollection services, IConfiguration configuracao)
        {
            var duracao = TimeSpan.FromSeconds(Segundos(configuracao));
            services.AddOutputCache(opcoes =>
            {
                opcoes.SizeLimit = 64 * 1024 * 1024;
                opcoes.AddPolicy(Politica, b => b
                    .AddPolicy<PoliticaPublica>()
                    .Expire(duracao)
                    .SetVaryByQuery("*")
                    .Tag(Etiqueta), excludeDefaultPolicy: true);
            });
            return services;
        }

        /// <summary>Apaga a cache depois de um pedido que altera dados ter tido sucesso.</summary>
        public static IApplicationBuilder UseInvalidacaoCachePublica(this IApplicationBuilder app) =>
            app.Use(async (contexto, next) =>
            {
                await next();

                var metodo = contexto.Request.Method;
                if (!HttpMethods.IsGet(metodo) && !HttpMethods.IsHead(metodo) && !HttpMethods.IsOptions(metodo) &&
                    contexto.Response.StatusCode < 400 &&
                    contexto.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                {
                    var loja = contexto.RequestServices.GetRequiredService<IOutputCacheStore>();
                    await loja.EvictByTagAsync(Etiqueta, CancellationToken.None);
                }
            });

        /// <summary>O endpoint usa a cache pública (para o Cache-Control da resposta).</summary>
        internal static bool Usa(Endpoint? endpoint) =>
            endpoint?.Metadata.GetMetadata<OutputCacheAttribute>() is { PolicyName: Politica };

        /// <summary>
        /// Como a política predefinida, mas também em pedidos autenticados (os dados são públicos).
        /// Só guarda respostas 200 sem cookies.
        /// </summary>
        private sealed class PoliticaPublica : IOutputCachePolicy
        {
            public ValueTask CacheRequestAsync(OutputCacheContext contexto, CancellationToken cancelamento)
            {
                var metodo = contexto.HttpContext.Request.Method;
                var ativa = HttpMethods.IsGet(metodo) || HttpMethods.IsHead(metodo);
                contexto.EnableOutputCaching = true;
                contexto.AllowCacheLookup = ativa;
                contexto.AllowCacheStorage = ativa;
                contexto.AllowLocking = true;
                contexto.CacheVaryByRules.QueryKeys = "*";
                return ValueTask.CompletedTask;
            }

            public ValueTask ServeFromCacheAsync(OutputCacheContext contexto, CancellationToken cancelamento) => ValueTask.CompletedTask;

            public ValueTask ServeResponseAsync(OutputCacheContext contexto, CancellationToken cancelamento)
            {
                var resposta = contexto.HttpContext.Response;
                if (resposta.StatusCode != StatusCodes.Status200OK || !Microsoft.Extensions.Primitives.StringValues.IsNullOrEmpty(resposta.Headers.SetCookie))
                {
                    contexto.AllowCacheStorage = false;
                }
                return ValueTask.CompletedTask;
            }
        }
    }
}
