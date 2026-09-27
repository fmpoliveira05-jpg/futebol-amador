using Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Tests.Integration;

namespace Tests.SqlServer
{
    /// <summary>
    /// API de testes com um SQL Server real, para o que a base de dados em memória não simula:
    /// concorrência otimista (<c>rowversion</c>), restrições únicas e transações.
    /// </summary>
    /// <remarks>
    /// Só corre com a variável de ambiente <c>FA_TESTES_SQLSERVER</c> (ligação ao servidor, sem
    /// <c>Database</c>), por exemplo com um SQL Server em Docker:
    /// <code>FA_TESTES_SQLSERVER="Server=localhost,1433;User Id=sa;Password=...;TrustServerCertificate=True" dotnet test --filter Category=SqlServer</code>
    /// Cada instância cria uma base de dados própria, aplica as migrações e apaga-a no fim.
    /// </remarks>
    public sealed class ApiSqlServerFactory : ApiTestAppFactory
    {
        public const string VariavelAmbiente = "FA_TESTES_SQLSERVER";

        public string Ligacao { get; }

        private ApiSqlServerFactory(string ligacao) => Ligacao = ligacao;

        /// <summary>Cria a base de dados com as migrações, ou ignora o teste sem SQL Server configurado.</summary>
        public static async Task<ApiSqlServerFactory> CriarAsync()
        {
            var servidor = Environment.GetEnvironmentVariable(VariavelAmbiente);
            if (string.IsNullOrWhiteSpace(servidor))
            {
                Assert.Ignore($"Sem SQL Server: definir {VariavelAmbiente} para correr estes testes.");
            }

            var ligacao = new SqlConnectionStringBuilder(servidor) { InitialCatalog = $"FaTestes_{Guid.NewGuid():N}" }.ConnectionString;
            var factory = new ApiSqlServerFactory(ligacao);
            await using var db = factory.NovoContexto();
            await db.Database.MigrateAsync();
            return factory;
        }

        protected override void ConfigurarBaseDados(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(Ligacao);
        }

        /// <summary>Contexto independente dos da API (outra "ligação", como outro pedido).</summary>
        public AmateurFootballContext NovoContexto() =>
            new(new DbContextOptionsBuilder<AmateurFootballContext>().UseSqlServer(Ligacao).Options);

        public override async ValueTask DisposeAsync()
        {
            await using (var db = NovoContexto())
            {
                await db.Database.EnsureDeletedAsync();
            }
            await base.DisposeAsync();
        }
    }
}
