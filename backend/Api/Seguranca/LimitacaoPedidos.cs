using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Api.Seguranca
{
    /// <summary>
    /// Limitação de pedidos (ASP.NET Core Rate Limiting), por endereço IP do cliente.
    /// </summary>
    /// <remarks>
    /// <para>Há um limite global para todos os pedidos e políticas mais apertadas para os endpoints
    /// que um atacante usaria para adivinhar palavras-passe ou abusar do envio de e-mails. Quando o
    /// limite é ultrapassado a API responde <c>429 Too Many Requests</c> com <c>Retry-After</c>.</para>
    /// <para>O IP é o de <c>HttpContext.Connection.RemoteIpAddress</c>; atrás de um proxy só fica certo
    /// com os cabeçalhos <c>X-Forwarded-For</c> de proxies conhecidos (ver <c>ForwardedHeaders</c> no
    /// Program.cs). Os valores vêm da secção <c>LimitacaoPedidos</c> da configuração.</para>
    /// </remarks>
    public static class LimitacaoPedidos
    {
        /// <summary>Login (janela deslizante por IP).</summary>
        public const string Autenticacao = "autenticacao";
        /// <summary>Renovação da sessão web.</summary>
        public const string Sessao = "sessao";
        /// <summary>Criação de contas (janela fixa por IP).</summary>
        public const string Registo = "registo";
        /// <summary>Envio de e-mails (confirmação e recuperação da palavra-passe).</summary>
        public const string Email = "email";
        /// <summary>Operações que confirmam a palavra-passe atual (por utilizador e IP).</summary>
        public const string PalavraPasse = "palavra-passe";
        /// <summary>Quota por utilizador: assinaturas de upload para o Cloudinary.</summary>
        public const string Uploads = "uploads";
        /// <summary>Quota por utilizador: salas de chat criadas.</summary>
        public const string SalasChat = "salas-chat";
        /// <summary>Quota por utilizador: convites de jogo, contrapropostas, pedidos/convites de adesão e propostas de transferência.</summary>
        public const string Convites = "convites";
        /// <summary>Quota por utilizador: equipas criadas.</summary>
        public const string Equipas = "equipas";

        /// <summary>Limite de uma política: pedidos permitidos por janela.</summary>
        public sealed record Limite(int Pedidos, int JanelaSegundos);

        /// <summary>
        /// Valores por omissão, sobrepostos por <c>LimitacaoPedidos:{Nome}:Pedidos</c> e
        /// <c>LimitacaoPedidos:{Nome}:JanelaSegundos</c> (por exemplo, a variável de ambiente
        /// <c>LimitacaoPedidos__Autenticacao__Pedidos=20</c>).
        /// </summary>
        public static readonly IReadOnlyDictionary<string, Limite> Predefinidos = new Dictionary<string, Limite>
        {
            ["Global"] = new(300, 60),
            ["Autenticacao"] = new(10, 300),
            ["Sessao"] = new(30, 300),
            ["Registo"] = new(5, 3600),
            ["Email"] = new(5, 900),
            ["PalavraPasse"] = new(5, 900),
            // Quotas por utilizador para operações que custam dinheiro ou incomodam terceiros.
            ["Uploads"] = new(20, 3600),
            ["SalasChat"] = new(10, 86400),
            ["Convites"] = new(30, 3600),
            ["Equipas"] = new(3, 86400),
        };

        public static IServiceCollection AddLimitacaoPedidos(this IServiceCollection services, IConfiguration configuration)
        {
            Limite Ler(string nome)
            {
                var seccao = configuration.GetSection($"LimitacaoPedidos:{nome}");
                var predefinido = Predefinidos[nome];
                return new Limite(
                    seccao.GetValue("Pedidos", predefinido.Pedidos),
                    seccao.GetValue("JanelaSegundos", predefinido.JanelaSegundos));
            }

            var global = Ler("Global");
            var autenticacao = Ler("Autenticacao");
            var sessao = Ler("Sessao");
            var registo = Ler("Registo");
            var email = Ler("Email");
            var palavraPasse = Ler("PalavraPasse");
            var quotas = new Dictionary<string, Limite>
            {
                [Uploads] = Ler("Uploads"),
                [SalasChat] = Ler("SalasChat"),
                [Convites] = Ler("Convites"),
                [Equipas] = Ler("Equipas"),
            };

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                    RateLimitPartition.GetFixedWindowLimiter(EnderecoCliente(ctx), _ => Janela(global)));

                options.AddPolicy(Autenticacao, ctx =>
                    RateLimitPartition.GetSlidingWindowLimiter(EnderecoCliente(ctx), _ => JanelaDeslizante(autenticacao)));

                options.AddPolicy(Sessao, ctx =>
                    RateLimitPartition.GetSlidingWindowLimiter(EnderecoCliente(ctx), _ => JanelaDeslizante(sessao)));

                options.AddPolicy(Registo, ctx =>
                    RateLimitPartition.GetFixedWindowLimiter(EnderecoCliente(ctx), _ => Janela(registo)));

                options.AddPolicy(Email, ctx =>
                    RateLimitPartition.GetFixedWindowLimiter(EnderecoCliente(ctx), _ => Janela(email)));

                // Por utilizador e por IP: um atacante com uma sessão roubada não consegue testar
                // palavras-passe à vontade trocando de IP, nem a partir de um IP para várias contas.
                options.AddPolicy(PalavraPasse, ctx =>
                {
                    var utilizador = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonimo";
                    return RateLimitPartition.GetSlidingWindowLimiter($"{utilizador}|{EnderecoCliente(ctx)}", _ => JanelaDeslizante(palavraPasse));
                });

                // Quotas por utilizador (janela fixa): o mesmo utilizador não passa o limite trocando de
                // IP. Sem sessão, a partição é o IP (o pedido é recusado depois pela autorização).
                foreach (var (nome, limite) in quotas)
                {
                    options.AddPolicy(nome, ctx =>
                    {
                        var utilizador = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var particao = utilizador != null ? $"u:{utilizador}" : $"ip:{EnderecoCliente(ctx)}";
                        return RateLimitPartition.GetFixedWindowLimiter(particao, _ => Janela(limite));
                    });
                }

                options.OnRejected = async (contexto, cancellationToken) =>
                {
                    var segundos = contexto.Lease.TryGetMetadata(MetadataName.RetryAfter, out var espera)
                        ? (int)Math.Ceiling(espera.TotalSeconds)
                        : 60;

                    var resposta = contexto.HttpContext.Response;
                    resposta.StatusCode = StatusCodes.Status429TooManyRequests;
                    resposta.Headers.RetryAfter = Math.Max(1, segundos).ToString(CultureInfo.InvariantCulture);

                    await resposta.WriteAsJsonAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Demasiados pedidos",
                        Detail = "Fizeste demasiados pedidos seguidos. Espera um pouco e tenta outra vez.",
                    }, cancellationToken);
                };
            });

            return services;
        }

        /// <summary>IP do cliente (já corrigido pelo UseForwardedHeaders quando há proxy conhecido).</summary>
        internal static string EnderecoCliente(HttpContext ctx) =>
            ctx.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

        private static FixedWindowRateLimiterOptions Janela(Limite limite) => new()
        {
            PermitLimit = limite.Pedidos,
            Window = TimeSpan.FromSeconds(limite.JanelaSegundos),
            QueueLimit = 0,
            AutoReplenishment = true,
        };

        private static SlidingWindowRateLimiterOptions JanelaDeslizante(Limite limite) => new()
        {
            PermitLimit = limite.Pedidos,
            Window = TimeSpan.FromSeconds(limite.JanelaSegundos),
            SegmentsPerWindow = 5,
            QueueLimit = 0,
            AutoReplenishment = true,
        };
    }
}
