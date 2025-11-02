using Api.Extensions;
using Api.Middlewares;
using Application;
using Google.Cloud.Firestore;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Define a informação básica do Swagger
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Api", Version = "v1" });

    // 1. Definir o esquema de segurança (Security Scheme) que o Swagger vai usar
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Autenticação JWT (Bearer). Insira 'Bearer' [espaço] e depois o seu token.\r\n\r\nExemplo: 'Bearer eyJhbGciOi...'"
    });

    // 2. Tornar o esquema de segurança obrigatório para os endpoints
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

//adiciona o Tratador de exceções global
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];
if (string.IsNullOrEmpty(firebaseProjectId))
{
    throw new ArgumentNullException(nameof(firebaseProjectId), "Firebase:ProjectId não pode ser nulo no appsettings.json");
}

//Autenticação com Firebase
builder.Services.AddFirebaseAuthentication(builder.Configuration);

builder.Services.AddSingleton(provider => FirestoreDb.Create(firebaseProjectId));

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
