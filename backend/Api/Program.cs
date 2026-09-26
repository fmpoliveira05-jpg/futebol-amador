using Api.Extensions;
using Api.Hubs.Notification;
using Api.Middlewares;
using Application;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Application.Services;
using Google.Cloud.Firestore;
using Infrastructure;
using Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Nos testes de integração (ambiente "Testing") o Firebase é substituído por mocks e a
// autenticação por um esquema de teste, por isso não se exigem credenciais.
var emTestes = builder.Environment.IsEnvironment("Testing");

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiBackGroundService();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentacion();

builder.Services.AddProblemDetails();
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

builder.Services.AddHttpClient<IAuthService, FireBaseAuthService>((sp, httpClient) =>
{
    var tokenUri = sp.GetRequiredService<IConfiguration>()["Authentication:TokenUri"];
    if (!string.IsNullOrEmpty(tokenUri))
    {
        httpClient.BaseAddress = new Uri(tokenUri);
    }
});

builder.Services.AddAuthorization();

// Origens autorizadas a chamar a API a partir do browser (Cors:Origins no appsettings).
var origens = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(origens)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
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

// Converte as exceções em ProblemDetails com o código HTTP certo (ver GlobalExceptionHandler).
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHubs();

app.Run();

/// <summary>Exposto para os testes de integração (WebApplicationFactory).</summary>
public partial class Program { }
