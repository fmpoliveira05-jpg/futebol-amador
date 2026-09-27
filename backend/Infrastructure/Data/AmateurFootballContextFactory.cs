using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data
{
    /// <summary>
    /// Cria o contexto para as ferramentas do EF Core (<c>dotnet ef migrations add</c>,
    /// <c>dotnet ef migrations script</c>) sem arrancar a API, que exige as credenciais do Firebase.
    /// </summary>
    /// <remarks>
    /// A ligação vem da variável de ambiente <c>ConnectionStrings__DefaultConnection</c>; sem ela usa-se
    /// um SQL Server local (só é preciso para <c>database update</c>, não para gerar migrações).
    /// </remarks>
    public sealed class AmateurFootballContextFactory : IDesignTimeDbContextFactory<AmateurFootballContext>
    {
        public AmateurFootballContext CreateDbContext(string[] args)
        {
            var ligacao = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Server=localhost,1433;Database=FutebolAmador;Integrated Security=false;TrustServerCertificate=True";

            var opcoes = new DbContextOptionsBuilder<AmateurFootballContext>()
                .UseSqlServer(ligacao)
                .Options;

            return new AmateurFootballContext(opcoes);
        }
    }
}
