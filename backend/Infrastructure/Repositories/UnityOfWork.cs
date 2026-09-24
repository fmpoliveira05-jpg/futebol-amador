using Application.Interfaces.Repositories;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Classe que implementa o padrão Unit of Work (Unidade de Trabalho).
    /// 
    /// É responsável por encapsular o contexto da base de dados ([AmateurFootballContext])
    /// e garantir que uma série de operações (adições, atualizações, remoções) são tratadas
    /// como uma única transação atómica através do método [SaveChangesAsync].
    /// </summary>
    public class UnityOfWork : IUnityOfWork
    {
        /// <summary>
        /// O contexto da base de dados ([AmateurFootballContext]) injetado.
        /// Este objeto rastreia todas as alterações (Changes Tracking) nas entidades.
        /// </summary>
        private readonly AmateurFootballContext context;

        /// <summary>
        /// Construtor da classe [UnityOfWork].
        /// </summary>
        /// <param name="context">O contexto da base de dados (Db Context) injetado via Dependency Injection.</param>
        public UnityOfWork(AmateurFootballContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Persiste todas as alterações pendentes (adições, atualizações, remoções) no contexto da base de dados de forma assíncrona.
        /// 
        /// Este método é o ponto de chamada que inicia a transação no SQL Server.
        /// </summary>
        /// <returns>O número de estados de entidade (registos) que foram guardados na base de dados.</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await context.SaveChangesAsync();
        }
    }
}