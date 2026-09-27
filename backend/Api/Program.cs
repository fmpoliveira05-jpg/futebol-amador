using Api.Extensions;
using Api.Hubs.Notification;
using Api.Middlewares;
using Api.Operacao;
using Api.Seguranca;
using Application;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Application.Services;
using Google.Cloud.Firestore;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.AspNetCore.HttpOverrides;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Nos testes de integração (ambiente "Testing") o Firebase é substituído por mocks e a
// autenticação por um esquema de teste, por isso não se exigem credenciais.
var emTestes = builder.Environment.IsEnvironment("Testing");

// Kestrel sem o cabeçalho "Server", com corpo máximo de 1 MB e tempo máximo para os cabeçalhos
// (ver Api/Operacao/Limites.cs e a secção "Limites" da configuração).
builder.WebHost.ConfigurarKestrel(builder.Configuration);
builder.Services.AddTimeoutsPedidos(builder.Configuration);

// Atrás de um proxy (nginx, Caddy, ...), o IP do cliente e o esquema vêm em X-Forwarded-For e
// X-Forwarded-Proto. Só se confia nesses cabeçalhos quando vêm de proxies conhecidos
// (ForwardedHeaders:KnownProxies e ForwardedHeaders:KnownNetworks); por omissão, só o loopback.
// A limitação de pedidos por IP depende disto.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = builder.Configuration.GetValue("ForwardedHeaders:ForwardLimit", 1);

    foreach (var proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? Array.Empty<string>())
    {
        options.KnownProxies.Add(IPAddress.Parse(proxy));
    }

    foreach (var rede in builder.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? Array.Empty<string>())
    {
        var partes = rede.Split('/');
        options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse(partes[0]), int.Parse(partes[1])));
    }
});

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddSaude();
// Respostas guardadas dos pedidos com Idempotency-Key. Com várias instâncias da API, trocar por
// uma cache distribuída (AddStackExchangeRedisCache ou AddDistributedSqlServerCache).
builder.Services.AddDistributedMemoryCache();
// Cache das consultas públicas (ligas, classificação, jornadas), ver Api/Operacao/CachePublica.cs.
builder.Services.AddCachePublica(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiBackGroundService();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentacion();

// Todas as respostas de erro (exceções, 404 de rotas inexistentes, 405, 401/403 sem corpo) saem
// como ProblemDetails em português, sem detalhes internos (ver RespostasErro).
builder.Services.AddProblemDetails(RespostasErro.Configurar);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Os tokens do Firebase trazem o uid em "sub" e "user_id"; ficam os dois como NameIdentifier.
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap["sub"] = ClaimTypes.NameIdentifier;
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap["user_id"] = ClaimTypes.NameIdentifier;

if (!emTestes)
{
    builder.Services.AddFirebaseAuthentication(builder.Configuration);
    var firebaseProjectId = builder.Configuration["Firebase:ProjectId"]!;
    builder.Services.AddSingleton(_ => FirestoreDb.Create(firebaseProjectId));
}

// Pedidos REST ao Firebase e ao Turnstile com tempo máximo, disjuntor e novas tentativas só em
// métodos idempotentes (ver Limites.AddResilienciaExterna).
builder.Services.AddHttpClient<IAuthService, FireBaseAuthService>((sp, httpClient) =>
{
    var tokenUri = sp.GetRequiredService<IConfiguration>()["Authentication:TokenUri"];
    if (!string.IsNullOrEmpty(tokenUri))
    {
        httpClient.BaseAddress = new Uri(tokenUri);
    }
}).AddResilienciaExterna(builder.Configuration);

// Por omissão todos os endpoints exigem sessão (e e-mail confirmado): os públicos têm [AllowAnonymous].
builder.Services.AddPoliticasAutorizacao(builder.Configuration);
builder.Services.AddLimitacaoPedidos(builder.Configuration);

builder.Services.AddSingleton<SessaoWeb>();
builder.Services.AddHttpClient<IVerificadorTurnstile, VerificadorTurnstile>().AddResilienciaExterna(builder.Configuration);
builder.Services.AddScoped<ExigirTurnstileAttribute>();
if (emTestes || !builder.Configuration.GetValue("Auth:VerificarRevogacao", true))
{
    builder.Services.AddSingleton<IRevogacaoTokens, SemRevogacaoTokens>();
}
else
{
    builder.Services.AddSingleton<IRevogacaoTokens, RevogacaoTokensFirebase>();
}
builder.Services.AddSingleton<IAssinaturaCloudinary, AssinaturaCloudinary>();
// Apagar as imagens de quem elimina a conta (RGPD). DELETE e GET são idempotentes: há novas tentativas.
builder.Services.AddHttpClient<IImagensUtilizador, ImagensCloudinary>().AddResilienciaExterna(builder.Configuration);

// Origens autorizadas a chamar a API a partir do browser (Cors:Origins no appsettings).
var origens = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(origens)
              .WithHeaders("Content-Type", "Authorization", SessaoWeb.CabecalhoCsrf, ExigirTurnstileAttribute.Cabecalho, Idempotencia.Cabecalho)
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithExposedHeaders(PaginarListaAttribute.CabecalhoTotal)
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

var app = builder.Build();

// Numa base de dados nova, cria as divisões e as ligas; numa antiga, passa as divisões a ligas (uma vez).
if (!emTestes)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AmateurFootballContext>();
    try
    {
        await DivisoesIniciais.GarantirAsync(db);
        await LigasIniciais.GarantirAsync(db, DateTime.UtcNow);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Não foi possível verificar as divisões iniciais (a base de dados está acessível e com as migrações aplicadas?).");
    }
}

// Primeiro o IP e o esquema reais (proxy), depois os cabeçalhos de segurança, que ficam também nas
// respostas de erro.
app.UseForwardedHeaders();
app.UseMiddleware<CabecalhosSeguranca>();

// Converte as exceções em ProblemDetails com o código HTTP certo (ver GlobalExceptionHandler).
app.UseExceptionHandler();
// Respostas de erro sem corpo (rota inexistente, método errado, 401/403 da autenticação) passam a
// ProblemDetails, como as outras.
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("Frontend");

// Pedidos com a sessão em cookie têm de vir da aplicação web (X-Requested-With e Origin).
app.UseMiddleware<ProtecaoCsrf>();

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.UseRequestTimeouts();
// POST com Idempotency-Key repetido devolve a resposta do primeiro (duplo clique, novas tentativas).
app.UseMiddleware<Idempotencia>();
app.UseInvalidacaoCachePublica();
app.UseOutputCache();

app.MapControllers();
app.MapHubs();
// /health/live e /health/ready: anónimos e fora da limitação de pedidos (monitorização).
app.MapSaude();

app.Run();

/// <summary>Exposto para os testes de integração (WebApplicationFactory).</summary>
public partial class Program { }
