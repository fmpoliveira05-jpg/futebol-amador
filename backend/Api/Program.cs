using Api.Extensions;
using Api.Hubs.Notification;
using Api.Middlewares;
using Application;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Application.Services;
using Google.Cloud.Firestore;
using Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiBackGroundService();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();

//notification service for the application layer
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentacion();

//adiciona o Tratador de exceções global
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap["sub"] = ClaimTypes.NameIdentifier;
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap["user_id"] = ClaimTypes.NameIdentifier;

var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];
var credentialPath = builder.Configuration["Firebase:CredentialPath"];

if (string.IsNullOrEmpty(firebaseProjectId))
{
    throw new ArgumentNullException(nameof(firebaseProjectId), "Firebase:ProjectId não pode ser nulo.");
}
if (string.IsNullOrEmpty(credentialPath))
{
    throw new ArgumentNullException(nameof(credentialPath), "Firebase:CredentialPath não foi encontrado. Verifique se o seu 'secrets.json' está correto.");
}
if (!File.Exists(credentialPath))
{
    throw new FileNotFoundException($"O ficheiro de credenciais não foi encontrado no caminho especificado: {credentialPath}. Verifique o caminho no 'secrets.json'.");
}

Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);

//Autenticação com Firebase
await builder.Services.AddFirebaseAuthentication(builder.Configuration);

builder.Services.AddSingleton(provider => FirestoreDb.Create(firebaseProjectId));

builder.Services.AddHttpClient<IAuthService, FireBaseAuthService>((sp, HttpClient) =>
{ 
    var configuration = sp.GetRequiredService<IConfiguration>();
    HttpClient.BaseAddress = new Uri(configuration["Authentication:TokenUri"]);
}

);
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.MapHubs();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseCors("AllowAngular");
app.UseAuthorization();

app.MapControllers();

app.Run();

//Classe para os testes de integração
public partial class Program { }