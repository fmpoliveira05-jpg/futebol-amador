using FirebaseAdmin.Auth;
using Microsoft.Extensions.Caching.Memory;

namespace Api.Seguranca
{
    /// <summary>
    /// Verifica se a sessão de um token ainda é válida: a conta existe, não está desativada e os
    /// tokens não foram revogados depois do login (<c>auth_time</c>).
    /// </summary>
    /// <remarks>
    /// É o que o <c>VerifyIdTokenAsync(token, checkRevoked: true)</c> do SDK Admin faz, mas com o
    /// estado da conta em cache (<c>Auth:RevogacaoCacheSegundos</c>, 60 s por omissão), para não
    /// ir ao Firebase em cada pedido. O logout e as mudanças de palavra-passe e de e-mail feitas
    /// pela API limpam a cache desse utilizador, por isso nesta instância têm efeito imediato.
    /// </remarks>
    public interface IRevogacaoTokens
    {
        /// <summary>
        /// <c>true</c> se o token emitido para <paramref name="uid"/> com <paramref name="authTimeSegundos"/>
        /// ainda for aceite.
        /// </summary>
        Task<bool> SessaoValidaAsync(string uid, long authTimeSegundos, CancellationToken cancellationToken = default);

        /// <summary>Esquece o estado guardado do utilizador (depois de revogar sessões).</summary>
        void Invalidar(string uid);
    }

    /// <summary>Implementação com o SDK Admin do Firebase.</summary>
    public sealed class RevogacaoTokensFirebase : IRevogacaoTokens
    {
        private readonly IMemoryCache cache;
        private readonly ILogger<RevogacaoTokensFirebase> logger;
        private readonly TimeSpan duracaoCache;

        private sealed record EstadoConta(bool Existe, bool Desativada, long ValidoDesdeMs);

        public RevogacaoTokensFirebase(IMemoryCache cache, IConfiguration configuration, ILogger<RevogacaoTokensFirebase> logger)
        {
            this.cache = cache;
            this.logger = logger;
            duracaoCache = TimeSpan.FromSeconds(configuration.GetValue("Auth:RevogacaoCacheSegundos", 60));
        }

        private static string Chave(string uid) => $"revogacao:{uid}";

        public async Task<bool> SessaoValidaAsync(string uid, long authTimeSegundos, CancellationToken cancellationToken = default)
        {
            if (!cache.TryGetValue(Chave(uid), out EstadoConta? estado) || estado == null)
            {
                try
                {
                    var utilizador = await FirebaseAuth.DefaultInstance.GetUserAsync(uid, cancellationToken);
                    var validoDesde = utilizador.TokensValidAfterTimestamp == default
                        ? 0
                        : new DateTimeOffset(DateTime.SpecifyKind(utilizador.TokensValidAfterTimestamp, DateTimeKind.Utc)).ToUnixTimeMilliseconds();
                    estado = new EstadoConta(true, utilizador.Disabled, validoDesde);
                }
                catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
                {
                    estado = new EstadoConta(false, true, long.MaxValue);
                }
                catch (Exception ex) when (ex is FirebaseAuthException or HttpRequestException or TaskCanceledException)
                {
                    // O token já foi validado (assinatura, emissor, validade); a revogação é uma defesa
                    // extra. Se o Firebase não responder, deixa-se passar e tenta-se no próximo pedido.
                    logger.LogWarning("Não foi possível confirmar a revogação da sessão ({Tipo}).", ex.GetType().Name);
                    return true;
                }

                cache.Set(Chave(uid), estado, duracaoCache);
            }

            return estado.Existe && !estado.Desativada && authTimeSegundos * 1000 >= estado.ValidoDesdeMs;
        }

        public void Invalidar(string uid) => cache.Remove(Chave(uid));
    }

    /// <summary>Sem verificação (testes de integração e <c>Auth:VerificarRevogacao=false</c>).</summary>
    public sealed class SemRevogacaoTokens : IRevogacaoTokens
    {
        public Task<bool> SessaoValidaAsync(string uid, long authTimeSegundos, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public void Invalidar(string uid)
        {
        }
    }
}
