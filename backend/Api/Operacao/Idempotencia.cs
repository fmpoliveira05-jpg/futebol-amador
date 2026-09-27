using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Api.Operacao
{
    /// <summary>
    /// Pedidos repetidos com o cabeçalho <c>Idempotency-Key</c> (duplo clique, nova tentativa depois
    /// de um timeout de rede) não voltam a criar nada: a API devolve a resposta guardada do primeiro.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Só pedidos <c>POST</c> a <c>/api</c> com sessão (a chave é do utilizador: o mesmo valor
    /// de dois utilizadores não colide) e com o cabeçalho.</item>
    /// <item>Guarda o estado, o tipo e o corpo das respostas abaixo de 500 durante
    /// <c>Idempotencia:HorasRetencao</c> (24 h). Um 5xx não fica guardado: a nova tentativa é
    /// processada.</item>
    /// <item>O mesmo pedido ainda em processamento → <c>409</c>; a mesma chave com um corpo ou
    /// endereço diferente → <c>422</c>. A resposta repetida leva <c>Idempotent-Replayed: true</c>.</item>
    /// <item>Guarda em <see cref="IDistributedCache"/> (em memória por omissão). Com várias instâncias
    /// da API, registar uma cache distribuída (Redis ou SQL Server) — o bloqueio de pedidos em curso
    /// é por instância; as restrições únicas da base de dados cobrem o resto.</item>
    /// </list>
    /// </remarks>
    public sealed partial class Idempotencia
    {
        public const string Cabecalho = "Idempotency-Key";
        public const string CabecalhoRepetido = "Idempotent-Replayed";
        private const int TamanhoMaximoCorpoGuardado = 256 * 1024;

        private static readonly ConcurrentDictionary<string, byte> EmCurso = new();

        private readonly RequestDelegate next;
        private readonly IDistributedCache cache;
        private readonly TimeSpan retencao;

        public Idempotencia(RequestDelegate next, IDistributedCache cache, IConfiguration configuracao)
        {
            this.next = next;
            this.cache = cache;
            retencao = TimeSpan.FromHours(configuracao.GetValue("Idempotencia:HorasRetencao", 24));
        }

        [GeneratedRegex("^[A-Za-z0-9_-]{8,100}$")]
        private static partial Regex FormatoChave();

        public async Task InvokeAsync(HttpContext contexto)
        {
            var pedido = contexto.Request;
            var chave = pedido.Headers[Cabecalho].ToString();
            var utilizador = contexto.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!HttpMethods.IsPost(pedido.Method) || string.IsNullOrEmpty(chave) || string.IsNullOrEmpty(utilizador) ||
                !pedido.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                await next(contexto);
                return;
            }

            if (!FormatoChave().IsMatch(chave))
            {
                await Problema(contexto, StatusCodes.Status400BadRequest, "Idempotency-Key inválida",
                    "A chave tem de ter entre 8 e 100 letras, algarismos, '-' ou '_'.");
                return;
            }

            var chaveCache = $"idem:{utilizador}:{chave}";
            var impressao = await ImpressaoAsync(pedido);

            var guardada = await LerAsync(chaveCache, contexto.RequestAborted);
            if (guardada != null)
            {
                await Repetir(contexto, guardada, impressao);
                return;
            }

            if (!EmCurso.TryAdd(chaveCache, 0))
            {
                await Problema(contexto, StatusCodes.Status409Conflict, "Pedido em processamento",
                    "Este pedido já está a ser processado. Aguarda a resposta.");
                return;
            }

            try
            {
                // Pode ter terminado entre a leitura e o bloqueio.
                guardada = await LerAsync(chaveCache, contexto.RequestAborted);
                if (guardada != null)
                {
                    await Repetir(contexto, guardada, impressao);
                    return;
                }

                var original = contexto.Response.Body;
                await using var copia = new MemoryStream();
                contexto.Response.Body = copia;
                try
                {
                    await next(contexto);
                }
                finally
                {
                    contexto.Response.Body = original;
                }

                copia.Position = 0;
                if (contexto.Response.StatusCode < 500 && copia.Length <= TamanhoMaximoCorpoGuardado)
                {
                    var resposta = new RespostaGuardada(impressao, contexto.Response.StatusCode,
                        contexto.Response.ContentType, copia.ToArray());
                    await cache.SetAsync(chaveCache, JsonSerializer.SerializeToUtf8Bytes(resposta),
                        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = retencao });
                }

                copia.Position = 0;
                await copia.CopyToAsync(original, contexto.RequestAborted);
            }
            finally
            {
                EmCurso.TryRemove(chaveCache, out _);
            }
        }

        /// <summary>Método, caminho e corpo do pedido (SHA-256): a chave não pode ser reutilizada noutro pedido.</summary>
        private static async Task<string> ImpressaoAsync(HttpRequest pedido)
        {
            pedido.EnableBuffering();
            using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            sha.AppendData(System.Text.Encoding.UTF8.GetBytes($"{pedido.Method} {pedido.Path}{pedido.QueryString}\n"));

            var buffer = new byte[16 * 1024];
            int lidos;
            while ((lidos = await pedido.Body.ReadAsync(buffer)) > 0)
            {
                sha.AppendData(buffer, 0, lidos);
            }
            pedido.Body.Position = 0;
            return Convert.ToHexString(sha.GetHashAndReset());
        }

        private async Task<RespostaGuardada?> LerAsync(string chave, CancellationToken cancelamento)
        {
            var bytes = await cache.GetAsync(chave, cancelamento);
            return bytes == null ? null : JsonSerializer.Deserialize<RespostaGuardada>(bytes);
        }

        private static async Task Repetir(HttpContext contexto, RespostaGuardada guardada, string impressao)
        {
            if (!string.Equals(guardada.Impressao, impressao, StringComparison.Ordinal))
            {
                await Problema(contexto, StatusCodes.Status422UnprocessableEntity, "Idempotency-Key reutilizada",
                    "Esta chave já foi usada num pedido diferente. Gera uma chave nova para cada operação.");
                return;
            }

            contexto.Response.StatusCode = guardada.Estado;
            if (!string.IsNullOrEmpty(guardada.TipoConteudo))
            {
                contexto.Response.ContentType = guardada.TipoConteudo;
            }
            contexto.Response.Headers[CabecalhoRepetido] = "true";
            await contexto.Response.Body.WriteAsync(guardada.Corpo);
        }

        private static Task Problema(HttpContext contexto, int estado, string titulo, string detalhe)
        {
            contexto.Response.StatusCode = estado;
            return contexto.Response.WriteAsJsonAsync(new ProblemDetails { Status = estado, Title = titulo, Detail = detalhe },
                options: null, contentType: "application/problem+json");
        }

        private sealed record RespostaGuardada(string Impressao, int Estado, string? TipoConteudo, byte[] Corpo);
    }
}
