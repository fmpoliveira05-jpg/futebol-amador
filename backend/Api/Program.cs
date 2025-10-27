using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Api.Extensions;
using Application;
using Infrastructure;
using Api.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//adiciona o Tratador de exceções global
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// 1. INICIALIZAR O SDK DO FIREBASE USANDO AS CREDENCIAIS PADRÃO DA APLICAÇÃO
var firebaseCredentialPath = builder.Configuration["Firebase:CredentialPath"];

// 2. INICIALIZAR O SDK DO FIREBASE USANDO O FICHEIRO
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile(firebaseCredentialPath)
});

var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];

// 3. CONFIGURAR A AUTENTICAÇÃO JWT (O próximo passo da receita original)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{firebaseProjectId}",
            ValidateAudience = true,
            ValidAudience = firebaseProjectId,
            ValidateLifetime = true
        };
    });

var app = builder.Build();

//Hubs
app.MapHubs();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
