using Api.Extensions;
using Api.Middlewares;
using Application;
using Infrastructure;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiBackGroundService();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentacion();

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

app.MapHubs();

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